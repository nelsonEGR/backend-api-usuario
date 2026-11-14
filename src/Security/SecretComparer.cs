using System.Security.Cryptography;
using System.Text;

namespace ApiUsuario.Security;

/// <summary>
/// Compara secretos en tiempo constante para evitar ataques de temporizacion.
/// </summary>
public static class SecretComparer
{
    public static bool Matches(string? expected, string? provided)
    {
        if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(provided))
        {
            return false;
        }

        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(provided));

        return CryptographicOperations.FixedTimeEquals(expectedHash, providedHash);
    }
}
