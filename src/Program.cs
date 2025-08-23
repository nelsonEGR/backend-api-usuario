using api_usuario.Data;
using Microsoft.EntityFrameworkCore;
using TuProyecto.Filters; // 👈 Importa tu filtro ApiKeyAttribute

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

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

// 👇 Aquí agregamos el filtro global de API key
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiKeyAttribute>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agrego el servicio CORS aquí
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 3. Construir la app
var app = builder.Build();

// 4. Middleware y Swagger en Development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHttpsRedirection();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    });
}

// Habilito el middleware CORS aquí
app.UseCors();

// 5. Health check en raíz
app.MapGet("/", () => Results.Ok("API corriendo 🎉"));

// 6. Autorización y controladores
app.UseAuthorization();
app.MapControllers();

// 7. Arrancar la app
app.Run();