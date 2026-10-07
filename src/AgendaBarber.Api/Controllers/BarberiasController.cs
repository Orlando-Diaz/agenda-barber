using AgendaBarber.Application.Barberias;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBarber.Api.Controllers;

[ApiController]
[Route("api/barberias")]
public class BarberiasController(ObtenerBarberiaPorSlug obtenerBarberiaPorSlug, ListarBarberias listarBarberias) : ControllerBase
{
    /// <summary>Directorio público: las barberías que ya reciben reservas.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var barberias = await listarBarberias.EjecutarAsync(ct);
        return Ok(barberias.Select(b => new { b.Nombre, b.Slug, b.Telefono }));
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
