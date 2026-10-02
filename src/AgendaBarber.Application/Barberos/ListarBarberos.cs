using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberos;

public class ListarBarberos(IBarberiaRepositorio barberias, IBarberoRepositorio barberos)
{
    /// <summary>Barberos activos con sus horarios. Devuelve null si la barbería no existe.</summary>
    public async Task<IReadOnlyList<Barbero>?> EjecutarAsync(string slug, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        return await barberos.ListarActivosAsync(barberia.Id, ct);
    }
}
