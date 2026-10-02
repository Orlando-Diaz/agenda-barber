using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Citas;

public interface ICitaRepositorio
{
    /// <summary>Citas del barbero que tocan el intervalo [desdeUtc, hastaUtc).</summary>
    Task<IReadOnlyList<Cita>> ListarDelBarberoAsync(
        Guid barberoId, DateTime desdeUtc, DateTime hastaUtc, CancellationToken ct = default);

    /// <summary>Guarda la cita. Lanza HoraNoDisponibleException si la base detecta un cruce.</summary>
    Task AgregarAsync(Cita cita, CancellationToken ct = default);

    /// <summary>Todas las citas de la barbería que empiezan en [desdeUtc, hastaUtc), ordenadas por hora.</summary>
    Task<IReadOnlyList<CitaDetalle>> ListarAgendaAsync(
        Guid barberiaId, DateTime desdeUtc, DateTime hastaUtc, CancellationToken ct = default);

    /// <summary>Busca una cita de esa barbería (con seguimiento de cambios, para poder modificarla).</summary>
    Task<Cita?> ObtenerAsync(Guid barberiaId, Guid citaId, CancellationToken ct = default);

    Task GuardarCambiosAsync(CancellationToken ct = default);
}
