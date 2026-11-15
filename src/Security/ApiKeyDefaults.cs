namespace ApiUsuario.Security;

/// <summary>
/// Nombres y claves compartidas para la validacion de la API key enviada en
/// la cabecera 'X-API-KEY'.
/// </summary>
public static class ApiKeyDefaults
{
    public const string HeaderName = "X-API-KEY";

    /// <summary>
    /// Clave usada en HttpContext.Items para indicar si la API key recibida
    /// corresponde a la llave administrativa.
    /// </summary>
    public const string IsAdminItemKey = "ApiKey.IsAdmin";
}
