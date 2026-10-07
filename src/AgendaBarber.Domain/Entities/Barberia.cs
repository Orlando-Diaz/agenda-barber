using System.Text.RegularExpressions;

namespace AgendaBarber.Domain.Entities;

/// <summary>El negocio (el "tenant"): cada barbería ve solo sus propios datos.</summary>
public partial class Barberia
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>Va en la URL pública: /b/{Slug}. Solo minúsculas, números y guiones.</summary>
    public string Slug { get; private set; } = default!;

    public string Nombre { get; private set; } = default!;
    public string? Telefono { get; private set; }
    public string ZonaHoraria { get; private set; } = "America/Bogota";

    /// <summary>Dónde queda la barbería (se muestra en la lista y en su página).</summary>
    public string? Direccion { get; private set; }

    /// <summary>Una presentación corta de la barbería.</summary>
    public string? Descripcion { get; private set; }

    /// <summary>Cuándo se cambió la foto de portada (null si no tiene). Sirve también para refrescar la imagen en caché.</summary>
    public DateTime? FotoActualizadaUtc { get; private set; }
    public DateTime CreadaEnUtc { get; private set; } = DateTime.UtcNow;

    private Barberia() { } // lo usa EF Core

    public Barberia(string nombre, string slug, string? telefono = null)
    {
        nombre = nombre?.Trim() ?? "";
        if (nombre.Length is < 2 or > 100)
            throw new DomainException("El nombre de la barbería debe tener entre 2 y 100 caracteres.");

        slug = slug?.Trim().ToLowerInvariant() ?? "";
        if (!SlugValido().IsMatch(slug))
            throw new DomainException("El enlace solo puede tener letras minúsculas, números y guiones (3 a 50 caracteres).");

        Nombre = nombre;
        Slug = slug;
        Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
    }

    public void ActualizarPerfil(string? direccion, string? descripcion)
    {
        direccion = string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim();
        descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        if (direccion is { Length: > 150 })
            throw new DomainException("La dirección puede tener hasta 150 caracteres.");
        if (descripcion is { Length: > 300 })
            throw new DomainException("La descripción puede tener hasta 300 caracteres.");

        Direccion = direccion;
        Descripcion = descripcion;
    }

    public void MarcarFoto(DateTime ahoraUtc) => FotoActualizadaUtc = ahoraUtc;

    public void QuitarFoto() => FotoActualizadaUtc = null;

    [GeneratedRegex("^[a-z0-9](?:[a-z0-9-]{1,48})[a-z0-9]$")]
    private static partial Regex SlugValido();
}
