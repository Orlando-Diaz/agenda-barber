using System.Text.RegularExpressions;

namespace AgendaBarber.Domain.Entities;

public enum RolUsuario
{
    Dueno = 0,
}

/// <summary>Quien inicia sesión. Hoy solo existe el dueño de una barbería; luego se agrega el rol de barbero.</summary>
public partial class Usuario
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid BarberiaId { get; private set; }
    public string Email { get; private set; } = default!;

    /// <summary>Nunca la contraseña: solo su "hash" (una huella que no se puede revertir).</summary>
    public string PasswordHash { get; private set; } = default!;

    public RolUsuario Rol { get; private set; }
    public DateTime CreadoEnUtc { get; private set; } = DateTime.UtcNow;

    private Usuario() { }

    public Usuario(Guid barberiaId, string email, string passwordHash, RolUsuario rol = RolUsuario.Dueno)
    {
        email = NormalizarEmail(email);
        if (email.Length > 254 || !EmailValido().IsMatch(email))
            throw new DomainException("El correo no es válido.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Falta la contraseña.");

        BarberiaId = barberiaId;
        Email = email;
        PasswordHash = passwordHash;
        Rol = rol;
    }

    /// <summary>"  Ana@Correo.COM " y "ana@correo.com" son el mismo correo.</summary>
    public static string NormalizarEmail(string? email) => email?.Trim().ToLowerInvariant() ?? "";

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailValido();
}
