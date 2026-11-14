namespace ApiUsuario.Configuration;

/// <summary>
/// Configuracion de Swagger. Se enlaza desde la seccion "Swagger".
/// </summary>
public sealed class SwaggerSettings
{
    public const string SectionName = "Swagger";

    /// <summary>Si es null, Swagger solo se habilita en Development.</summary>
    public bool? Enabled { get; set; }

    public bool IsEnabled(IHostEnvironment environment) =>
        Enabled ?? environment.IsDevelopment();
}
