using ApiUsuario.Common.Errors;
using Microsoft.IdentityModel.Tokens;

namespace ApiUsuario.Security;

/// <summary>
/// Describe el motivo por el cual fallo la autenticacion con el access token.
/// </summary>
public static class AuthFailureMessages
{
    private const string CodeItemKey = "Auth.Failure.Code";
    private const string DetailItemKey = "Auth.Failure.Detail";

    public static void Set(HttpContext context, string code, string detail)
    {
        context.Items[CodeItemKey] = code;
        context.Items[DetailItemKey] = detail;
    }

    public static (string Code, string Detail) Describe(Exception exception) =>
        exception switch
        {
            SecurityTokenExpiredException => (
                ErrorCodes.TokenExpired,
                "El access token expiro. Solicite uno nuevo en 'POST /api/auth/token'."),
            SecurityTokenInvalidSignatureException => (
                ErrorCodes.TokenInvalid,
                "La firma del access token no es valida."),
            SecurityTokenInvalidIssuerException => (
                ErrorCodes.TokenInvalid,
                "El emisor del access token no es valido."),
            SecurityTokenInvalidAudienceException => (
                ErrorCodes.TokenInvalid,
                "La audiencia del access token no es valida."),
            SecurityTokenNotYetValidException => (
                ErrorCodes.TokenInvalid,
                "El access token aun no es valido."),
            SecurityTokenMalformedException => (
                ErrorCodes.TokenMalformed,
                "El formato del access token no es valido."),
            _ => (
                ErrorCodes.TokenInvalid,
                "El access token es invalido.")
        };

    public static (string Code, string Detail) Resolve(HttpContext context)
    {
        if (context.Items.TryGetValue(CodeItemKey, out var code)
            && code is string codeValue
            && context.Items.TryGetValue(DetailItemKey, out var detail)
            && detail is string detailValue)
        {
            return (codeValue, detailValue);
        }

        var header = context.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(header))
        {
            return (
                ErrorCodes.TokenMissing,
                "Falta el access token. Envie la cabecera 'Authorization: Bearer {accessToken}'.");
        }

        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return (
                ErrorCodes.TokenMalformed,
                "La cabecera Authorization debe usar el esquema 'Bearer'.");
        }

        var token = header["Bearer ".Length..].Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            return (
                ErrorCodes.TokenMissing,
                "La cabecera Authorization no incluye el access token.");
        }

        return (
            ErrorCodes.TokenInvalid,
            "El access token es invalido o esta expirado.");
    }
}
