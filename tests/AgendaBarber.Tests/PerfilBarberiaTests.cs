using AgendaBarber.Domain;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Tests;

public class PerfilBarberiaTests
{
    private static Barberia Nueva() => new("El Patrón", "el-patron");

    [Fact]
    public void ActualizarPerfil_guarda_direccion_y_descripcion_sin_espacios_sobrantes()
    {
        var b = Nueva();
        b.ActualizarPerfil("  Calle 5 # 3-20, Quimbaya ", " Cortes clásicos y barba ");
        Assert.Equal("Calle 5 # 3-20, Quimbaya", b.Direccion);
        Assert.Equal("Cortes clásicos y barba", b.Descripcion);
    }

    [Fact]
    public void ActualizarPerfil_con_textos_vacios_los_deja_en_null()
    {
        var b = Nueva();
        b.ActualizarPerfil("Calle 1", "Hola");
        b.ActualizarPerfil("  ", "");
        Assert.Null(b.Direccion);
        Assert.Null(b.Descripcion);
    }

    [Fact]
    public void ActualizarPerfil_rechaza_textos_muy_largos()
    {
        var b = Nueva();
        Assert.Throws<DomainException>(() => b.ActualizarPerfil(new string('a', 151), null));
        Assert.Throws<DomainException>(() => b.ActualizarPerfil(null, new string('a', 301)));
    }

    [Fact]
    public void Foto_acepta_jpeg_png_y_webp()
    {
        var id = Guid.NewGuid();
        foreach (var tipo in new[] { "image/jpeg", "image/png", "image/webp" })
            Assert.Equal(tipo, new FotoBarberia(id, [1, 2, 3], tipo).Tipo);
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("text/html")]
    [InlineData("")]
    public void Foto_rechaza_otros_tipos(string tipo) =>
        Assert.Throws<DomainException>(() => new FotoBarberia(Guid.NewGuid(), [1, 2, 3], tipo));

    [Fact]
    public void Foto_rechaza_vacia_o_demasiado_pesada()
    {
        Assert.Throws<DomainException>(() => new FotoBarberia(Guid.NewGuid(), [], "image/jpeg"));
        Assert.Throws<DomainException>(() => new FotoBarberia(Guid.NewGuid(), new byte[FotoBarberia.TamanoMaximo + 1], "image/jpeg"));
    }

    [Fact]
    public void MarcarFoto_y_QuitarFoto_cambian_la_fecha()
    {
        var b = Nueva();
        var ahora = new DateTime(2026, 10, 7, 20, 0, 0, DateTimeKind.Utc);
        b.MarcarFoto(ahora);
        Assert.Equal(ahora, b.FotoActualizadaUtc);
        b.QuitarFoto();
        Assert.Null(b.FotoActualizadaUtc);
    }
}
