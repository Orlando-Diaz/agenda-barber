using AgendaBarber.Api.Seguridad;
using Microsoft.AspNetCore.Authorization;
using AgendaBarber.Application.Citas;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBarber.Api.Controllers;

[Authorize(Policy = PoliticasDeAcceso.DuenoDeLaBarberia)]
[ApiController]
[Route("api/barberias/{slug}/agenda")]
public class AgendaController(ConsultarAgenda consultarAgenda) : ControllerBase
{
    /// <summary>Todas las citas desde hoy y los próximos días, para ver de un vistazo quién viene.</summary>
    [HttpGet("proximas")]
    public async Task<IActionResult> Proximas(string slug, [FromQuery] int dias = 14, CancellationToken ct = default)
    {
        var citas = await consultarAgenda.ProximasAsync(slug, dias, ct);
        if (citas is null)
            return NotFound(new { error = "No encontramos esa barbería." });

        return Ok(citas.Select(p => new
        {
            Fecha = p.Fecha.ToString("yyyy-MM-dd"),
            p.Cita.Id,
            HoraInicio = p.Cita.HoraInicio.ToString("HH:mm"),
            HoraFin = p.Cita.HoraFin.ToString("HH:mm"),
            p.Cita.Barbero,
            p.Cita.Servicio,
            p.Cita.ClienteNombre,
            p.Cita.ClienteTelefono,
            p.Cita.Precio,
            p.Cita.Estado,
        }));
    }

    [HttpGet]
    public async Task<IActionResult> Consultar(string slug, [FromQuery] DateOnly fecha, CancellationToken ct)
    {
        var citas = await consultarAgenda.EjecutarAsync(slug, fecha, ct);
        if (citas is null)
            return NotFound(new { error = "No encontramos esa barbería." });

        return Ok(citas.Select(c => new
        {
            c.Id,
            HoraInicio = c.HoraInicio.ToString("HH:mm"),
            HoraFin = c.HoraFin.ToString("HH:mm"),
            c.Barbero,
            c.Servicio,
            c.ClienteNombre,
            c.ClienteTelefono,
            c.Precio,
            c.Estado,
        }));
    }
}
