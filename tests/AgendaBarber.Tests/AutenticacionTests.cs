using AgendaBarber.Application.Autenticacion;
using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain;
using AgendaBarber.Domain.Entities;
using AgendaBarber.Infrastructure.Seguridad;

namespace AgendaBarber.Tests;

/// <summary>Registro e inicio de sesión del dueño, con repositorios falsos en memoria.</summary>
public class AutenticacionTests
{
    // ---------- Falsos ----------

    private sealed class BarberiasFalsas : IBarberiaRepositorio
    {
        public List<Barberia> Lista { get; } = [];
        public Task<bool> ExisteSlugAsync(string slug, CancellationToken ct = default) => Task.FromResult(Lista.Any(b => b.Slug == slug));
        public Task AgregarAsync(Barberia barberia, CancellationToken ct = default) { Lista.Add(barberia); return Task.CompletedTask; }
        public Task<Barberia?> ObtenerPorSlugAsync(string slug, CancellationToken ct = default) => Task.FromResult(Lista.FirstOrDefault(b => b.Slug == slug));
        public Task<Barberia?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Lista.FirstOrDefault(b => b.Id == id));
        public Task<IReadOnlyList<Barberia>> ListarReservablesAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Barberia>>([]);
    }

    private sealed class UsuariosFalsos(BarberiasFalsas barberias) : IUsuarioRepositorio
    {
        public List<Usuario> Lista { get; } = [];
        public Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default) => Task.FromResult(Lista.Any(u => u.Email == email));
        public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct = default) => Task.FromResult(Lista.FirstOrDefault(u => u.Email == email));
        public Task AgregarDuenoAsync(Barberia barberia, Usuario usuario, CancellationToken ct = default)
        {
            barberias.Lista.Add(barberia);
            Lista.Add(usuario);
            return Task.CompletedTask;
        }
    }

    /// <summary>Hash falso y rápido: "hash:" + contraseña (el real se prueba aparte).</summary>
    private sealed class HasheadorFalso : IHasheadorContrasenas
    {
        public string Hashear(string contrasena) => "hash:" + contrasena;
        public bool Verificar(string hashGuardado, string contrasena) => hashGuardado == "hash:" + contrasena;
    }

    private sealed class GeneradorFalso : IGeneradorToken
    {
        public TokenGenerado Generar(Usuario usuario, Barberia barberia) =>
            new($"token-de-{usuario.Email}-{barberia.Slug}", DateTime.UtcNow.AddHours(1));
    }

    private static (RegistrarDueno Registrar, IniciarSesion Iniciar, BarberiasFalsas Barberias, UsuariosFalsos Usuarios) Crear()
    {
        var barberias = new BarberiasFalsas();
        var usuarios = new UsuariosFalsos(barberias);
        var hasheador = new HasheadorFalso();
        var generador = new GeneradorFalso();
        return (new RegistrarDueno(barberias, usuarios, hasheador, generador),
                new IniciarSesion(barberias, usuarios, hasheador, generador),
                barberias, usuarios);
    }

    // ---------- Registro ----------

    [Fact]
    public async Task Registrar_crea_barberia_y_dueno_y_devuelve_sesion()
    {
        var (registrar, _, barberias, usuarios) = Crear();

        var sesion = await registrar.EjecutarAsync("El Patrón", "el-patron", "3001234567", "Dueno@Correo.com", "ClaveSegura123");

        Assert.Equal("el-patron", sesion.Slug);
        Assert.Equal("El Patrón", sesion.NombreBarberia);
        Assert.False(string.IsNullOrEmpty(sesion.Token));
        Assert.Single(barberias.Lista);
        Assert.Equal("dueno@correo.com", Assert.Single(usuarios.Lista).Email); // correo normalizado
    }

    [Fact]
    public async Task Registrar_no_guarda_la_contrasena_en_claro()
    {
        var (registrar, _, _, usuarios) = Crear();
        await registrar.EjecutarAsync("El Patrón", "el-patron", null, "a@b.co", "ClaveSegura123");
        Assert.DoesNotContain("ClaveSegura123", Assert.Single(usuarios.Lista).PasswordHash.Replace("hash:", ""));
    }

    [Theory]
    [InlineData("corta")]
    [InlineData("")]
    public async Task Registrar_rechaza_contrasena_corta(string password)
    {
        var (registrar, _, barberias, _) = Crear();
        await Assert.ThrowsAsync<DomainException>(() => registrar.EjecutarAsync("El Patrón", "el-patron", null, "a@b.co", password));
        Assert.Empty(barberias.Lista);
    }

    [Fact]
    public async Task Registrar_rechaza_slug_repetido()
    {
        var (registrar, _, _, _) = Crear();
        await registrar.EjecutarAsync("El Patrón", "el-patron", null, "a@b.co", "ClaveSegura123");
        await Assert.ThrowsAsync<DomainException>(() => registrar.EjecutarAsync("Otra", "el-patron", null, "c@d.co", "ClaveSegura123"));
    }

    [Fact]
    public async Task Registrar_rechaza_correo_repetido_aunque_cambien_mayusculas()
    {
        var (registrar, _, _, _) = Crear();
        await registrar.EjecutarAsync("El Patrón", "el-patron", null, "a@b.co", "ClaveSegura123");
        await Assert.ThrowsAsync<DomainException>(() => registrar.EjecutarAsync("Otra", "otra", null, " A@B.CO ", "ClaveSegura123"));
    }

    [Fact]
    public async Task Registrar_rechaza_correo_invalido()
    {
        var (registrar, _, barberias, _) = Crear();
        await Assert.ThrowsAsync<DomainException>(() => registrar.EjecutarAsync("El Patrón", "el-patron", null, "no-es-correo", "ClaveSegura123"));
        Assert.Empty(barberias.Lista);
    }

    // ---------- Inicio de sesión ----------

    [Fact]
    public async Task Iniciar_sesion_con_credenciales_correctas()
    {
        var (registrar, iniciar, _, _) = Crear();
        await registrar.EjecutarAsync("El Patrón", "el-patron", null, "dueno@correo.com", "ClaveSegura123");

        var sesion = await iniciar.EjecutarAsync("  DUENO@correo.com ", "ClaveSegura123");

        Assert.NotNull(sesion);
        Assert.Equal("el-patron", sesion.Slug);
    }

    [Fact]
    public async Task Iniciar_sesion_con_contrasena_incorrecta_devuelve_null()
    {
        var (registrar, iniciar, _, _) = Crear();
        await registrar.EjecutarAsync("El Patrón", "el-patron", null, "dueno@correo.com", "ClaveSegura123");
        Assert.Null(await iniciar.EjecutarAsync("dueno@correo.com", "otra-clave"));
    }

    [Fact]
    public async Task Iniciar_sesion_con_correo_inexistente_devuelve_null()
    {
        var (_, iniciar, _, _) = Crear();
        Assert.Null(await iniciar.EjecutarAsync("nadie@correo.com", "ClaveSegura123"));
    }

    // ---------- Hasheador real ----------

    [Fact]
    public void Hasheador_verifica_la_contrasena_correcta_y_rechaza_otras()
    {
        var h = new HasheadorContrasenas();
        var hash = h.Hashear("ClaveSegura123");

        Assert.True(h.Verificar(hash, "ClaveSegura123"));
        Assert.False(h.Verificar(hash, "claveSegura123"));
    }

    [Fact]
    public void Hasheador_usa_sal_distinta_cada_vez()
    {
        var h = new HasheadorContrasenas();
        Assert.NotEqual(h.Hashear("ClaveSegura123"), h.Hashear("ClaveSegura123"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("basura")]
    [InlineData("v1.600000.no-es-base64.tampoco")]
    public void Hasheador_rechaza_hashes_mal_formados_sin_lanzar_excepcion(string hashRaro)
    {
        Assert.False(new HasheadorContrasenas().Verificar(hashRaro, "ClaveSegura123"));
    }
}
