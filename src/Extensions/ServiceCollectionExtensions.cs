using System.Threading.RateLimiting;
using ApiUsuario.Configuration;
using ApiUsuario.Data;
using ApiUsuario.HealthChecks;
using ApiUsuario.Middleware;
using ApiUsuario.Security;
using ApiUsuario.Services;
using ApiUsuario.Swagger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace ApiUsuario.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = ConnectionStringResolver.Resolve(configuration);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                npgsql.CommandTimeout(30);
            }));

        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));
        services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.SectionName));
        services.Configure<RateLimitingSettings>(configuration.GetSection(RateLimitingSettings.SectionName));
        services.Configure<SwaggerSettings>(configuration.GetSection(SwaggerSettings.SectionName));

        // Compatibilidad con las variables de entorno usadas por el despliegue anterior.
        services.PostConfigure<SecurityOptions>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                options.ApiKey = Environment.GetEnvironmentVariable("API_KEY");
            }

            if (string.IsNullOrWhiteSpace(options.AdminApiKey))
            {
                options.AdminApiKey = Environment.GetEnvironmentVariable("ADMIN_API_KEY");
            }

            options.ApiKey = options.ApiKey?.Trim();
            options.AdminApiKey = options.AdminApiKey?.Trim();
        });

        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IUsuarioDetalleService, UsuarioDetalleService>();
        services.AddScoped<IDepartamentoService, DepartamentoService>();
        services.AddScoped<ICiudadService, CiudadService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(entry => entry.Value?.Errors.Count > 0)
                        .ToDictionary(
                            entry => entry.Key,
                            entry => entry.Value!.Errors
                                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                    ? "Valor invalido."
                                    : error.ErrorMessage)
                                .ToArray());

                    var detail = errors.Values
                        .SelectMany(messages => messages)
                        .FirstOrDefault() ?? "La solicitud contiene datos invalidos.";

                    var problem = new ValidationProblemDetails(errors)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Solicitud invalida",
                        Detail = detail,
                        Instance = context.HttpContext.Request.Path
                    };
                    problem.Extensions["mensaje"] = detail;

                    return new BadRequestObjectResult(problem)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        AddCors(services, configuration, environment);
        AddRateLimiting(services, configuration);
        AddSecurity(services);
        AddSwagger(services);

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

        return services;
    }

    private static void AddCors(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var settings = configuration
            .GetSection(CorsSettings.SectionName)
            .Get<CorsSettings>() ?? new CorsSettings();

        var origins = (settings.AllowedOrigins ?? [])
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (origins.Length > 0)
                {
                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("X-Total-Count");
                }
                else if (settings.AllowAnyOrigin || environment.IsDevelopment())
                {
                    // Compatibilidad con el comportamiento anterior. En produccion
                    // se recomienda configurar Cors__AllowedOrigins__0 y desactivar
                    // Cors__AllowAnyOrigin.
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("X-Total-Count");
                }
                else
                {
                    policy.SetIsOriginAllowed(_ => false);
                }
            });
        });
    }

    private static void AddRateLimiting(IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(RateLimitingSettings.SectionName)
            .Get<RateLimitingSettings>() ?? new RateLimitingSettings();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = Math.Max(1, settings.PermitLimit),
                        Window = TimeSpan.FromSeconds(Math.Max(1, settings.WindowSeconds)),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });

            options.AddFixedWindowLimiter("sensitive", limiter =>
            {
                limiter.PermitLimit = Math.Max(1, settings.SensitivePermitLimit);
                limiter.Window = TimeSpan.FromSeconds(Math.Max(1, settings.SensitiveWindowSeconds));
                limiter.QueueLimit = 0;
                limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.HttpContext.Response.HasStarted)
                {
                    return;
                }

                const string detail = "Demasiadas solicitudes. Intente nuevamente en unos segundos.";
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Demasiadas solicitudes",
                    Detail = detail,
                    Instance = context.HttpContext.Request.Path
                };
                problem.Extensions["mensaje"] = detail;

                await context.HttpContext.Response.WriteAsJsonAsync(
                    problem,
                    options: null,
                    contentType: "application/problem+json",
                    cancellationToken: cancellationToken);
            };
        });
    }

    private static void AddSecurity(IServiceCollection services)
    {
        services
            .AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationOptions.DefaultScheme,
                _ => { });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(Policies.Admin, policy => policy.RequireRole("admin"));
        });
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API Usuario",
                Version = "v1",
                Description =
                    "Gestion de usuarios, ubicacion y contrasenas. " +
                    "Todas las rutas requieren el header X-API-KEY."
            });

            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key enviada en el header X-API-KEY.",
                Name = "X-API-KEY",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Oculta la estructura de los DTOs en el documento OpenAPI. Swagger
            // conserva rutas, metodos y parametros para generar las URLs/curl.
            options.DocumentFilter<HideSchemasDocumentFilter>();

            // Incluye los comentarios XML del proyecto en Swagger para documentar
            // parametros y respuestas directamente en la UI.
            var xmlPath = Path.Combine(AppContext.BaseDirectory, "api.xml");

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        });
    }
}
