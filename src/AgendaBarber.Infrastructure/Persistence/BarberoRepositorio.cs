using AgendaBarber.Application.Barberos;
using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgendaBarber.Infrastructure.Persistence;

public class BarberoRepositorio(AgendaDbContext db) : IBarberoRepositorio
{
    public async Task AgregarAsync(Barbero barbero, CancellationToken ct = default)
    {
        db.Barberos.Add(barbero);
        await db.SaveChangesAsync(ct);
    }

    public Task<Barbero?> ObtenerConHorariosAsync(Guid barberiaId, Guid barberoId, CancellationToken ct = default) =>
        db.Barberos
            .Include(b => b.Horarios)
            .FirstOrDefaultAsync(b => b.Id == barberoId && b.BarberiaId == barberiaId, ct);

    public async Task<IReadOnlyList<Barbero>> ListarActivosAsync(Guid barberiaId, CancellationToken ct = default) =>
        await db.Barberos
            .AsNoTracking()
            .Include(b => b.Horarios)
            .Where(b => b.BarberiaId == barberiaId && b.Activo)
            .OrderBy(b => b.Nombre)
            .ToListAsync(ct);

    public Task GuardarCambiosAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
