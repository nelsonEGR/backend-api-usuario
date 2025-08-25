using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/[controller]")]
public class ConsultaSqlController : ControllerBase
{
    private readonly string _connectionString;

    public ConsultaSqlController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no configurada.");
    }

    [HttpPost]
    public async Task<IActionResult> EjecutarSelect([FromBody] string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return BadRequest("La consulta no puede estar vacía.");

        // 1️⃣ Eliminar comentarios
        string sinComentarios = Regex.Replace(sql, @"--.*?$", "", RegexOptions.Multiline);
        sinComentarios = Regex.Replace(sinComentarios, @"/\*.*?\*/", "", RegexOptions.Singleline);

        var consulta = sinComentarios.Trim();

        // 2️⃣ Quitar punto y coma final
        if (consulta.EndsWith(";"))
            consulta = consulta[..^1].Trim();

        // 3️⃣ Rechazar múltiples sentencias
        if (consulta.Contains(";"))
            return BadRequest("No se permiten múltiples sentencias en la consulta.");

        // 4️⃣ Validar que comience con SELECT
        if (!consulta.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Solo se permiten consultas SELECT.");

        // 5️⃣ Palabras prohibidas
        var prohibidas = new[] { "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "TRUNCATE", "CREATE" };
        if (prohibidas.Any(p => consulta.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0))
            return BadRequest("La consulta contiene palabras no permitidas.");

        // 6️⃣ Forzar que solo devuelva 1 registro
        consulta = Regex.Replace(consulta, @"\bLIMIT\s+\d+\b", "", RegexOptions.IgnoreCase); // quitar cualquier LIMIT existente
        consulta += " LIMIT 1";

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var resultado = (await connection.QueryAsync<dynamic>(consulta)).FirstOrDefault();

            if (resultado == null)
                return BadRequest("La consulta no devolvió resultados.");

            return Ok(resultado);
        }
        // Errores específicos de PostgreSQL
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
        // Errores generales de conexión u otros de Npgsql
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