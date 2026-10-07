import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from './api.config';
import {
  AccionCita, Barberia, Barbero, CitaCreada, HoraDisponible, ItemAgenda, NuevaCita, NuevoRegistro, NuevoServicio, Servicio, Sesion,
} from './models';

/** Todas las llamadas a la API pasan por aquí: los componentes no conocen las URLs. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly url = inject(API_URL);

  barberia(slug: string): Observable<Barberia> {
    return this.http.get<Barberia>(`${this.url}/barberias/${slug}`);
  }

  servicios(slug: string): Observable<Servicio[]> {
    return this.http.get<Servicio[]>(`${this.url}/barberias/${slug}/servicios`);
  }

  barberos(slug: string): Observable<Barbero[]> {
    return this.http.get<Barbero[]>(`${this.url}/barberias/${slug}/barberos`);
  }

  disponibilidad(slug: string, barberoId: string, servicioId: string, fecha: string): Observable<HoraDisponible[]> {
    const params = new HttpParams().set('barberoId', barberoId).set('servicioId', servicioId).set('fecha', fecha);
    return this.http.get<HoraDisponible[]>(`${this.url}/barberias/${slug}/disponibilidad`, { params });
  }

  reservar(slug: string, cita: NuevaCita): Observable<CitaCreada> {
    return this.http.post<CitaCreada>(`${this.url}/barberias/${slug}/citas`, cita);
  }

  // ---------- Panel del dueño (el interceptor agrega el token) ----------

  login(email: string, password: string): Observable<Sesion> {
    return this.http.post<Sesion>(`${this.url}/auth/login`, { email, password });
  }

  registro(datos: NuevoRegistro): Observable<Sesion> {
    return this.http.post<Sesion>(`${this.url}/auth/registro`, datos);
  }

  agenda(slug: string, fecha: string): Observable<ItemAgenda[]> {
    const params = new HttpParams().set('fecha', fecha);
    return this.http.get<ItemAgenda[]>(`${this.url}/barberias/${slug}/agenda`, { params });
  }

  crearServicio(slug: string, servicio: NuevoServicio): Observable<Servicio> {
    return this.http.post<Servicio>(`${this.url}/barberias/${slug}/servicios`, servicio);
  }

  crearBarbero(slug: string, nombre: string): Observable<Barbero> {
    return this.http.post<Barbero>(`${this.url}/barberias/${slug}/barberos`, { nombre });
  }

  /** Devuelve el barbero ya con todos sus horarios. */
  agregarHorario(slug: string, barberoId: string, dia: number, inicio: string, fin: string): Observable<Barbero> {
    return this.http.post<Barbero>(`${this.url}/barberias/${slug}/barberos/${barberoId}/horarios`, { dia, inicio, fin });
  }

  editarServicio(slug: string, servicioId: string, datos: NuevoServicio): Observable<Servicio> {
    return this.http.put<Servicio>(`${this.url}/barberias/${slug}/servicios/${servicioId}`, datos);
  }

  quitarServicio(slug: string, servicioId: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/barberias/${slug}/servicios/${servicioId}`);
  }

  editarBarbero(slug: string, barberoId: string, nombre: string): Observable<Barbero> {
    return this.http.put<Barbero>(`${this.url}/barberias/${slug}/barberos/${barberoId}`, { nombre });
  }

  quitarBarbero(slug: string, barberoId: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/barberias/${slug}/barberos/${barberoId}`);
  }

  /** Devuelve el barbero con los horarios que le quedan. */
  quitarHorario(slug: string, barberoId: string, horarioId: string): Observable<Barbero> {
    return this.http.delete<Barbero>(`${this.url}/barberias/${slug}/barberos/${barberoId}/horarios/${horarioId}`);
  }

  cambiarEstado(slug: string, citaId: string, accion: AccionCita): Observable<{ id: string; estado: string }> {
    return this.http.post<{ id: string; estado: string }>(`${this.url}/barberias/${slug}/citas/${citaId}/${accion}`, {});
  }
}

/** La API responde errores como { "error": "mensaje" }: aquí se saca ese mensaje. */
export function mensajeDeError(error: unknown, porDefecto: string): string {
  if (error instanceof HttpErrorResponse) {
    if (typeof error.error?.error === 'string') return error.error.error;
    if (error.status === 0) return 'No pudimos conectar con el servidor. Revisa tu conexión e intenta de nuevo.';
  }
  return porDefecto;
}
