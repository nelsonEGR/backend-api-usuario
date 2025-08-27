using api_usuario.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models; // Necesario para OpenApi*
using TuProyecto.Filters;       // Importa tu filtro ApiKeyAttribute

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// Detectar si estamos en Render
var isRender = Environment.GetEnvironmentVariable("RENDER") == "true";

// 1. Configurar Kestrel SOLO si Render expone PORT
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var portNumber))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(portNumber);
    });
}

// 2. Registrar servicios
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Filtro global de API key
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiKeyAttribute>();
});

builder.Services.AddEndpointsApiExplorer();

// 3. Configuración de Swagger con API Key (solo en Render o Producción)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Usuario", Version = "v1" });

    if (isRender || env.IsProduction())
    {
        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "Ingrese su API Key",
            Name = "X-API-KEY", // nombre exacto del header
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                new List<string>()
            }
        });
    }
});

// 4. Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 5. Construir la app
var app = builder.Build();

// 6. Middleware y Swagger
if (env.IsDevelopment() || isRender)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    });
}

// 7. Habilitar CORS antes de Authorization
app.UseCors();

// 8. Health check en raíz
app.MapGet("/", () => Results.Ok("API corriendo"));

// 9. Autorización y controladores
app.UseAuthorization();
app.MapControllers();

// 10. Arrancar la app
app.Run();