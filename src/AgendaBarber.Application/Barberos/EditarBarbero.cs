using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberos;

public class EditarBarbero(IBarberiaRepositorio barberias, IBarberoRepositorio barberos)
{
    /// <summary>Cambia el nombre. Devuelve null si la barbería o el barbero no existen (o ya fue quitado).</summary>
    public async Task<Barbero?> EjecutarAsync(string slug, Guid barberoId, string nombre, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        var barbero = await barberos.ObtenerConHorariosAsync(barberia.Id, barberoId, ct);
        if (barbero is null || !barbero.Activo) return null;

        barbero.Renombrar(nombre);
        await barberos.GuardarCambiosAsync(ct);
        return barbero;
    }
}
