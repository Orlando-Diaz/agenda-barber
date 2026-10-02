using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberos;

public class AgregarHorarioBarbero(IBarberiaRepositorio barberias, IBarberoRepositorio barberos)
{
    /// <summary>Agrega un tramo de trabajo. Devuelve null si la barbería o el barbero no existen.</summary>
    public async Task<Barbero?> EjecutarAsync(
        string slug, Guid barberoId, DayOfWeek dia, TimeOnly inicio, TimeOnly fin, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        var barbero = await barberos.ObtenerConHorariosAsync(barberia.Id, barberoId, ct);
        if (barbero is null) return null;

        barbero.AgregarHorario(dia, inicio, fin); // el dominio valida fin > inicio y que no se crucen
        await barberos.GuardarCambiosAsync(ct);
        return barbero;
    }
}
