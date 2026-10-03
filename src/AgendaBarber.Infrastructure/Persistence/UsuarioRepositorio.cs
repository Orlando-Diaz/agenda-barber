using AgendaBarber.Application.Autenticacion;
using AgendaBarber.Domain;
using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AgendaBarber.Infrastructure.Persistence;

public class UsuarioRepositorio(AgendaDbContext db) : IUsuarioRepositorio
{
    public Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default) =>
        db.Usuarios.AnyAsync(u => u.Email == email, ct);

    public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct = default) =>
        db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task AgregarDuenoAsync(Barberia barberia, Usuario usuario, CancellationToken ct = default)
    {
        db.Barberias.Add(barberia);
        db.Usuarios.Add(usuario);
        try
        {
            await db.SaveChangesAsync(ct); // un solo guardado = una sola transacción
        }
        // 23505 = índice único: otro registro simultáneo ganó el mismo correo o enlace.
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DomainException("Ese correo o enlace ya está en uso.");
        }
    }
}
