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

    public async Task<IReadOnlyList<CitaDetalle>> ListarAgendaAsync(
        Guid barberiaId, DateTime desdeUtc, DateTime hastaUtc, CancellationToken ct = default) =>
        await (
            from c in db.Citas.AsNoTracking()
            join b in db.Barberos on c.BarberoId equals b.Id
            join s in db.Servicios on c.ServicioId equals s.Id
            where c.BarberiaId == barberiaId && c.InicioUtc >= desdeUtc && c.InicioUtc < hastaUtc
            orderby c.InicioUtc
            select new CitaDetalle(
                c.Id, c.InicioUtc, c.FinUtc, b.Nombre, s.Nombre,
                c.ClienteNombre, c.ClienteTelefono, c.Precio, c.Estado)
        ).ToListAsync(ct);

    public Task<Cita?> ObtenerAsync(Guid barberiaId, Guid citaId, CancellationToken ct = default) =>
        db.Citas.FirstOrDefaultAsync(c => c.Id == citaId && c.BarberiaId == barberiaId, ct);

    public Task GuardarCambiosAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
