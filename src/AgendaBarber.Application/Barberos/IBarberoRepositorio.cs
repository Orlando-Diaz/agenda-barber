using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberos;

public interface IBarberoRepositorio
{
    Task AgregarAsync(Barbero barbero, CancellationToken ct = default);
    Task<Barbero?> ObtenerConHorariosAsync(Guid barberiaId, Guid barberoId, CancellationToken ct = default);
    Task<IReadOnlyList<Barbero>> ListarActivosAsync(Guid barberiaId, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
