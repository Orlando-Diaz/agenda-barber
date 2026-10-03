using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgendaBarber.Infrastructure.Persistence;

public class BarberiaRepositorio(AgendaDbContext db) : IBarberiaRepositorio
{
    public Task<bool> ExisteSlugAsync(string slug, CancellationToken ct = default) =>
        db.Barberias.AnyAsync(b => b.Slug == slug, ct);

    public async Task AgregarAsync(Barberia barberia, CancellationToken ct = default)
    {
        db.Barberias.Add(barberia);
        await db.SaveChangesAsync(ct);
    }

    public Task<Barberia?> ObtenerPorSlugAsync(string slug, CancellationToken ct = default) =>
        db.Barberias.AsNoTracking().FirstOrDefaultAsync(b => b.Slug == slug, ct);

    public Task<Barberia?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Barberias.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct);
}
