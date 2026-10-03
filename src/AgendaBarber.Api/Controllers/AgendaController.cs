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
