import { Injectable, signal } from '@angular/core';

export type Tema = 'claro' | 'oscuro';
const CLAVE = 'agendabarber.tema';

/** Modo claro u oscuro: se recuerda lo que el usuario eligió y, si no eligió, se usa lo que pide su sistema. */
@Injectable({ providedIn: 'root' })
export class TemaService {
  readonly tema = signal<Tema>(this.inicial());

  constructor() {
    this.aplicar(this.tema());
  }

  alternar(): void {
    const nuevo: Tema = this.tema() === 'oscuro' ? 'claro' : 'oscuro';
    this.tema.set(nuevo);
    this.aplicar(nuevo);
    try {
      localStorage.setItem(CLAVE, nuevo);
    } catch {
      // sin almacenamiento disponible: el tema solo dura mientras la página esté abierta
    }
  }

  private inicial(): Tema {
    try {
      const guardado = localStorage.getItem(CLAVE);
      if (guardado === 'claro' || guardado === 'oscuro') return guardado;
    } catch {
      // se ignora: se usa la preferencia del sistema
    }
    return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'oscuro' : 'claro';
  }

  private aplicar(tema: Tema): void {
    document.documentElement.dataset['tema'] = tema;
    document.querySelector('meta[name="theme-color"]')?.setAttribute('content', tema === 'oscuro' ? '#15286b' : '#1d3fa8');
  }
}
