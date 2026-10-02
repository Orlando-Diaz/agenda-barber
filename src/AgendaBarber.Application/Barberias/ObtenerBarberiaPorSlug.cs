using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberias;

public class ObtenerBarberiaPorSlug(IBarberiaRepositorio repositorio)
{
    /// <summary>Devuelve la barbería o null si no existe.</summary>
    public Task<Barberia?> EjecutarAsync(string slug, CancellationToken ct = default) =>
        repositorio.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
}
