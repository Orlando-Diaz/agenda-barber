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

    public async Task<IReadOnlyList<BarberiaEnLista>> ListarReservablesAsync(CancellationToken ct = default) =>
        await db.Barberias.AsNoTracking()
            .Where(b => db.Servicios.Any(s => s.BarberiaId == b.Id && s.Activo)
                     && db.Barberos.Any(x => x.BarberiaId == b.Id && x.Activo && x.Horarios.Any()))
            .OrderBy(b => b.Nombre)
            .Take(100)
            .Select(b => new BarberiaEnLista(
                b.Nombre, b.Slug, b.Telefono, b.Direccion, b.Descripcion, b.FotoActualizadaUtc,
                db.Servicios.Where(s => s.BarberiaId == b.Id && s.Activo).Min(s => (decimal?)s.Precio)))
            .ToListAsync(ct);

    public Task<Barberia?> ObtenerParaEditarPorSlugAsync(string slug, CancellationToken ct = default) =>
        db.Barberias.FirstOrDefaultAsync(b => b.Slug == slug, ct);

    public Task GuardarCambiosAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
