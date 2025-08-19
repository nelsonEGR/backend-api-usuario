using api_usuario.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Kestrel para el puerto que asigna Render
var portEnv = Environment.GetEnvironmentVariable("PORT") ?? "8080";
if (!int.TryParse(portEnv, out var portNumber))
    portNumber = 8080;
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(portNumber);
});

// 2. Registrar servicios
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// 5. Health check en raíz para que Render confirme que la app está viva
app.MapGet("/", () => Results.Ok("API corriendo 🎉"));

// 6. Autorización y controladores
app.UseAuthorization();
app.MapControllers();

// 7. Arrancar la app
app.Run();