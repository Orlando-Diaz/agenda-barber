using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBarber.Api.Controllers;

public record CrearBarberiaRequest(string Nombre, string Slug, string? Telefono);

[ApiController]
[Route("api/barberias")]
public class BarberiasController(
    CrearBarberia crearBarberia,
    ObtenerBarberiaPorSlug obtenerBarberiaPorSlug) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Crear(CrearBarberiaRequest request, CancellationToken ct)
    {
        try
        {
            var barberia = await crearBarberia.EjecutarAsync(request.Nombre, request.Slug, request.Telefono, ct);
            return Created($"/api/barberias/{barberia.Slug}", new { barberia.Id, barberia.Nombre, barberia.Slug });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> ObtenerPorSlug(string slug, CancellationToken ct)
    {
        var barberia = await obtenerBarberiaPorSlug.EjecutarAsync(slug, ct);
        if (barberia is null)
            return NotFound(new { error = "No encontramos esa barbería." });

        return Ok(new { barberia.Id, barberia.Nombre, barberia.Slug, barberia.Telefono });
    }
}
