using AgendaBarber.Application.Disponibilidad;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBarber.Api.Controllers;

[ApiController]
[Route("api/barberias/{slug}/disponibilidad")]
public class DisponibilidadController(ConsultarDisponibilidad consultarDisponibilidad) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Consultar(
        string slug, [FromQuery] Guid barberoId, [FromQuery] Guid servicioId, [FromQuery] DateOnly fecha,
        CancellationToken ct)
    {
        var horas = await consultarDisponibilidad.EjecutarAsync(slug, barberoId, servicioId, fecha, ct);
        if (horas is null)
            return NotFound(new { error = "No encontramos la barbería, el barbero o el servicio." });

        return Ok(horas.Select(h => new { h.InicioUtc, Hora = h.InicioLocal.ToString("HH:mm") }));
    }
}
