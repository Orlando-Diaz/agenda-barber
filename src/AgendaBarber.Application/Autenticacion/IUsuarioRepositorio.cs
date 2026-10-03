using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Autenticacion;

public interface IUsuarioRepositorio
{
    Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default);
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Guarda la barbería y su dueño juntos: o se guardan los dos o ninguno.</summary>
    Task AgregarDuenoAsync(Barberia barberia, Usuario usuario, CancellationToken ct = default);
}
