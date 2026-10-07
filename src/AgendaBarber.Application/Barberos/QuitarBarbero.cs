using AgendaBarber.Application.Barberias;

namespace AgendaBarber.Application.Barberos;

public class QuitarBarbero(IBarberiaRepositorio barberias, IBarberoRepositorio barberos)
{
    /// <summary>
    /// Desactiva al barbero: deja de aparecer para reservar, pero sus citas ya hechas se conservan en la
    /// agenda. Devuelve false si la barbería o el barbero no existen (o ya fue quitado).
    /// </summary>
    public async Task<bool> EjecutarAsync(string slug, Guid barberoId, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return false;

        var barbero = await barberos.ObtenerConHorariosAsync(barberia.Id, barberoId, ct);
        if (barbero is null || !barbero.Activo) return false;

        barbero.Desactivar();
        await barberos.GuardarCambiosAsync(ct);
        return true;
    }
}
