using ApiUsuario.Dtos;

namespace ApiUsuario.Services;

public interface IDepartamentoService
{
    Task<IReadOnlyList<DepartamentoDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<DepartamentoDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
