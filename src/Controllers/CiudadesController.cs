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
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CiudadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCiudades(
        [FromQuery(Name = "id_ciudad")] int? idCiudad,
        [FromQuery(Name = "id_departamento")] int? idDepartamento,
        CancellationToken cancellationToken)
    {
        var ciudades = await ciudadService.GetAsync(idCiudad, idDepartamento, cancellationToken);

        return Ok(ciudades);
    }
}
