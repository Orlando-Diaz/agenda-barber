using AgendaBarber.Application.Citas;
using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AgendaBarber.Infrastructure.Persistence;

public class CitaRepositorio(AgendaDbContext db) : ICitaRepositorio
{
    public async Task<IReadOnlyList<Cita>> ListarDelBarberoAsync(
        Guid barberoId, DateTime desdeUtc, DateTime hastaUtc, CancellationToken ct = default) =>
        await db.Citas
            .AsNoTracking()
            .Where(c => c.BarberoId == barberoId && c.InicioUtc < hastaUtc && c.FinUtc > desdeUtc)
            .ToListAsync(ct);

    public async Task AgregarAsync(Cita cita, CancellationToken ct = default)
    {
        try
        {
            db.Citas.Add(cita);
            await db.SaveChangesAsync(ct);
        }
        // 23P01 = violación de la restricción de exclusión: otra cita ocupa ese hueco.
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ExclusionViolation })
        {
            db.Entry(cita).State = EntityState.Detached;
            throw new HoraNoDisponibleException();
        }
    }
}
