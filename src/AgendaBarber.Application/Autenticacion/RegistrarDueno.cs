using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Autenticacion;

public class RegistrarDueno(
    IBarberiaRepositorio barberias,
    IUsuarioRepositorio usuarios,
    IHasheadorContrasenas hasheador,
    IGeneradorToken generador)
{
    /// <summary>Crea la barbería y a su dueño, y deja la sesión iniciada.</summary>
    public async Task<SesionIniciada> EjecutarAsync(
        string nombreBarberia, string slug, string? telefono, string email, string password,
        CancellationToken ct = default)
    {
        if (password is null || password.Length is < 8 or > 100)
            throw new DomainException("La contraseña debe tener entre 8 y 100 caracteres.");

        var barberia = new Barberia(nombreBarberia, slug, telefono); // valida nombre y slug

        if (await barberias.ExisteSlugAsync(barberia.Slug, ct))
            throw new DomainException("Ese enlace ya está en uso. Elige otro.");
        if (await usuarios.ExisteEmailAsync(Usuario.NormalizarEmail(email), ct))
            throw new DomainException("Ese correo ya está registrado.");

        var usuario = new Usuario(barberia.Id, email, hasheador.Hashear(password)); // valida el correo
        await usuarios.AgregarDuenoAsync(barberia, usuario, ct);

        var token = generador.Generar(usuario, barberia);
        return new SesionIniciada(token.Token, token.ExpiraUtc, barberia.Slug, barberia.Nombre);
    }
}
