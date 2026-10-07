namespace AgendaBarber.Domain.Entities;

/// <summary>La foto de portada de una barbería. Va en su propia tabla para no cargarla cada vez que se lee la barbería.</summary>
public class FotoBarberia
{
    public const int TamanoMaximo = 400_000; // bytes: la web la reduce antes de enviarla
    private static readonly string[] Tipos = ["image/jpeg", "image/png", "image/webp"];

    public Guid BarberiaId { get; private set; }
    public byte[] Datos { get; private set; } = default!;
    public string Tipo { get; private set; } = default!;

    private FotoBarberia() { }

    public FotoBarberia(Guid barberiaId, byte[] datos, string tipo)
    {
        Validar(datos, tipo);
        BarberiaId = barberiaId;
        Datos = datos;
        Tipo = tipo;
    }

    public void Reemplazar(byte[] datos, string tipo)
    {
        Validar(datos, tipo);
        Datos = datos;
        Tipo = tipo;
    }

    private static void Validar(byte[]? datos, string? tipo)
    {
        if (datos is null || datos.Length == 0)
            throw new DomainException("La foto está vacía.");
        if (datos.Length > TamanoMaximo)
            throw new DomainException("La foto es muy pesada. Prueba con una más pequeña.");
        if (tipo is null || !Tipos.Contains(tipo))
            throw new DomainException("La foto debe ser JPG, PNG o WebP.");
    }
}
