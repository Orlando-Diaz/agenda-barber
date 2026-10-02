using AgendaBarber.Application.Servicios;
using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgendaBarber.Infrastructure.Persistence;

public class ServicioRepositorio(AgendaDbContext db) : IServicioRepositorio
{
    public async Task AgregarAsync(Servicio servicio, CancellationToken ct = default)
    {
        db.Servicios.Add(servicio);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Servicio>> ListarActivosAsync(Guid barberiaId, CancellationToken ct = default) =>
        await db.Servicios
            .AsNoTracking()
            .Where(s => s.BarberiaId == barberiaId && s.Activo)
            .OrderBy(s => s.Nombre)
            .ToListAsync(ct);

    public Task<Servicio?> ObtenerActivoAsync(Guid barberiaId, Guid servicioId, CancellationToken ct = default) =>
        db.Servicios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == servicioId && s.BarberiaId == barberiaId && s.Activo, ct);
}
