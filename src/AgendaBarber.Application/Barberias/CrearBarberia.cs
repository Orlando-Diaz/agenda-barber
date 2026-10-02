using AgendaBarber.Domain;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberias;

public class CrearBarberia(IBarberiaRepositorio repositorio)
{
    public async Task<Barberia> EjecutarAsync(
        string nombre, string slug, string? telefono, CancellationToken ct = default)
    {
        var barberia = new Barberia(nombre, slug, telefono); // aquí el dominio valida sus reglas

        if (await repositorio.ExisteSlugAsync(barberia.Slug, ct))
            throw new DomainException("Ese enlace ya está en uso. Elige otro.");

        await repositorio.AgregarAsync(barberia, ct);
        return barberia;
    }
}