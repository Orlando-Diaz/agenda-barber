using AgendaBarber.Domain;
using AgendaBarber.Domain.Entities;
using Xunit;

namespace AgendaBarber.Tests;

public class CitaTests
{
    private static readonly DateTime Ahora = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid BarberiaId = Guid.NewGuid();

    private static Barbero UnBarbero(Guid? barberiaId = null) => new(barberiaId ?? BarberiaId, "Carlos");
    private static Servicio UnServicio(Guid? barberiaId = null) => new(barberiaId ?? BarberiaId, "Corte", 30, 25000m);

    private static Cita UnaCita(DateTime? inicio = null) =>
        Cita.Agendar(UnBarbero(), UnServicio(), inicio ?? Ahora.AddDays(1), "Juan Pérez", "300 123 4567", Ahora);

    [Fact]
    public void Agendar_calcula_el_fin_y_congela_el_precio()
    {
        var inicio = Ahora.AddDays(1);
        var cita = UnaCita(inicio);

        Assert.Equal(inicio.AddMinutes(30), cita.FinUtc);
        Assert.Equal(25000m, cita.Precio);
        Assert.Equal("3001234567", cita.ClienteTelefono);
        Assert.Equal(EstadoCita.Pendiente, cita.Estado);
    }

    [Fact]
    public void No_se_puede_agendar_en_el_pasado()
    {
        Assert.Throws<DomainException>(() => UnaCita(Ahora.AddMinutes(-1)));
    }

    [Fact]
    public void Exige_hora_en_UTC()
    {
        var local = new DateTime(2026, 10, 6, 9, 0, 0, DateTimeKind.Unspecified);
        Assert.Throws<DomainException>(() => UnaCita(local));
    }

    [Fact]
    public void Barbero_y_servicio_deben_ser_de_la_misma_barberia()
    {
        var otra = Guid.NewGuid();
        Assert.Throws<DomainException>(() =>
            Cita.Agendar(UnBarbero(), UnServicio(otra), Ahora.AddDays(1), "Juan", "3001234567", Ahora));
    }

    [Fact]
    public void Citas_pegadas_no_se_solapan_pero_las_cruzadas_si()
    {
        var cita = UnaCita(Ahora.AddDays(1)); // dura 30 min

        Assert.False(cita.SeSolapaCon(cita.FinUtc, cita.FinUtc.AddMinutes(30)));            // empieza justo cuando termina
        Assert.False(cita.SeSolapaCon(cita.InicioUtc.AddMinutes(-30), cita.InicioUtc));     // termina justo cuando empieza
        Assert.True(cita.SeSolapaCon(cita.InicioUtc.AddMinutes(15), cita.FinUtc.AddMinutes(15)));
    }

    [Fact]
    public void Cancelar_libera_el_hueco_y_no_se_puede_repetir()
    {
        var cita = UnaCita();
        cita.Cancelar();

        Assert.False(cita.OcupaAgenda);
        Assert.Throws<DomainException>(() => cita.Cancelar());
        Assert.Throws<DomainException>(() => cita.Confirmar());
    }

    [Fact]
    public void No_se_puede_marcar_atendida_antes_de_que_empiece()
    {
        var cita = UnaCita(Ahora.AddDays(1));
        Assert.Throws<DomainException>(() => cita.MarcarAtendida(Ahora));

        cita.MarcarAtendida(cita.InicioUtc.AddMinutes(40));
        Assert.Equal(EstadoCita.Atendida, cita.Estado);
    }

    [Fact]
    public void El_horario_de_un_barbero_no_admite_tramos_cruzados()
    {
        var barbero = UnBarbero();
        barbero.AgregarHorario(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        barbero.AgregarHorario(DayOfWeek.Monday, new TimeOnly(14, 0), new TimeOnly(18, 0)); // descanso al almuerzo: válido

        Assert.Throws<DomainException>(() =>
            barbero.AgregarHorario(DayOfWeek.Monday, new TimeOnly(11, 0), new TimeOnly(13, 0)));
        Assert.Throws<DomainException>(() =>
            barbero.AgregarHorario(DayOfWeek.Tuesday, new TimeOnly(10, 0), new TimeOnly(9, 0)));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("Mi Barbería")]
    [InlineData("-barberia")]
    public void Slug_invalido_se_rechaza(string slug)
    {
        Assert.Throws<DomainException>(() => new Barberia("El Patrón", slug));
    }

    [Fact]
    public void Slug_valido_se_acepta()
    {
        Assert.Equal("el-patron", new Barberia("El Patrón", "El-Patron").Slug);
    }
}
