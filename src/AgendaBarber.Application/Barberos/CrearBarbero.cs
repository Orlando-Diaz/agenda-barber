using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberos;

public class CrearBarbero(IBarberiaRepositorio barberias, IBarberoRepositorio barberos)
{
    /// <summary>Crea el barbero. Devuelve null si la barbería no existe.</summary>
    public async Task<Barbero?> EjecutarAsync(string slug, string nombre, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        var barbero = new Barbero(barberia.Id, nombre);
        await barberos.AgregarAsync(barbero, ct);
        return barbero;
    }
}
