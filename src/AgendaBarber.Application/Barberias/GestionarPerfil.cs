using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Barberias;

public class ActualizarPerfilBarberia(IBarberiaRepositorio barberias)
{
    /// <summary>Cambia dirección y descripción. Null si la barbería no existe.</summary>
    public async Task<Barberia?> EjecutarAsync(string slug, string? direccion, string? descripcion, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerParaEditarPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        barberia.ActualizarPerfil(direccion, descripcion);
        await barberias.GuardarCambiosAsync(ct);
        return barberia;
    }
}

public class GuardarFotoBarberia(IBarberiaRepositorio barberias, IFotoBarberiaRepositorio fotos, TimeProvider reloj)
{
    /// <summary>Pone o reemplaza la foto de portada. Devuelve false si la barbería no existe.</summary>
    public async Task<bool> EjecutarAsync(string slug, byte[] datos, string tipo, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerParaEditarPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return false;

        await fotos.ReemplazarAsync(barberia.Id, datos, tipo, ct); // valida tipo y tamaño
        barberia.MarcarFoto(reloj.GetUtcNow().UtcDateTime);
        await barberias.GuardarCambiosAsync(ct);
        return true;
    }
}

public class QuitarFotoBarberia(IBarberiaRepositorio barberias, IFotoBarberiaRepositorio fotos)
{
    /// <summary>Quita la foto de portada. Devuelve false si la barbería no existe.</summary>
    public async Task<bool> EjecutarAsync(string slug, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerParaEditarPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return false;

        await fotos.QuitarAsync(barberia.Id, ct);
        barberia.QuitarFoto();
        await barberias.GuardarCambiosAsync(ct);
        return true;
    }
}

public class ObtenerFotoBarberia(IBarberiaRepositorio barberias, IFotoBarberiaRepositorio fotos)
{
    /// <summary>La foto de portada, o null si la barbería no existe o no tiene.</summary>
    public async Task<FotoBarberia?> EjecutarAsync(string slug, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null || barberia.FotoActualizadaUtc is null) return null;
        return await fotos.ObtenerAsync(barberia.Id, ct);
    }
}
