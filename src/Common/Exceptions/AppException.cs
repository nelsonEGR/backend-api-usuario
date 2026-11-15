namespace ApiUsuario.Common.Exceptions;

/// <summary>Error de negocio que se traduce a una respuesta HTTP controlada.</summary>
public abstract class AppException(string message) : Exception(message);

public sealed class BadRequestException(string message) : AppException(message);

public sealed class UnauthorizedException(string message) : AppException(message);

public sealed class ForbiddenException(string message) : AppException(message);

public sealed class NotFoundException(string message) : AppException(message);

public sealed class ConflictException(string message) : AppException(message);
