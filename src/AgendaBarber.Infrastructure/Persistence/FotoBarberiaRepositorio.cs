using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgendaBarber.Infrastructure.Persistence;

public class FotoBarberiaRepositorio(AgendaDbContext db) : IFotoBarberiaRepositorio
{
    public Task<FotoBarberia?> ObtenerAsync(Guid barberiaId, CancellationToken ct = default) =>
        db.FotosBarberia.AsNoTracking().FirstOrDefaultAsync(f => f.BarberiaId == barberiaId, ct);

    public async Task ReemplazarAsync(Guid barberiaId, byte[] datos, string tipo, CancellationToken ct = default)
    {
        var existente = await db.FotosBarberia.FirstOrDefaultAsync(f => f.BarberiaId == barberiaId, ct);
        if (existente is null) db.FotosBarberia.Add(new FotoBarberia(barberiaId, datos, tipo));
        else existente.Reemplazar(datos, tipo);
    }

    public async Task QuitarAsync(Guid barberiaId, CancellationToken ct = default)
    {
        var existente = await db.FotosBarberia.FirstOrDefaultAsync(f => f.BarberiaId == barberiaId, ct);
        if (existente is not null) db.FotosBarberia.Remove(existente);
    }
}
