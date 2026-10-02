using AgendaBarber.Application.Citas;
using AgendaBarber.Domain;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBarber.Api.Controllers;

public record ReservarCitaRequest(
    Guid BarberoId, Guid ServicioId, DateTime InicioUtc, string ClienteNombre, string ClienteTelefono);

[ApiController]
[Route("api/barberias/{slug}/citas")]
public class CitasController(ReservarCita reservarCita) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Reservar(string slug, ReservarCitaRequest request, CancellationToken ct)
    {
        try
        {
            var cita = await reservarCita.EjecutarAsync(
                slug, request.BarberoId, request.ServicioId, request.InicioUtc,
                request.ClienteNombre, request.ClienteTelefono, ct);

            if (cita is null)
                return NotFound(new { error = "No encontramos la barbería, el barbero o el servicio." });

            return Created($"/api/barberias/{slug}/citas/{cita.Id}",
                new { cita.Id, cita.InicioUtc, cita.FinUtc, Estado = cita.Estado.ToString(), cita.Precio });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (HoraNoDisponibleException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}
