using AgendaBarber.Application.Barberias;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBarber.Api.Controllers;

[ApiController]
[Route("api/barberias")]
public class BarberiasController(ObtenerBarberiaPorSlug obtenerBarberiaPorSlug) : ControllerBase
{
    [HttpGet("{slug}")]
    public async Task<IActionResult> ObtenerPorSlug(string slug, CancellationToken ct)
    {
        var barberia = await obtenerBarberiaPorSlug.EjecutarAsync(slug, ct);
        if (barberia is null)
            return NotFound(new { error = "No encontramos esa barbería." });

        return Ok(new { barberia.Id, barberia.Nombre, barberia.Slug, barberia.Telefono });
    }
}
