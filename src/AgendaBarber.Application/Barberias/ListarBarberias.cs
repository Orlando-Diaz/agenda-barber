namespace AgendaBarber.Application.Barberias;

public class ListarBarberias(IBarberiaRepositorio repositorio)
{
    /// <summary>El directorio público: solo barberías que ya se pueden reservar.</summary>
    public Task<IReadOnlyList<BarberiaEnLista>> EjecutarAsync(CancellationToken ct = default) =>
        repositorio.ListarReservablesAsync(ct);
}
