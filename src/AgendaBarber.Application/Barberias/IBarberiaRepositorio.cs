using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberias;

public interface IBarberiaRepositorio
{
    Task<bool> ExisteSlugAsync(string slug, CancellationToken ct = default);
    Task AgregarAsync(Barberia barberia, CancellationToken ct = default);
}