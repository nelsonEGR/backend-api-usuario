namespace ApiUsuario.Common.Http;

/// <summary>
/// Normaliza los errores de cliente a 400, conservando los codigos que tienen
/// un significado especial para autenticacion, permisos, rutas y rate limiting.
/// El 404 se reserva exclusivamente para endpoints que no existen.
/// </summary>
public static class ClientErrorStatus
{
    public static int Normalize(int statusCode)
    {
        if (statusCode is < 400 or >= 500)
        {
            return statusCode;
        }

        return statusCode switch
        {
            StatusCodes.Status400BadRequest => statusCode,
            StatusCodes.Status401Unauthorized => statusCode,
            StatusCodes.Status402PaymentRequired => statusCode,
            StatusCodes.Status403Forbidden => statusCode,
            StatusCodes.Status404NotFound => statusCode,
            StatusCodes.Status429TooManyRequests => statusCode,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
