using AgendaBarber.Domain;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Tests;

public class BarberoTests
{
    private static Barbero UnBarbero() => new(Guid.NewGuid(), "Carlos");

    [Fact]
    public void Renombrar_cambia_el_nombre_y_valida_el_largo()
    {
        var barbero = UnBarbero();

        barbero.Renombrar("  Andrés ");
        Assert.Equal("Andrés", barbero.Nombre);

        Assert.Throws<DomainException>(() => barbero.Renombrar("A"));
        Assert.Equal("Andrés", barbero.Nombre);
    }

    [Fact]
    public void QuitarHorario_elimina_solo_ese_tramo()
    {
        var barbero = UnBarbero();
        barbero.AgregarHorario(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        barbero.AgregarHorario(DayOfWeek.Monday, new TimeOnly(14, 0), new TimeOnly(18, 0));
        var manana = barbero.Horarios.Single(h => h.Inicio == new TimeOnly(9, 0));

        var quitado = barbero.QuitarHorario(manana.Id);

        Assert.True(quitado);
        var queda = Assert.Single(barbero.Horarios);
        Assert.Equal(new TimeOnly(14, 0), queda.Inicio);
    }

    [Fact]
    public void QuitarHorario_de_un_id_que_no_existe_devuelve_false()
    {
        var barbero = UnBarbero();
        barbero.AgregarHorario(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));

        Assert.False(barbero.QuitarHorario(Guid.NewGuid()));
        Assert.Single(barbero.Horarios);
    }

    [Fact]
    public void Tras_quitar_un_tramo_se_puede_agregar_otro_que_antes_se_cruzaba()
    {
        var barbero = UnBarbero();
        barbero.AgregarHorario(DayOfWeek.Friday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        Assert.Throws<DomainException>(() => barbero.AgregarHorario(DayOfWeek.Friday, new TimeOnly(11, 0), new TimeOnly(13, 0)));

        barbero.QuitarHorario(barbero.Horarios.Single().Id);
        barbero.AgregarHorario(DayOfWeek.Friday, new TimeOnly(11, 0), new TimeOnly(13, 0));

        Assert.Single(barbero.Horarios);
    }
}
