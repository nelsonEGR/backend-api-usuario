using ApiUsuario.Common.Exceptions;
using ApiUsuario.Dtos;
using ApiUsuario.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiUsuario.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class DepartamentosController(IDepartamentoService departamentoService)
    : ControllerBase
{
    /// <summary>Lista departamentos o consulta uno por ID.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DepartamentoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartamentos(
        [FromQuery] int? id,
        CancellationToken cancellationToken)
    {
        if (id.HasValue)
        {
            var departamento = await departamentoService.GetByIdAsync(id.Value, cancellationToken)
                ?? throw new NotFoundException($"No existe el departamento con ID {id.Value}");

            return Ok(departamento);
        }

        var departamentos = await departamentoService.GetAllAsync(cancellationToken);
        return Ok(departamentos);
    }
}
