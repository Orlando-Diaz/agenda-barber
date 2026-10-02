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

    [GeneratedRegex("^[a-z0-9](?:[a-z0-9-]{1,48})[a-z0-9]$")]
    private static partial Regex SlugValido();
}
