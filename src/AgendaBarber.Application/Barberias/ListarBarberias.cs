using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberias;

public class ListarBarberias(IBarberiaRepositorio repositorio)
{
    /// <summary>El directorio público: solo barberías que ya se pueden reservar.</summary>
    public Task<IReadOnlyList<Barberia>> EjecutarAsync(CancellationToken ct = default) =>
        repositorio.ListarReservablesAsync(ct);
}
