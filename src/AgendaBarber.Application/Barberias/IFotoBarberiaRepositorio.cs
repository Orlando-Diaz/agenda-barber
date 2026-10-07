using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberias;

public interface IFotoBarberiaRepositorio
{
    Task<FotoBarberia?> ObtenerAsync(Guid barberiaId, CancellationToken ct = default);

    /// <summary>Crea o reemplaza la foto. No guarda: se guarda junto con la barbería (mismo DbContext).</summary>
    Task ReemplazarAsync(Guid barberiaId, byte[] datos, string tipo, CancellationToken ct = default);

    /// <summary>Marca la foto para borrarla. No guarda.</summary>
    Task QuitarAsync(Guid barberiaId, CancellationToken ct = default);
}
