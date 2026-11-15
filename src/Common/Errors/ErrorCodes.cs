namespace ApiUsuario.Common.Errors;

/// <summary>
/// Codigos estables que identifican el motivo del error. Se incluyen en la
/// respuesta JSON (propiedad "codigo") y en los logs para facilitar busquedas.
/// </summary>
public static class ErrorCodes
{
    public const string BadRequest = "BAD_REQUEST";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string PaymentRequired = "PAYMENT_REQUIRED";
    public const string Forbidden = "FORBIDDEN";
    public const string ForbiddenRole = "FORBIDDEN_ROLE";
    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
    public const string EndpointNotFound = "ENDPOINT_NOT_FOUND";
    public const string MethodNotAllowed = "METHOD_NOT_ALLOWED";
    public const string NotAcceptable = "NOT_ACCEPTABLE";
    public const string RequestTimeout = "REQUEST_TIMEOUT";
    public const string Conflict = "CONFLICT";
    public const string PayloadTooLarge = "PAYLOAD_TOO_LARGE";
    public const string UnsupportedMediaType = "UNSUPPORTED_MEDIA_TYPE";
    public const string RateLimited = "RATE_LIMITED";
    public const string InternalError = "INTERNAL_ERROR";
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    public const string GatewayTimeout = "GATEWAY_TIMEOUT";
    public const string HttpError = "HTTP_ERROR";

    public const string ApiKeyMissing = "API_KEY_MISSING";
    public const string ApiKeyInvalid = "API_KEY_INVALID";
    public const string TokenMissing = "TOKEN_MISSING";
    public const string TokenInvalid = "TOKEN_INVALID";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string TokenMalformed = "TOKEN_MALFORMED";
}
