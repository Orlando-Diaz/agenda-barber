namespace AgendaBarber.Domain.Entities;

public enum EstadoCita
{
    Pendiente = 0,
    Confirmada = 1,
    Cancelada = 2,
    Atendida = 3,
    NoAsistio = 4,
}

public class Cita
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid BarberiaId { get; private set; }
    public Guid BarberoId { get; private set; }
    public Guid ServicioId { get; private set; }

    /// <summary>Siempre en UTC. A hora de Colombia se convierte solo al mostrarla.</summary>
    public DateTime InicioUtc { get; private set; }
    public DateTime FinUtc { get; private set; }

    public string ClienteNombre { get; private set; } = default!;
    public string ClienteTelefono { get; private set; } = default!;

    /// <summary>Precio del servicio al momento de agendar: si luego cambia el precio, esta cita no cambia.</summary>
    public decimal Precio { get; private set; }

    public EstadoCita Estado { get; private set; } = EstadoCita.Pendiente;
    public DateTime CreadaEnUtc { get; private set; }

    private Cita() { }

    /// <summary>Una cita pendiente o confirmada ocupa el hueco; una cancelada lo libera.</summary>
    public bool OcupaAgenda => Estado is EstadoCita.Pendiente or EstadoCita.Confirmada;

    /// <summary>Intervalos [inicio, fin): una cita que termina a las 10:00 NO choca con otra que empieza a las 10:00.</summary>
    public bool SeSolapaCon(DateTime inicioUtc, DateTime finUtc) => InicioUtc < finUtc && inicioUtc < FinUtc;

    public static Cita Agendar(
        Barbero barbero, Servicio servicio, DateTime inicioUtc,
        string clienteNombre, string clienteTelefono, DateTime ahoraUtc)
    {
        if (!barbero.Activo) throw new DomainException("El barbero no está disponible.");
        if (!servicio.Activo) throw new DomainException("El servicio no está disponible.");
        if (barbero.BarberiaId != servicio.BarberiaId)
            throw new DomainException("El barbero y el servicio no son de la misma barbería.");
        if (inicioUtc.Kind != DateTimeKind.Utc)
            throw new DomainException("La hora de inicio debe estar en UTC.");
        if (inicioUtc <= ahoraUtc)
            throw new DomainException("No se puede agendar una cita en el pasado.");

        clienteNombre = clienteNombre?.Trim() ?? "";
        if (clienteNombre.Length is < 2 or > 100)
            throw new DomainException("El nombre debe tener entre 2 y 100 caracteres.");

        var telefono = new string((clienteTelefono ?? "").Where(char.IsDigit).ToArray());
        if (telefono.Length is < 7 or > 15)
            throw new DomainException("El teléfono no es válido.");

        return new Cita
        {
            BarberiaId = barbero.BarberiaId,
            BarberoId = barbero.Id,
            ServicioId = servicio.Id,
            InicioUtc = inicioUtc,
            FinUtc = inicioUtc.AddMinutes(servicio.DuracionMinutos),
            ClienteNombre = clienteNombre,
            ClienteTelefono = telefono,
            Precio = servicio.Precio,
            CreadaEnUtc = ahoraUtc,
        };
    }

    public void Confirmar()
    {
        if (Estado != EstadoCita.Pendiente)
            throw new DomainException("Solo se puede confirmar una cita pendiente.");
        Estado = EstadoCita.Confirmada;
    }

    public void Cancelar()
    {
        if (!OcupaAgenda)
            throw new DomainException("Esta cita ya no se puede cancelar.");
        Estado = EstadoCita.Cancelada;
    }

    public void MarcarAtendida(DateTime ahoraUtc) => Cerrar(EstadoCita.Atendida, ahoraUtc);

    public void MarcarNoAsistio(DateTime ahoraUtc) => Cerrar(EstadoCita.NoAsistio, ahoraUtc);

    private void Cerrar(EstadoCita nuevoEstado, DateTime ahoraUtc)
    {
        if (!OcupaAgenda)
            throw new DomainException("Esta cita ya fue cerrada o cancelada.");
        if (ahoraUtc < InicioUtc)
            throw new DomainException("La cita todavía no ha empezado.");
        Estado = nuevoEstado;
    }
}
