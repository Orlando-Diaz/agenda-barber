namespace AgendaBarber.Domain.Entities;

/// <summary>Lo que ofrece la barbería: corte, barba, corte + barba...</summary>
public class Servicio
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid BarberiaId { get; private set; }
    public string Nombre { get; private set; } = default!;
    public int DuracionMinutos { get; private set; }
    public decimal Precio { get; private set; }
    public bool Activo { get; private set; } = true;

    private Servicio() { }

    public Servicio(Guid barberiaId, string nombre, int duracionMinutos, decimal precio)
    {
        (Nombre, DuracionMinutos, Precio) = Validar(nombre, duracionMinutos, precio);
        BarberiaId = barberiaId;
    }

    /// <summary>
    /// Cambia los datos del servicio. Las citas ya agendadas no se tocan: cada cita guarda el precio
    /// y la hora de fin que tenía al reservarse. El cambio aplica a las reservas nuevas.
    /// </summary>
    public void Editar(string nombre, int duracionMinutos, decimal precio) =>
        (Nombre, DuracionMinutos, Precio) = Validar(nombre, duracionMinutos, precio);

    public void Desactivar() => Activo = false;

    // Las mismas reglas al crear y al editar: así no pueden quedar desalineadas.
    private static (string Nombre, int DuracionMinutos, decimal Precio) Validar(string nombre, int duracionMinutos, decimal precio)
    {
        nombre = nombre?.Trim() ?? "";
        if (nombre.Length is < 2 or > 80)
            throw new DomainException("El nombre del servicio debe tener entre 2 y 80 caracteres.");
        if (duracionMinutos is < 5 or > 480)
            throw new DomainException("La duración debe estar entre 5 y 480 minutos.");
        if (precio < 0)
            throw new DomainException("El precio no puede ser negativo.");

        return (nombre, duracionMinutos, precio);
    }
}
