using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Servicios;

public class EditarServicio(IBarberiaRepositorio barberias, IServicioRepositorio servicios)
{
    /// <summary>Cambia nombre, duración y precio. Devuelve null si la barbería o el servicio no existen.</summary>
    public async Task<Servicio?> EjecutarAsync(
        string slug, Guid servicioId, string nombre, int duracionMinutos, decimal precio, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        var servicio = await servicios.ObtenerParaEditarAsync(barberia.Id, servicioId, ct);
        if (servicio is null) return null;

        servicio.Editar(nombre, duracionMinutos, precio); // el dominio valida las mismas reglas que al crear
        await servicios.GuardarCambiosAsync(ct);
        return servicio;
    }
}
