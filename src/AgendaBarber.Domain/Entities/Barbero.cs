namespace AgendaBarber.Domain.Entities;

public class Barbero
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid BarberiaId { get; private set; }
    public string Nombre { get; private set; } = default!;
    public bool Activo { get; private set; } = true;
    public ICollection<HorarioTrabajo> Horarios { get; private set; } = [];

    private Barbero() { }

    public Barbero(Guid barberiaId, string nombre)
    {
        Nombre = ValidarNombre(nombre);
        BarberiaId = barberiaId;
    }

    public void Renombrar(string nombre) => Nombre = ValidarNombre(nombre);

    /// <summary>
    /// Agrega un tramo de trabajo (p. ej. lunes 9:00–12:00). Un día puede tener varios tramos
    /// (mañana y tarde, con descanso en medio), pero no pueden cruzarse.
    /// </summary>
    public void AgregarHorario(DayOfWeek dia, TimeOnly inicio, TimeOnly fin)
    {
        if (!Enum.IsDefined(dia))
            throw new DomainException("El día debe ser un número entre 0 (domingo) y 6 (sábado).");

        var nuevo = new HorarioTrabajo(Id, dia, inicio, fin);
        if (Horarios.Any(h => h.Dia == dia && h.Inicio < fin && inicio < h.Fin))
            throw new DomainException("Ese tramo se cruza con otro horario del mismo día.");
        Horarios.Add(nuevo);
    }

    /// <summary>Quita un tramo de trabajo. Devuelve false si el barbero no tiene ese horario.</summary>
    public bool QuitarHorario(Guid horarioId)
    {
        var horario = Horarios.FirstOrDefault(h => h.Id == horarioId);
        if (horario is null) return false;

        Horarios.Remove(horario);
        return true;
    }

    public void Desactivar() => Activo = false;

    private static string ValidarNombre(string nombre)
    {
        nombre = nombre?.Trim() ?? "";
        if (nombre.Length is < 2 or > 80)
            throw new DomainException("El nombre del barbero debe tener entre 2 y 80 caracteres.");
        return nombre;
    }
}
