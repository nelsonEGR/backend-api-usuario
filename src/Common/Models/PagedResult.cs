namespace ApiUsuario.Common.Models;

/// <summary>Resultado paginado reutilizable.</summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Total,
    int? Page,
    int? PageSize);
