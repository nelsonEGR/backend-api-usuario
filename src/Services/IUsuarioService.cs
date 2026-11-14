using ApiUsuario.Common.Models;
using ApiUsuario.Dtos;

namespace ApiUsuario.Services;

public interface IUsuarioService
{
    Task<PagedResult<UsuarioDto>> GetAllAsync(int? page, int? pageSize, CancellationToken cancellationToken);

    Task<UsuarioDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto, CancellationToken cancellationToken);

    Task UpdateAsync(int id, UsuarioUpdateDto dto, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);

    Task<UsuarioDto> ActivateAsync(int id, CancellationToken cancellationToken);

    Task<UsuarioDto> DeactivateAsync(int id, CancellationToken cancellationToken);
}
