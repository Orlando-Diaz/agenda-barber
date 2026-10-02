using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Citas;

/// <summary>Una cita lista para mostrar: ya trae el nombre del barbero y del servicio (no solo sus ids).</summary>
public record CitaDetalle(
    Guid Id, DateTime InicioUtc, DateTime FinUtc, string Barbero, string Servicio,
    string ClienteNombre, string ClienteTelefono, decimal Precio, EstadoCita Estado);
