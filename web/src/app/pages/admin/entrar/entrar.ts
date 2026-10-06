import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { mensajeDeError } from '../../../core/api.service';
import { SesionService } from '../../../core/sesion.service';

type Modo = 'entrar' | 'crear';

/** "Barbería El Patrón" -> "barberia-el-patron" (minúsculas, sin tildes, guiones). */
export function aSlug(texto: string): string {
  return texto
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
    .slice(0, 50);
}

@Component({
  selector: 'app-entrar',
  imports: [RouterLink],
  templateUrl: './entrar.html',
  styleUrl: './entrar.css',
})
export class Entrar {
  private readonly sesion = inject(SesionService);
  private readonly router = inject(Router);

  protected readonly modo = signal<Modo>('entrar');
  protected readonly enviando = signal(false);
  protected readonly error = signal<string | null>(null);

  /** El enlace público se propone a partir del nombre, hasta que el dueño lo edite a mano. */
  protected readonly slug = signal('');
  private slugEditado = false;

  protected cambiarModo(modo: Modo): void {
    this.modo.set(modo);
    this.error.set(null);
  }

  protected escribirNombre(evento: Event): void {
    if (this.slugEditado) return;
    this.slug.set(aSlug((evento.target as HTMLInputElement).value));
  }

  protected escribirSlug(evento: Event): void {
    this.slugEditado = true;
    this.slug.set((evento.target as HTMLInputElement).value.toLowerCase());
  }

  protected enviar(evento: Event, form: HTMLFormElement): void {
    evento.preventDefault();
    if (this.enviando()) return;
    const v = (nombre: string) => (form.elements.namedItem(nombre) as HTMLInputElement | null)?.value ?? '';

    this.enviando.set(true);
    this.error.set(null);

    const peticion =
      this.modo() === 'entrar'
        ? this.sesion.login(v('email').trim(), v('password'))
        : this.sesion.registrar({
            nombreBarberia: v('nombre').trim(),
            slug: this.slug().trim(),
            telefono: v('telefono').trim() || null,
            email: v('email').trim(),
            password: v('password'),
          });

    peticion.subscribe({
      next: () => void this.router.navigateByUrl('/admin'),
      error: (e) => {
        this.enviando.set(false);
        this.error.set(mensajeDeError(e, 'No pudimos completar la solicitud. Intenta de nuevo.'));
      },
    });
  }
}
