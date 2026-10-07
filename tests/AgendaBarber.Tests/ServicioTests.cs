using AgendaBarber.Domain;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Tests;

public class ServicioTests
{
    private static Servicio UnServicio() => new(Guid.NewGuid(), "Corte", 30, 25000m);

    [Fact]
    public void Editar_cambia_nombre_duracion_y_precio()
    {
        var servicio = UnServicio();

        servicio.Editar("  Corte y barba ", 50, 35000m);

        Assert.Equal("Corte y barba", servicio.Nombre);
        Assert.Equal(50, servicio.DuracionMinutos);
        Assert.Equal(35000m, servicio.Precio);
    }

    [Theory]
    [InlineData("A", 30, 1000)]        // nombre muy corto
    [InlineData("Corte", 4, 1000)]     // duración menor al mínimo
    [InlineData("Corte", 481, 1000)]   // duración mayor al máximo
    [InlineData("Corte", 30, -1)]      // precio negativo
    public void Editar_aplica_las_mismas_reglas_que_crear(string nombre, int duracion, decimal precio)
    {
        var servicio = UnServicio();

        Assert.Throws<DomainException>(() => servicio.Editar(nombre, duracion, precio));
    }

    [Fact]
    public void Una_edicion_invalida_no_cambia_nada()
    {
        var servicio = UnServicio();

        Assert.Throws<DomainException>(() => servicio.Editar("Barba", 30, -5m));

        Assert.Equal("Corte", servicio.Nombre);
        Assert.Equal(25000m, servicio.Precio);
    }

    [Fact]
    public void Desactivar_lo_deja_inactivo()
    {
        var servicio = UnServicio();

        servicio.Desactivar();

        Assert.False(servicio.Activo);
    }
}
