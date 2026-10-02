using AgendaBarber.Application.Barberias;

namespace AgendaBarber.Application.Citas;

/// <summary>Una cita de la agenda, con las horas ya en hora local de la barbería.</summary>
public record ItemAgenda(
    Guid Id, TimeOnly HoraInicio, TimeOnly HoraFin, string Barbero, string Servicio,
    string ClienteNombre, string ClienteTelefono, decimal Precio, string Estado);

public class ConsultarAgenda(IBarberiaRepositorio barberias, ICitaRepositorio citas)
{
    /// <summary>Las citas de ese día (hora local de la barbería). Null si la barbería no existe.</summary>
    public async Task<IReadOnlyList<ItemAgenda>?> EjecutarAsync(string slug, DateOnly fecha, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        var zona = TimeZoneInfo.FindSystemTimeZoneById(barberia.ZonaHoraria);
        var desdeUtc = TimeZoneInfo.ConvertTimeToUtc(fecha.ToDateTime(TimeOnly.MinValue), zona);
        var hastaUtc = TimeZoneInfo.ConvertTimeToUtc(fecha.AddDays(1).ToDateTime(TimeOnly.MinValue), zona);

        var lista = await citas.ListarAgendaAsync(barberia.Id, desdeUtc, hastaUtc, ct);

        TimeOnly Local(DateTime utc) => TimeOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, zona));

        return lista
            .Select(c => new ItemAgenda(
                c.Id, Local(c.InicioUtc), Local(c.FinUtc), c.Barbero, c.Servicio,
                c.ClienteNombre, c.ClienteTelefono, c.Precio, c.Estado.ToString()))
            .ToList();
    }
}
