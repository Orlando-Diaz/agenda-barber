import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from './api.config';
import {
  AccionCita, Barberia, Barbero, CitaCreada, HoraDisponible, ItemAgenda, NuevaCita, NuevoRegistro, Servicio, Sesion,
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
