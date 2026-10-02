namespace AgendaBarber.Domain;

/// <summary>Se lanza cuando se viola una regla del negocio (no es un error de programación).</summary>
public class DomainException(string message) : Exception(message);
