using AgendaBarber.Application.Citas;
using AgendaBarber.Domain;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBarber.Api.Controllers;

public record ReservarCitaRequest(
    Guid BarberoId, Guid ServicioId, DateTime InicioUtc, string ClienteNombre, string ClienteTelefono);

[ApiController]
[Route("api/barberias/{slug}/citas")]
public class CitasController(ReservarCita reservarCita, CambiarEstadoCita cambiarEstado) : ControllerBase
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

    [HttpPost("{citaId:guid}/confirmar")]
    public Task<IActionResult> Confirmar(string slug, Guid citaId, CancellationToken ct) =>
        Cambiar(slug, citaId, AccionCita.Confirmar, ct);

    [HttpPost("{citaId:guid}/cancelar")]
    public Task<IActionResult> Cancelar(string slug, Guid citaId, CancellationToken ct) =>
        Cambiar(slug, citaId, AccionCita.Cancelar, ct);

    [HttpPost("{citaId:guid}/atendida")]
    public Task<IActionResult> Atendida(string slug, Guid citaId, CancellationToken ct) =>
        Cambiar(slug, citaId, AccionCita.Atendida, ct);

    [HttpPost("{citaId:guid}/no-asistio")]
    public Task<IActionResult> NoAsistio(string slug, Guid citaId, CancellationToken ct) =>
        Cambiar(slug, citaId, AccionCita.NoAsistio, ct);

    private async Task<IActionResult> Cambiar(string slug, Guid citaId, AccionCita accion, CancellationToken ct)
    {
        try
        {
            var cita = await cambiarEstado.EjecutarAsync(slug, citaId, accion, ct);
            if (cita is null)
                return NotFound(new { error = "No encontramos la barbería o la cita." });

            return Ok(new { cita.Id, Estado = cita.Estado.ToString() });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
