import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { ApiService } from './api.service';
import { NuevoRegistro, Sesion } from './models';

const CLAVE = 'agendabarber.sesion';

/** Quién está conectado. La sesión vive en un signal y se copia a localStorage para sobrevivir a recargar la página. */
@Injectable({ providedIn: 'root' })
export class SesionService {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);

  private readonly actual = signal<Sesion | null>(this.leer());

  /** La sesión, o null si no hay o ya venció. */
  readonly sesion = computed(() => {
    const s = this.actual();
    return s && Date.parse(s.expiraUtc) > Date.now() ? s : null;
  });
  readonly activa = computed(() => this.sesion() !== null);

  login(email: string, password: string) {
    return this.api.login(email, password).pipe(tap((s) => this.guardar(s)));
  }

  registrar(datos: NuevoRegistro) {
    return this.api.registro(datos).pipe(tap((s) => this.guardar(s)));
  }

  cerrar(): void {
    this.actual.set(null);
    try {
      localStorage.removeItem(CLAVE);
    } catch {
      /* sin almacenamiento (modo privado): la sesión ya quedó cerrada en memoria */
    }
  }

  /** Cierra la sesión y manda al login. */
  salir(): void {
    this.cerrar();
    void this.router.navigateByUrl('/admin/entrar');
  }

  private guardar(s: Sesion): void {
    this.actual.set(s);
    try {
      localStorage.setItem(CLAVE, JSON.stringify(s));
    } catch {
      /* si no se puede guardar, la sesión dura mientras la pestaña siga abierta */
    }
  }

  private leer(): Sesion | null {
    try {
      const crudo = localStorage.getItem(CLAVE);
      if (!crudo) return null;
      const s = JSON.parse(crudo) as Sesion;
      return s?.token && s.slug && s.expiraUtc ? s : null;
    } catch {
      return null;
    }
  }
}
