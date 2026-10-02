using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Citas;

public interface ICitaRepositorio
{
    /// <summary>Citas del barbero que tocan el intervalo [desdeUtc, hastaUtc).</summary>
    Task<IReadOnlyList<Cita>> ListarDelBarberoAsync(
        Guid barberoId, DateTime desdeUtc, DateTime hastaUtc, CancellationToken ct = default);
}
