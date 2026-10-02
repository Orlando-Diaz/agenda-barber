using AgendaBarber.Application.Barberias;
using AgendaBarber.Application.Barberos;
using AgendaBarber.Application.Citas;
using AgendaBarber.Application.Servicios;

namespace AgendaBarber.Application.Disponibilidad;

/// <summary>Una hora libre: en UTC (para guardar) y en hora local de la barbería (para mostrar).</summary>
public record HoraDisponible(DateTime InicioUtc, TimeOnly InicioLocal);

public class ConsultarDisponibilidad(
    IBarberiaRepositorio barberias,
    IServicioRepositorio servicios,
    IBarberoRepositorio barberos,
    ICitaRepositorio citas,
    TimeProvider reloj)
{
    /// <summary>Horas libres de ese barbero para ese servicio y día. Null si algo no existe.</summary>
    public async Task<IReadOnlyList<HoraDisponible>?> EjecutarAsync(
        string slug, Guid barberoId, Guid servicioId, DateOnly fecha, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        var servicio = await servicios.ObtenerActivoAsync(barberia.Id, servicioId, ct);
        if (servicio is null) return null;

        var barbero = await barberos.ObtenerConHorariosAsync(barberia.Id, barberoId, ct);
        if (barbero is null || !barbero.Activo) return null;

        var zona = TimeZoneInfo.FindSystemTimeZoneById(barberia.ZonaHoraria);

        // El día local [00:00, 24:00) convertido a UTC: ese es el rango de citas que importa.
        var desdeUtc = TimeZoneInfo.ConvertTimeToUtc(fecha.ToDateTime(TimeOnly.MinValue), zona);
        var hastaUtc = TimeZoneInfo.ConvertTimeToUtc(fecha.AddDays(1).ToDateTime(TimeOnly.MinValue), zona);
        var citasDelDia = await citas.ListarDelBarberoAsync(barbero.Id, desdeUtc, hastaUtc, ct);

        var horasUtc = CalculadoraDisponibilidad.HorasDisponibles(
            barbero.Horarios, citasDelDia, fecha, zona, servicio.DuracionMinutos, reloj.GetUtcNow().UtcDateTime);

        return horasUtc
            .Select(h => new HoraDisponible(h, TimeOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(h, zona))))
            .ToList();
    }
}
