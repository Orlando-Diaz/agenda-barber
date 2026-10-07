using AgendaBarber.Application.Barberias;
using AgendaBarber.Application.Barberos;
using AgendaBarber.Application.Servicios;
using AgendaBarber.Domain;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Tests;

/// <summary>
/// Pruebas de los casos de uso de editar y quitar, con repositorios falsos en memoria.
/// Lo más importante: una barbería nunca puede tocar los datos de otra.
/// </summary>
public class CasosDeUsoGestionTests
{
    // ---------- Repositorios falsos ----------

    private sealed class BarberiasFalsas(params Barberia[] barberias) : IBarberiaRepositorio
    {
        public Task<bool> ExisteSlugAsync(string slug, CancellationToken ct = default) =>
            Task.FromResult(barberias.Any(b => b.Slug == slug));
        public Task AgregarAsync(Barberia barberia, CancellationToken ct = default) => Task.CompletedTask;
        public Task<Barberia?> ObtenerPorSlugAsync(string slug, CancellationToken ct = default) =>
            Task.FromResult(barberias.FirstOrDefault(b => b.Slug == slug));
        public Task<Barberia?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(barberias.FirstOrDefault(b => b.Id == id));
        public Task<IReadOnlyList<BarberiaEnLista>> ListarReservablesAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<BarberiaEnLista>>([]);
        public Task<Barberia?> ObtenerParaEditarPorSlugAsync(string slug, CancellationToken ct = default) => ObtenerPorSlugAsync(slug, ct);
        public Task GuardarCambiosAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class ServiciosFalsos(params Servicio[] servicios) : IServicioRepositorio
    {
        public int Guardados { get; private set; }
        public Task AgregarAsync(Servicio servicio, CancellationToken ct = default) => Task.CompletedTask;
        public Task<IReadOnlyList<Servicio>> ListarActivosAsync(Guid barberiaId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Servicio>>(servicios.Where(s => s.BarberiaId == barberiaId && s.Activo).ToList());
        public Task<Servicio?> ObtenerActivoAsync(Guid barberiaId, Guid servicioId, CancellationToken ct = default) =>
            ObtenerParaEditarAsync(barberiaId, servicioId, ct);
        public Task<Servicio?> ObtenerParaEditarAsync(Guid barberiaId, Guid servicioId, CancellationToken ct = default) =>
            Task.FromResult(servicios.FirstOrDefault(s => s.Id == servicioId && s.BarberiaId == barberiaId && s.Activo));
        public Task GuardarCambiosAsync(CancellationToken ct = default) { Guardados++; return Task.CompletedTask; }
    }

    private sealed class BarberosFalsos(params Barbero[] barberos) : IBarberoRepositorio
    {
        public int Guardados { get; private set; }
        public Task AgregarAsync(Barbero barbero, CancellationToken ct = default) => Task.CompletedTask;
        public Task<Barbero?> ObtenerConHorariosAsync(Guid barberiaId, Guid barberoId, CancellationToken ct = default) =>
            Task.FromResult(barberos.FirstOrDefault(b => b.Id == barberoId && b.BarberiaId == barberiaId));
        public Task<IReadOnlyList<Barbero>> ListarActivosAsync(Guid barberiaId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Barbero>>(barberos.Where(b => b.BarberiaId == barberiaId && b.Activo).ToList());
        public Task GuardarCambiosAsync(CancellationToken ct = default) { Guardados++; return Task.CompletedTask; }
    }

    private static readonly Barberia ElPatron = new("El Patrón", "el-patron");
    private static readonly Barberia LaOtra = new("La Otra", "la-otra");

    // ---------- Servicios ----------

    [Fact]
    public async Task EditarServicio_cambia_los_datos_y_guarda()
    {
        var servicio = new Servicio(ElPatron.Id, "Corte", 30, 20000m);
        var repo = new ServiciosFalsos(servicio);
        var caso = new EditarServicio(new BarberiasFalsas(ElPatron, LaOtra), repo);

        var resultado = await caso.EjecutarAsync("el-patron", servicio.Id, "Corte clásico", 40, 25000m);

        Assert.NotNull(resultado);
        Assert.Equal("Corte clásico", resultado.Nombre);
        Assert.Equal(1, repo.Guardados);
    }

    [Fact]
    public async Task EditarServicio_de_otra_barberia_devuelve_null_y_no_toca_nada()
    {
        var ajeno = new Servicio(LaOtra.Id, "Corte", 30, 20000m);
        var repo = new ServiciosFalsos(ajeno);
        var caso = new EditarServicio(new BarberiasFalsas(ElPatron, LaOtra), repo);

        var resultado = await caso.EjecutarAsync("el-patron", ajeno.Id, "Hackeado", 5, 1m);

        Assert.Null(resultado);
        Assert.Equal("Corte", ajeno.Nombre);
        Assert.Equal(0, repo.Guardados);
    }

    [Fact]
    public async Task EditarServicio_con_datos_invalidos_lanza_y_no_guarda()
    {
        var servicio = new Servicio(ElPatron.Id, "Corte", 30, 20000m);
        var repo = new ServiciosFalsos(servicio);
        var caso = new EditarServicio(new BarberiasFalsas(ElPatron), repo);

        await Assert.ThrowsAsync<DomainException>(() => caso.EjecutarAsync("el-patron", servicio.Id, "Corte", 30, -1m));

        Assert.Equal(0, repo.Guardados);
    }

    [Fact]
    public async Task QuitarServicio_lo_desactiva_y_deja_de_listarse()
    {
        var servicio = new Servicio(ElPatron.Id, "Corte", 30, 20000m);
        var repo = new ServiciosFalsos(servicio);
        var caso = new QuitarServicio(new BarberiasFalsas(ElPatron), repo);

        Assert.True(await caso.EjecutarAsync("el-patron", servicio.Id));

        Assert.False(servicio.Activo);
        Assert.Empty(await repo.ListarActivosAsync(ElPatron.Id));
        Assert.False(await caso.EjecutarAsync("el-patron", servicio.Id)); // ya no existe para el dueño
    }

    [Fact]
    public async Task QuitarServicio_de_otra_barberia_o_inexistente_devuelve_false()
    {
        var ajeno = new Servicio(LaOtra.Id, "Corte", 30, 20000m);
        var caso = new QuitarServicio(new BarberiasFalsas(ElPatron, LaOtra), new ServiciosFalsos(ajeno));

        Assert.False(await caso.EjecutarAsync("el-patron", ajeno.Id));
        Assert.True(ajeno.Activo);
        Assert.False(await caso.EjecutarAsync("no-existe", ajeno.Id));
    }

    // ---------- Barberos ----------

    [Fact]
    public async Task EditarBarbero_cambia_el_nombre_pero_no_el_de_otra_barberia()
    {
        var propio = new Barbero(ElPatron.Id, "Carlos");
        var ajeno = new Barbero(LaOtra.Id, "Luis");
        var repo = new BarberosFalsos(propio, ajeno);
        var caso = new EditarBarbero(new BarberiasFalsas(ElPatron, LaOtra), repo);

        Assert.NotNull(await caso.EjecutarAsync("el-patron", propio.Id, "Carlos Pérez"));
        Assert.Equal("Carlos Pérez", propio.Nombre);

        Assert.Null(await caso.EjecutarAsync("el-patron", ajeno.Id, "Hackeado"));
        Assert.Equal("Luis", ajeno.Nombre);
    }

    [Fact]
    public async Task QuitarBarbero_lo_desactiva_y_no_se_puede_quitar_dos_veces()
    {
        var barbero = new Barbero(ElPatron.Id, "Carlos");
        var repo = new BarberosFalsos(barbero);
        var caso = new QuitarBarbero(new BarberiasFalsas(ElPatron), repo);

        Assert.True(await caso.EjecutarAsync("el-patron", barbero.Id));
        Assert.False(barbero.Activo);
        Assert.False(await caso.EjecutarAsync("el-patron", barbero.Id));
    }

    [Fact]
    public async Task QuitarHorarioBarbero_quita_el_tramo_y_devuelve_el_barbero()
    {
        var barbero = new Barbero(ElPatron.Id, "Carlos");
        barbero.AgregarHorario(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        var horarioId = barbero.Horarios.Single().Id;
        var repo = new BarberosFalsos(barbero);
        var caso = new QuitarHorarioBarbero(new BarberiasFalsas(ElPatron), repo);

        var resultado = await caso.EjecutarAsync("el-patron", barbero.Id, horarioId);

        Assert.NotNull(resultado);
        Assert.Empty(resultado.Horarios);
        Assert.Equal(1, repo.Guardados);
    }

    [Fact]
    public async Task QuitarHorarioBarbero_con_horario_inexistente_o_de_otra_barberia_devuelve_null()
    {
        var propio = new Barbero(ElPatron.Id, "Carlos");
        propio.AgregarHorario(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        var ajeno = new Barbero(LaOtra.Id, "Luis");
        ajeno.AgregarHorario(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        var repo = new BarberosFalsos(propio, ajeno);
        var caso = new QuitarHorarioBarbero(new BarberiasFalsas(ElPatron, LaOtra), repo);

        Assert.Null(await caso.EjecutarAsync("el-patron", propio.Id, Guid.NewGuid()));
        Assert.Null(await caso.EjecutarAsync("el-patron", ajeno.Id, ajeno.Horarios.Single().Id));
        Assert.Single(ajeno.Horarios);
        Assert.Equal(0, repo.Guardados);
    }
}
