using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Autenticacion;

public class IniciarSesion(
    IBarberiaRepositorio barberias,
    IUsuarioRepositorio usuarios,
    IHasheadorContrasenas hasheador,
    IGeneradorToken generador)
{
    /// <summary>Devuelve la sesión, o null si el correo o la contraseña no coinciden (no se dice cuál falló).</summary>
    public async Task<SesionIniciada?> EjecutarAsync(string email, string password, CancellationToken ct = default)
    {
        var usuario = await usuarios.ObtenerPorEmailAsync(Usuario.NormalizarEmail(email), ct);
        if (usuario is null || !hasheador.Verificar(usuario.PasswordHash, password ?? ""))
            return null;

        var barberia = await barberias.ObtenerPorIdAsync(usuario.BarberiaId, ct);
        if (barberia is null) return null;

        var token = generador.Generar(usuario, barberia);
        return new SesionIniciada(token.Token, token.ExpiraUtc, barberia.Slug, barberia.Nombre);
    }
}
