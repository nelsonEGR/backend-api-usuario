namespace ApiUsuario.Security;

/// <summary>Datos del access token emitido.</summary>
public sealed record TokenResult(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    DateTimeOffset ExpiresAt);

/// <summary>Emite access tokens JWT para clientes autenticados.</summary>
public interface ITokenService
{
    TokenResult CreateAccessToken(string clientId, bool isAdmin);
}
