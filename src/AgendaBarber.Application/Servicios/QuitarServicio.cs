using AgendaBarber.Application.Barberias;

namespace AgendaBarber.Application.Servicios;

public class QuitarServicio(IBarberiaRepositorio barberias, IServicioRepositorio servicios)
{
    /// <summary>
    /// Desactiva el servicio: deja de ofrecerse, pero no se borra, porque las citas ya hechas lo siguen
    /// mostrando en la agenda. Devuelve false si la barbería o el servicio no existen.
    /// </summary>
    public async Task<bool> EjecutarAsync(string slug, Guid servicioId, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return false;

        var servicio = await servicios.ObtenerParaEditarAsync(barberia.Id, servicioId, ct);
        if (servicio is null) return false;

        servicio.Desactivar();
        await servicios.GuardarCambiosAsync(ct);
        return true;
    }
}
