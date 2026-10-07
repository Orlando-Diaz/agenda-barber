using AgendaBarber.Domain;
using Microsoft.AspNetCore.Diagnostics;

namespace AgendaBarber.Api.Seguridad;

/// <summary>
/// Red de seguridad: cualquier excepción que no capture un controlador termina aquí.
/// Una regla de negocio rota es un 400 con su mensaje; cualquier otra cosa es un 500
/// genérico (el detalle se registra en el log, nunca se le muestra al cliente).
/// </summary>
public sealed class ManejadorDeExcepciones(ILogger<ManejadorDeExcepciones> log) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excepcion, CancellationToken ct)
    {
        if (excepcion is DomainException dominio)
        {
            contexto.Response.StatusCode = StatusCodes.Status400BadRequest;
            await contexto.Response.WriteAsJsonAsync(new { error = dominio.Message }, ct);
            return true;
        }

        log.LogError(excepcion, "Error no controlado en {Metodo} {Ruta}", contexto.Request.Method, contexto.Request.Path);
        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await contexto.Response.WriteAsJsonAsync(new { error = "Ocurrió un error inesperado. Intenta de nuevo en un momento." }, ct);
        return true;
    }
}
