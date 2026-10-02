using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Servicios;

public interface IServicioRepositorio
{
    Task AgregarAsync(Servicio servicio, CancellationToken ct = default);
    Task<IReadOnlyList<Servicio>> ListarActivosAsync(Guid barberiaId, CancellationToken ct = default);
}
