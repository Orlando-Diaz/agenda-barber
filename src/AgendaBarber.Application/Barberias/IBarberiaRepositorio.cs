using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberias;

public interface IBarberiaRepositorio
{
    Task<bool> ExisteSlugAsync(string slug, CancellationToken ct = default);
    Task AgregarAsync(Barberia barberia, CancellationToken ct = default);
    Task<Barberia?> ObtenerPorSlugAsync(string slug, CancellationToken ct = default);
    Task<Barberia?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Barberías listas para recibir reservas (con al menos un servicio y un barbero con horario), por nombre.</summary>
    Task<IReadOnlyList<BarberiaEnLista>> ListarReservablesAsync(CancellationToken ct = default);

    /// <summary>La barbería con seguimiento de cambios, para modificar su perfil.</summary>
    Task<Barberia?> ObtenerParaEditarPorSlugAsync(string slug, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
