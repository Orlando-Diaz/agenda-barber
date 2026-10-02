using AgendaBarber.Application.Disponibilidad;
using AgendaBarber.Domain.Entities;
using Xunit;

namespace AgendaBarber.Tests;

public class CalculadoraDisponibilidadTests
{
    private static readonly TimeZoneInfo Bogota = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
    private static readonly DateOnly Lunes = new(2026, 10, 12);
    private static readonly DateTime Ahora = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid BarberiaId = Guid.NewGuid();

    /// <summary>Una hora del lunes 12 de octubre en Colombia, expresada en UTC (que es como se guarda).</summary>
    private static DateTime Utc(int hora, int minuto = 0) =>
        TimeZoneInfo.ConvertTimeToUtc(new DateTime(2026, 10, 12, hora, minuto, 0), Bogota);

    private static Barbero BarberoConTramos(params (int inicio, int fin)[] tramos)
    {
        var barbero = new Barbero(BarberiaId, "Carlos");
        foreach (var (inicio, fin) in tramos)
            barbero.AgregarHorario(DayOfWeek.Monday, new TimeOnly(inicio, 0), new TimeOnly(fin, 0));
        return barbero;
    }

    private static Cita CitaDe30Min(Barbero barbero, DateTime inicioUtc) =>
        Cita.Agendar(barbero, new Servicio(BarberiaId, "Corte", 30, 25000m), inicioUtc, "Cliente", "3001234567", Ahora);

    private static IReadOnlyList<DateTime> Calcular(
        Barbero barbero, IEnumerable<Cita> citas, int duracion = 30, DateTime? ahora = null, DateOnly? fecha = null) =>
        CalculadoraDisponibilidad.HorasDisponibles(
            barbero.Horarios, citas, fecha ?? Lunes, Bogota, duracion, ahora ?? Ahora);

    [Fact]
    public void Ofrece_un_hueco_cada_30_minutos_dentro_del_horario()
    {
        var barbero = BarberoConTramos((9, 12));

        var horas = Calcular(barbero, []);

        Assert.Equal([Utc(9), Utc(9, 30), Utc(10), Utc(10, 30), Utc(11), Utc(11, 30)], horas);
    }

    [Fact]
    public void Una_cita_bloquea_los_huecos_que_se_cruzan_con_ella()
    {
        var barbero = BarberoConTramos((9, 12));
        var cita = CitaDe30Min(barbero, Utc(10)); // 10:00 a 10:30

        var horas = Calcular(barbero, [cita], duracion: 60);

        // 9:00–10:00 cabe justo antes; 9:30 y 10:00 chocan; 10:30 empieza justo cuando ella termina.
        Assert.Equal([Utc(9), Utc(10, 30), Utc(11)], horas);
    }

    [Fact]
    public void Una_cita_cancelada_libera_el_hueco()
    {
        var barbero = BarberoConTramos((9, 12));
        var cita = CitaDe30Min(barbero, Utc(10));
        cita.Cancelar();

        var horas = Calcular(barbero, [cita]);

        Assert.Contains(Utc(10), horas);
        Assert.Equal(6, horas.Count);
    }

    [Fact]
    public void No_ofrece_horas_en_el_descanso_ni_servicios_que_lo_cruzan()
    {
        var barbero = BarberoConTramos((9, 12), (14, 18));

        var horas = Calcular(barbero, [], duracion: 60);

        Assert.Contains(Utc(11), horas);      // 11:00–12:00 cabe justo
        Assert.DoesNotContain(Utc(11, 30), horas); // terminaría 12:30, ya en el descanso
        Assert.DoesNotContain(Utc(12), horas);
        Assert.DoesNotContain(Utc(13, 30), horas);
        Assert.Contains(Utc(14), horas);
    }

    [Fact]
    public void No_ofrece_horas_que_ya_pasaron()
    {
        var barbero = BarberoConTramos((9, 12));

        var horas = Calcular(barbero, [], ahora: Utc(10, 15));

        Assert.Equal([Utc(10, 30), Utc(11), Utc(11, 30)], horas);
    }

    [Fact]
    public void Un_dia_sin_horario_no_tiene_huecos()
    {
        var barbero = BarberoConTramos((9, 12)); // solo trabaja los lunes

        var martes = new DateOnly(2026, 10, 13);

        Assert.Empty(Calcular(barbero, [], fecha: martes));
    }

    [Fact]
    public void Si_el_servicio_no_cabe_en_el_tramo_no_hay_huecos()
    {
        var barbero = new Barbero(BarberiaId, "Carlos");
        barbero.AgregarHorario(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(9, 20));

        Assert.Empty(Calcular(barbero, [], duracion: 30));
    }
}
