using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Disponibilidad;

/// <summary>
/// Calcula a qué horas puede reservar un cliente. Es una función pura: recibe datos y devuelve
/// un resultado, sin tocar la base de datos ni el reloj. Por eso se prueba fácil y rápido.
/// </summary>
public static class CalculadoraDisponibilidad
{
    /// <param name="horarios">Los tramos de trabajo del barbero (todos los días; aquí se filtra el que corresponde).</param>
    /// <param name="citas">Las citas del barbero para ese día (las canceladas se ignoran).</param>
    /// <param name="fecha">El día que consulta el cliente, en hora local de la barbería.</param>
    /// <param name="zona">Zona horaria de la barbería (America/Bogota).</param>
    /// <param name="duracionMinutos">Duración del servicio elegido.</param>
    /// <param name="ahoraUtc">El momento actual: no se ofrecen horas que ya pasaron.</param>
    /// <param name="pasoMinutos">Cada cuántos minutos se ofrece un inicio posible (9:00, 9:30, 10:00...).</param>
    /// <returns>Las horas de inicio disponibles, en UTC y ordenadas.</returns>
    public static IReadOnlyList<DateTime> HorasDisponibles(
        IEnumerable<HorarioTrabajo> horarios,
        IEnumerable<Cita> citas,
        DateOnly fecha,
        TimeZoneInfo zona,
        int duracionMinutos,
        DateTime ahoraUtc,
        int pasoMinutos = 30)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duracionMinutos, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pasoMinutos, 0);

        var duracion = TimeSpan.FromMinutes(duracionMinutos);
        var paso = TimeSpan.FromMinutes(pasoMinutos);
        var ocupadas = citas.Where(c => c.OcupaAgenda).ToList();
        var resultado = new List<DateTime>();

        foreach (var tramo in horarios.Where(h => h.Dia == fecha.DayOfWeek).OrderBy(h => h.Inicio))
        {
            // El horario está en hora local; las citas, en UTC. Se convierte cada candidato.
            var inicioLocal = fecha.ToDateTime(tramo.Inicio);
            var finLocal = fecha.ToDateTime(tramo.Fin);

            for (var t = inicioLocal; t + duracion <= finLocal; t += paso)
            {
                var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(t, zona);
                if (inicioUtc <= ahoraUtc) continue;

                var finUtc = inicioUtc + duracion;
                if (ocupadas.Any(c => c.SeSolapaCon(inicioUtc, finUtc))) continue;

                resultado.Add(inicioUtc);
            }
        }

        return resultado;
    }
}
