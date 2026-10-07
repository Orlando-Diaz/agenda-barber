using AgendaBarber.Api.Seguridad;
using Microsoft.AspNetCore.Authorization;
using AgendaBarber.Application.Barberos;
using AgendaBarber.Domain;
using AgendaBarber.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBarber.Api.Controllers;

public record CrearBarberoRequest(string Nombre);
public record EditarBarberoRequest(string Nombre);
public record AgregarHorarioRequest(DayOfWeek Dia, TimeOnly Inicio, TimeOnly Fin);

[ApiController]
[Route("api/barberias/{slug}/barberos")]
public class BarberosController(
    CrearBarbero crearBarbero,
    AgregarHorarioBarbero agregarHorario,
    ListarBarberos listarBarberos,
    EditarBarbero editarBarbero,
    QuitarBarbero quitarBarbero,
    QuitarHorarioBarbero quitarHorario) : ControllerBase
{
    private const string NoEncontrado = "No encontramos la barbería o el barbero.";

    [Authorize(Policy = PoliticasDeAcceso.DuenoDeLaBarberia)]
    [HttpPost]
    public async Task<IActionResult> Crear(string slug, CrearBarberoRequest request, CancellationToken ct)
    {
        try
        {
            var barbero = await crearBarbero.EjecutarAsync(slug, request.Nombre, ct);
            if (barbero is null) return NotFound(new { error = NoEncontrado });

            return Created($"/api/barberias/{slug}/barberos", Mostrar(barbero));
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Policy = PoliticasDeAcceso.DuenoDeLaBarberia)]
    [HttpPost("{barberoId:guid}/horarios")]
    public async Task<IActionResult> AgregarHorario(
        string slug, Guid barberoId, AgregarHorarioRequest request, CancellationToken ct)
    {
        try
        {
            var barbero = await agregarHorario.EjecutarAsync(
                slug, barberoId, request.Dia, request.Inicio, request.Fin, ct);
            if (barbero is null) return NotFound(new { error = NoEncontrado });

            return Ok(Mostrar(barbero));
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Policy = PoliticasDeAcceso.DuenoDeLaBarberia)]
    [HttpPut("{barberoId:guid}")]
    public async Task<IActionResult> Editar(string slug, Guid barberoId, EditarBarberoRequest request, CancellationToken ct)
    {
        try
        {
            var barbero = await editarBarbero.EjecutarAsync(slug, barberoId, request.Nombre, ct);
            if (barbero is null) return NotFound(new { error = NoEncontrado });

            return Ok(Mostrar(barbero));
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Policy = PoliticasDeAcceso.DuenoDeLaBarberia)]
    [HttpDelete("{barberoId:guid}")]
    public async Task<IActionResult> Quitar(string slug, Guid barberoId, CancellationToken ct)
    {
        var quitado = await quitarBarbero.EjecutarAsync(slug, barberoId, ct);
        return quitado ? NoContent() : NotFound(new { error = NoEncontrado });
    }

    [Authorize(Policy = PoliticasDeAcceso.DuenoDeLaBarberia)]
    [HttpDelete("{barberoId:guid}/horarios/{horarioId:guid}")]
    public async Task<IActionResult> QuitarHorario(string slug, Guid barberoId, Guid horarioId, CancellationToken ct)
    {
        var barbero = await quitarHorario.EjecutarAsync(slug, barberoId, horarioId, ct);
        if (barbero is null)
            return NotFound(new { error = "No encontramos la barbería, el barbero o el horario." });

        return Ok(Mostrar(barbero));
    }

    [HttpGet]
    public async Task<IActionResult> Listar(string slug, CancellationToken ct)
    {
        var barberos = await listarBarberos.EjecutarAsync(slug, ct);
        if (barberos is null) return NotFound(new { error = "No encontramos esa barbería." });

        return Ok(barberos.Select(Mostrar));
    }

    private static object Mostrar(Barbero b) => new
    {
        b.Id,
        b.Nombre,
        Horarios = b.Horarios
            .OrderBy(h => h.Dia).ThenBy(h => h.Inicio)
            .Select(h => new { h.Id, Dia = (int)h.Dia, NombreDia = h.Dia.ToString(), Inicio = h.Inicio.ToString("HH:mm"), Fin = h.Fin.ToString("HH:mm") })
    };
}
