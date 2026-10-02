using AgendaBarber.Application.Barberias;
using AgendaBarber.Application.Barberos;
using AgendaBarber.Application.Citas;
using AgendaBarber.Application.Disponibilidad;
using AgendaBarber.Application.Servicios;
using AgendaBarber.Domain;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Citas;

public class ReservarCita(
    IBarberiaRepositorio barberias,
    IServicioRepositorio servicios,
    IBarberoRepositorio barberos,
    ICitaRepositorio citas,
    TimeProvider reloj)
{
    /// <summary>Reserva la cita. Devuelve null si la barbería, el barbero o el servicio no existen.</summary>
    public async Task<Cita?> EjecutarAsync(
        string slug, Guid barberoId, Guid servicioId, DateTime inicioUtc,
        string clienteNombre, string clienteTelefono, CancellationToken ct = default)
    {
        // Si el cliente mandó "2026-10-05T09:00:00-05:00", System.Text.Json lo da como hora local: se pasa a UTC.
        if (inicioUtc.Kind == DateTimeKind.Local) inicioUtc = inicioUtc.ToUniversalTime();
        if (inicioUtc.Kind != DateTimeKind.Utc)
            throw new DomainException("La hora de inicio debe incluir la zona horaria (por ejemplo, terminar en Z).");

        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        var servicio = await servicios.ObtenerActivoAsync(barberia.Id, servicioId, ct);
        if (servicio is null) return null;

        var barbero = await barberos.ObtenerConHorariosAsync(barberia.Id, barberoId, ct);
        if (barbero is null) return null;

        var ahoraUtc = reloj.GetUtcNow().UtcDateTime;

        // 1) La hora debe estar dentro del horario del barbero y libre (misma regla que ve el cliente).
        var zona = TimeZoneInfo.FindSystemTimeZoneById(barberia.ZonaHoraria);
        var fecha = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(inicioUtc, zona));
        var desdeUtc = TimeZoneInfo.ConvertTimeToUtc(fecha.ToDateTime(TimeOnly.MinValue), zona);
        var hastaUtc = TimeZoneInfo.ConvertTimeToUtc(fecha.AddDays(1).ToDateTime(TimeOnly.MinValue), zona);
        var citasDelDia = await citas.ListarDelBarberoAsync(barbero.Id, desdeUtc, hastaUtc, ct);

        var libres = CalculadoraDisponibilidad.HorasDisponibles(
            barbero.Horarios, citasDelDia, fecha, zona, servicio.DuracionMinutos, ahoraUtc);
        if (!libres.Contains(inicioUtc))
            throw new HoraNoDisponibleException("Esa hora no está disponible. Elige otra.");

        // 2) El dominio valida nombre, teléfono, pasado, etc., y arma la cita.
        var cita = Cita.Agendar(barbero, servicio, inicioUtc, clienteNombre, clienteTelefono, ahoraUtc);

        // 3) Si dos clientes llegan a la vez, la base de datos rechaza al segundo (HoraNoDisponibleException).
        await citas.AgregarAsync(cita, ct);
        return cita;
    }
}
