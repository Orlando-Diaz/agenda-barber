// Las "formas" de los datos que devuelve la API (en camelCase, como las manda ASP.NET).

export interface Barberia {
  id: string;
  nombre: string;
  slug: string;
  telefono: string | null;
}

export interface Servicio {
  id: string;
  nombre: string;
  duracionMinutos: number;
  precio: number;
}

export interface HorarioBarbero {
  dia: number; // 0 = domingo ... 6 = sábado
  nombreDia: string;
  inicio: string;
  fin: string;
}

export interface Barbero {
  id: string;
  nombre: string;
  horarios: HorarioBarbero[];
}

export interface HoraDisponible {
  inicioUtc: string; // lo que se manda al reservar
  hora: string; // "09:30", en hora local de la barbería: lo que se muestra
}

export interface NuevaCita {
  barberoId: string;
  servicioId: string;
  inicioUtc: string;
  clienteNombre: string;
  clienteTelefono: string;
}

export interface CitaCreada {
  id: string;
  inicioUtc: string;
  finUtc: string;
  estado: string;
  precio: number;
}

/** Lo que se muestra en el tiquete cuando la reserva salió bien. */
export interface CitaConfirmada {
  servicio: string;
  barbero: string;
  dia: string; // "2026-10-05"
  hora: string; // "09:30"
  precio: number;
  cliente: string;
}

// ---------- Panel del dueño ----------

/** Lo que responden login y registro. */
export interface Sesion {
  token: string;
  expiraUtc: string;
  slug: string;
  nombreBarberia: string;
}

export interface NuevoRegistro {
  nombreBarberia: string;
  slug: string;
  telefono: string | null;
  email: string;
  password: string;
}

export type EstadoCita = 'Pendiente' | 'Confirmada' | 'Cancelada' | 'Atendida' | 'NoAsistio';
export type AccionCita = 'confirmar' | 'cancelar' | 'atendida' | 'no-asistio';

/** Una cita de la agenda del día (horas ya en hora local de la barbería). */
export interface ItemAgenda {
  id: string;
  horaInicio: string;
  horaFin: string;
  barbero: string;
  servicio: string;
  clienteNombre: string;
  clienteTelefono: string;
  precio: number;
  estado: EstadoCita;
}
