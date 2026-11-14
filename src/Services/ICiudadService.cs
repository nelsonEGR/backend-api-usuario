using ApiUsuario.Dtos;

namespace ApiUsuario.Services;

public interface ICiudadService
{
    Task<IReadOnlyList<CiudadDto>> GetAsync(
        int? idCiudad,
        int? idDepartamento,
        CancellationToken cancellationToken);
}
