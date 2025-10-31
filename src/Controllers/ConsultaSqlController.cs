using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/internal/consulta-sql-privada")] //  Ruta personalizada
[ApiExplorerSettings(IgnoreApi = true)]      //  Oculta este controlador de Swagger
public class ConsultaSqlController : ControllerBase
{
    private readonly string _connectionString;

    public ConsultaSqlController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no configurada.");
    }

    [HttpPost]
    [Consumes("text/plain")] // 👈 acepta texto plano
    public async Task<IActionResult> EjecutarSelect()
    {
        // Leer el body crudo como texto plano
        using var reader = new StreamReader(Request.Body);
        var sql = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(sql))
            return BadRequest("La consulta no puede estar vacía.");

        //  Eliminar comentarios
        string sinComentarios = Regex.Replace(sql, @"--.*?$", "", RegexOptions.Multiline);
        sinComentarios = Regex.Replace(sinComentarios, @"/\*.*?\*/", "", RegexOptions.Singleline);

        var consulta = sinComentarios.Trim();

        //  Quitar punto y coma final
        if (consulta.EndsWith(";"))
            consulta = consulta[..^1].Trim();

        //  Validar que comience con SELECT
        if (!consulta.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Solo se permiten consultas SELECT.");

        // Palabras prohibidas
        var prohibidas = new[] { "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "TRUNCATE", "CREATE" };
        if (prohibidas.Any(p => consulta.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0))
            return BadRequest("La consulta contiene palabras no permitidas.");

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var resultado = await connection.QueryAsync<dynamic>(consulta);

            if (!resultado.Any())
                return BadRequest("La consulta no devolvió resultados.");

            return Ok(resultado);

        }
        catch (PostgresException ex) when (ex.SqlState == "42P01") // undefined_table
        {
            return BadRequest($"La tabla especificada no existe: {ex.MessageText}");
        }
        catch (PostgresException ex) when (ex.SqlState == "42703") // undefined_column
        {
            return BadRequest($"La columna especificada no existe: {ex.MessageText}");
        }
        catch (PostgresException ex) when (ex.SqlState == "42601") // syntax_error
        {
            return BadRequest($"Error de sintaxis en la consulta: {ex.MessageText}");
        }
        catch (PostgresException ex) // otros errores de PostgreSQL
        {
            return BadRequest($"Error en la consulta: {ex.MessageText}");
        }
        catch (NpgsqlException ex)
        {
            return BadRequest($"No se pudo conectar a la base de datos: {ex.Message}");
        }
        catch (TimeoutException)
        {
            return BadRequest("Tiempo de espera agotado al intentar conectar a la base de datos.");
        }
    }
}