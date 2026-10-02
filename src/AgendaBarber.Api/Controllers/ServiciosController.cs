using AgendaBarber.Application.Servicios;
using AgendaBarber.Domain;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBarber.Api.Controllers;

public record CrearServicioRequest(string Nombre, int DuracionMinutos, decimal Precio);

[ApiController]
[Route("api/barberias/{slug}/servicios")]
public class ServiciosController(CrearServicio crearServicio, ListarServicios listarServicios) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Crear(string slug, CrearServicioRequest request, CancellationToken ct)
    {
        try
        {
            var servicio = await crearServicio.EjecutarAsync(
                slug, request.Nombre, request.DuracionMinutos, request.Precio, ct);

            if (servicio is null)
                return NotFound(new { error = "No encontramos esa barbería." });

            return Created($"/api/barberias/{slug}/servicios",
                new { servicio.Id, servicio.Nombre, servicio.DuracionMinutos, servicio.Precio });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Listar(string slug, CancellationToken ct)
    {
        var servicios = await listarServicios.EjecutarAsync(slug, ct);
        if (servicios is null)
            return NotFound(new { error = "No encontramos esa barbería." });

        return Ok(servicios.Select(s => new { s.Id, s.Nombre, s.DuracionMinutos, s.Precio }));
    }
}
