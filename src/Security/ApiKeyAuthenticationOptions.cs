using Microsoft.AspNetCore.Authentication;

namespace ApiUsuario.Security;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";

    public string HeaderName { get; set; } = "X-API-KEY";
}
