using AgendaBarber.Api.Seguridad;
using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain;
using AgendaBarber.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBarber.Api.Controllers;

public record PerfilRequest(string? Direccion, string? Descripcion);

[ApiController]
[Route("api/barberias")]
public class BarberiasController(
    ObtenerBarberiaPorSlug obtenerBarberiaPorSlug,
    ListarBarberias listarBarberias,
    ActualizarPerfilBarberia actualizarPerfil,
    GuardarFotoBarberia guardarFoto,
    QuitarFotoBarberia quitarFoto,
    ObtenerFotoBarberia obtenerFoto) : ControllerBase
{
    /// <summary>Directorio público: las barberías que ya reciben reservas.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var barberias = await listarBarberias.EjecutarAsync(ct);
        return Ok(barberias.Select(b => new
        {
            b.Nombre,
            b.Slug,
            b.Telefono,
            b.Direccion,
            b.Descripcion,
            FotoVersion = Version(b.FotoActualizadaUtc),
            b.PrecioDesde,
        }));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> ObtenerPorSlug(string slug, CancellationToken ct)
    {
        var barberia = await obtenerBarberiaPorSlug.EjecutarAsync(slug, ct);
        if (barberia is null)
            return NotFound(new { error = "No encontramos esa barbería." });

        return Ok(Mostrar(barberia));
    }

    /// <summary>La foto de portada (pública). La web la pide con ?v=versión para refrescarla cuando cambia.</summary>
    [HttpGet("{slug}/foto")]
    public async Task<IActionResult> Foto(string slug, CancellationToken ct)
    {
        var foto = await obtenerFoto.EjecutarAsync(slug, ct);
        if (foto is null) return NotFound();

        Response.Headers.CacheControl = "public, max-age=86400";
        return File(foto.Datos, foto.Tipo);
    }

    [Authorize(Policy = PoliticasDeAcceso.DuenoDeLaBarberia)]
    [HttpPut("{slug}/perfil")]
    public async Task<IActionResult> Perfil(string slug, PerfilRequest request, CancellationToken ct)
    {
        try
        {
            var barberia = await actualizarPerfil.EjecutarAsync(slug, request.Direccion, request.Descripcion, ct);
            return barberia is null ? NotFound(new { error = "No encontramos esa barbería." }) : Ok(Mostrar(barberia));
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>El cuerpo de la petición es la imagen tal cual (image/jpeg, image/png o image/webp).</summary>
    [Authorize(Policy = PoliticasDeAcceso.DuenoDeLaBarberia)]
    [HttpPut("{slug}/foto")]
    [RequestSizeLimit(FotoBarberia.TamanoMaximo + 1024)]
    public async Task<IActionResult> GuardarFoto(string slug, CancellationToken ct)
    {
        var tipo = Request.ContentType?.Split(';')[0].Trim().ToLowerInvariant() ?? "";
        using var memoria = new MemoryStream();
        await Request.Body.CopyToAsync(memoria, ct);

        try
        {
            var existe = await guardarFoto.EjecutarAsync(slug, memoria.ToArray(), tipo, ct);
            if (!existe) return NotFound(new { error = "No encontramos esa barbería." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        var barberia = await obtenerBarberiaPorSlug.EjecutarAsync(slug, ct);
        return Ok(Mostrar(barberia!));
    }

    [Authorize(Policy = PoliticasDeAcceso.DuenoDeLaBarberia)]
    [HttpDelete("{slug}/foto")]
    public async Task<IActionResult> QuitarFoto(string slug, CancellationToken ct) =>
        await quitarFoto.EjecutarAsync(slug, ct) ? NoContent() : NotFound(new { error = "No encontramos esa barbería." });

    private static long? Version(DateTime? fecha) => fecha is null ? null : new DateTimeOffset(fecha.Value, TimeSpan.Zero).ToUnixTimeSeconds();

    private static object Mostrar(Barberia b) => new
    {
        b.Id,
        b.Nombre,
        b.Slug,
        b.Telefono,
        b.Direccion,
        b.Descripcion,
        FotoVersion = Version(b.FotoActualizadaUtc),
    };
}
