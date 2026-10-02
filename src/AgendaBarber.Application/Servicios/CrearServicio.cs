using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Servicios;

public class CrearServicio(IBarberiaRepositorio barberias, IServicioRepositorio servicios)
{
    /// <summary>Crea el servicio. Devuelve null si la barbería no existe.</summary>
    public async Task<Servicio?> EjecutarAsync(
        string slug, string nombre, int duracionMinutos, decimal precio, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        var servicio = new Servicio(barberia.Id, nombre, duracionMinutos, precio);
        await servicios.AgregarAsync(servicio, ct);
        return servicio;
    }
}
