using ApiUsuario.Common.Errors;

namespace ApiUsuario.Common.Http;

/// <summary>
/// Traduce codigos HTTP sin cuerpo a un mensaje entendible para el cliente.
/// </summary>
public static class StatusCodeProblemCatalog
{
    public static (string Code, string Title, string Detail) Describe(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => (
                ErrorCodes.BadRequest,
                "Solicitud invalida",
                "La solicitud no se pudo interpretar. Verifique el cuerpo y los parametros enviados."),
            StatusCodes.Status401Unauthorized => (
                ErrorCodes.Unauthorized,
                "No autorizado",
                "Debe autenticarse para acceder a este recurso."),
            StatusCodes.Status402PaymentRequired => (
                ErrorCodes.PaymentRequired,
                "Pago requerido",
                "La operacion requiere un plan o pago activo."),
            StatusCodes.Status403Forbidden => (
                ErrorCodes.Forbidden,
                "Acceso denegado",
                "No tiene permisos para realizar esta operacion."),
            StatusCodes.Status404NotFound => (
                ErrorCodes.EndpointNotFound,
                "Endpoint no encontrado",
                "La ruta solicitada no existe. Verifique la URL base y que el despliegue este actualizado."),
            StatusCodes.Status405MethodNotAllowed => (
                ErrorCodes.MethodNotAllowed,
                "Metodo no permitido",
                "El metodo HTTP usado no esta permitido para esta ruta."),
            StatusCodes.Status406NotAcceptable => (
                ErrorCodes.NotAcceptable,
                "Formato no soportado",
                "El formato solicitado no es soportado. Use 'application/json'."),
            StatusCodes.Status408RequestTimeout => (
                ErrorCodes.RequestTimeout,
                "Solicitud agotada",
                "La solicitud tardo demasiado. Intente nuevamente."),
            StatusCodes.Status409Conflict => (
                ErrorCodes.Conflict,
                "Conflicto",
                "La solicitud entra en conflicto con el estado actual de los datos."),
            StatusCodes.Status413PayloadTooLarge => (
                ErrorCodes.PayloadTooLarge,
                "Solicitud demasiado grande",
                "El cuerpo de la solicitud supera el tamano permitido."),
            StatusCodes.Status415UnsupportedMediaType => (
                ErrorCodes.UnsupportedMediaType,
                "Tipo de contenido no soportado",
                "El Content-Type no es soportado. Use 'application/json'."),
            StatusCodes.Status429TooManyRequests => (
                ErrorCodes.RateLimited,
                "Demasiadas solicitudes",
                "Se supero el limite de solicitudes. Intente nuevamente en unos segundos."),
            StatusCodes.Status500InternalServerError => (
                ErrorCodes.InternalError,
                "Error interno",
                "Ocurrio un error inesperado. Intente nuevamente o contacte al administrador."),
            StatusCodes.Status502BadGateway => (
                ErrorCodes.ServiceUnavailable,
                "Servicio no disponible",
                "El servicio no esta disponible en este momento. Intente nuevamente."),
            StatusCodes.Status503ServiceUnavailable => (
                ErrorCodes.ServiceUnavailable,
                "Servicio no disponible",
                "El servicio no esta disponible en este momento. Intente nuevamente."),
            StatusCodes.Status504GatewayTimeout => (
                ErrorCodes.GatewayTimeout,
                "Tiempo de espera agotado",
                "El servicio tardo demasiado en responder. Intente nuevamente."),
            _ => (
                ErrorCodes.HttpError,
                "Error en la solicitud",
                "La solicitud no pudo completarse.")
        };
}
