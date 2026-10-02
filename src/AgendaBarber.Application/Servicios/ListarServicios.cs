using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Servicios;

public class ListarServicios(IBarberiaRepositorio barberias, IServicioRepositorio servicios)
{
    /// <summary>Servicios activos de la barbería. Devuelve null si la barbería no existe.</summary>
    public async Task<IReadOnlyList<Servicio>?> EjecutarAsync(string slug, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        return await servicios.ListarActivosAsync(barberia.Id, ct);
    }
}
