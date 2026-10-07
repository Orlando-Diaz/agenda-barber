using AgendaBarber.Api.Seguridad;
using Microsoft.AspNetCore.Authorization;
using AgendaBarber.Application.Servicios;
using AgendaBarber.Domain;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBarber.Api.Controllers;

public record CrearServicioRequest(string Nombre, int DuracionMinutos, decimal Precio);
public record EditarServicioRequest(string Nombre, int DuracionMinutos, decimal Precio);

[ApiController]
[Route("api/barberias/{slug}/servicios")]
public class ServiciosController(
    CrearServicio crearServicio,
    ListarServicios listarServicios,
    EditarServicio editarServicio,
    QuitarServicio quitarServicio) : ControllerBase
{
    [Authorize(Policy = PoliticasDeAcceso.DuenoDeLaBarberia)]
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

    [Authorize(Policy = PoliticasDeAcceso.DuenoDeLaBarberia)]
    [HttpPut("{servicioId:guid}")]
    public async Task<IActionResult> Editar(string slug, Guid servicioId, EditarServicioRequest request, CancellationToken ct)
    {
        try
        {
            var servicio = await editarServicio.EjecutarAsync(
                slug, servicioId, request.Nombre, request.DuracionMinutos, request.Precio, ct);

            if (servicio is null)
                return NotFound(new { error = "No encontramos la barbería o el servicio." });

            return Ok(new { servicio.Id, servicio.Nombre, servicio.DuracionMinutos, servicio.Precio });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Policy = PoliticasDeAcceso.DuenoDeLaBarberia)]
    [HttpDelete("{servicioId:guid}")]
    public async Task<IActionResult> Quitar(string slug, Guid servicioId, CancellationToken ct)
    {
        var quitado = await quitarServicio.EjecutarAsync(slug, servicioId, ct);
        return quitado ? NoContent() : NotFound(new { error = "No encontramos la barbería o el servicio." });
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
