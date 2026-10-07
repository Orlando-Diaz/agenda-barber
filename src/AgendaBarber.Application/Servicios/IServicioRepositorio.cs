using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Servicios;

public interface IServicioRepositorio
{
    Task AgregarAsync(Servicio servicio, CancellationToken ct = default);
    Task<IReadOnlyList<Servicio>> ListarActivosAsync(Guid barberiaId, CancellationToken ct = default);
    Task<Servicio?> ObtenerActivoAsync(Guid barberiaId, Guid servicioId, CancellationToken ct = default);

    /// <summary>El servicio activo para modificarlo (con seguimiento de cambios, a diferencia de ObtenerActivoAsync).</summary>
    Task<Servicio?> ObtenerParaEditarAsync(Guid barberiaId, Guid servicioId, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
