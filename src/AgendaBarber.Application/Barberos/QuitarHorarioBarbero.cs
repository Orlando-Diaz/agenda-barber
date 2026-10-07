using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberos;

public class QuitarHorarioBarbero(IBarberiaRepositorio barberias, IBarberoRepositorio barberos)
{
    /// <summary>
    /// Quita un tramo de trabajo y devuelve el barbero actualizado. Devuelve null si la barbería, el
    /// barbero o el horario no existen. Las citas ya agendadas en ese tramo no se tocan.
    /// </summary>
    public async Task<Barbero?> EjecutarAsync(string slug, Guid barberoId, Guid horarioId, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        var barbero = await barberos.ObtenerConHorariosAsync(barberia.Id, barberoId, ct);
        if (barbero is null || !barbero.Activo) return null;

        if (!barbero.QuitarHorario(horarioId)) return null;

        await barberos.GuardarCambiosAsync(ct);
        return barbero;
    }
}
