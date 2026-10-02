using AgendaBarber.Application.Citas;
using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgendaBarber.Infrastructure.Persistence;

public class CitaRepositorio(AgendaDbContext db) : ICitaRepositorio
{
    public async Task<IReadOnlyList<Cita>> ListarDelBarberoAsync(
        Guid barberoId, DateTime desdeUtc, DateTime hastaUtc, CancellationToken ct = default) =>
        await db.Citas
            .AsNoTracking()
            .Where(c => c.BarberoId == barberoId && c.InicioUtc < hastaUtc && c.FinUtc > desdeUtc)
            .ToListAsync(ct);
}
