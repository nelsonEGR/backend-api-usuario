using ApiUsuario.Dtos;
using ApiUsuario.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiUsuario.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class CiudadesController(ICiudadService ciudadService) : ControllerBase
{
    /// <summary>Lista ciudades, opcionalmente filtradas por ciudad o departamento.</summary>
    /// <remarks>
    /// Sin filtros devuelve todas las ciudades.
    /// </remarks>
    /// <param name="idCiudad">Filtra por identificador de ciudad (opcional).</param>
    /// <param name="idDepartamento">Filtra por identificador de departamento (opcional).</param>
    /// <param name="cancellationToken">Token de cancelacion de la peticion.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CiudadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetCiudades(
        [FromQuery(Name = "id_ciudad")] int? idCiudad,
        [FromQuery(Name = "id_departamento")] int? idDepartamento,
        CancellationToken cancellationToken)
    {
        var ciudades = await ciudadService.GetAsync(idCiudad, idDepartamento, cancellationToken);

        return ciudades.Count == 0 ? NoContent() : Ok(ciudades);
    }
}
