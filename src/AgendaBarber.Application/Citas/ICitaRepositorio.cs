using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Citas;

public interface ICitaRepositorio
{
    /// <summary>Citas del barbero que tocan el intervalo [desdeUtc, hastaUtc).</summary>
    Task<IReadOnlyList<Cita>> ListarDelBarberoAsync(
        Guid barberoId, DateTime desdeUtc, DateTime hastaUtc, CancellationToken ct = default);

    /// <summary>Guarda la cita. Lanza HoraNoDisponibleException si la base detecta un cruce.</summary>
    Task AgregarAsync(Cita cita, CancellationToken ct = default);
}
