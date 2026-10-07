import { RouterLink } from '@angular/router';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ApiService, mensajeDeError } from '../../../core/api.service';
import { pesos } from '../../../core/fechas';
import { Servicio } from '../../../core/models';
import { SesionService } from '../../../core/sesion.service';

@Component({
  imports: [RouterLink],
  selector: 'app-servicios',
  templateUrl: './servicios.html',
  styleUrl: './servicios.css',
})
export class Servicios implements OnInit {
  private readonly api = inject(ApiService);
  private readonly slug = inject(SesionService).sesion()?.slug ?? '';

  protected readonly servicios = signal<Servicio[] | null>(null);
  protected readonly errorCarga = signal<string | null>(null);
  protected readonly enviando = signal(false);
  protected readonly error = signal<string | null>(null);

  /** Servicio que se está editando / que espera confirmación de quitar, y el error de esa fila. */
  protected readonly editando = signal<string | null>(null);
  protected readonly confirmando = signal<string | null>(null);
  protected readonly ocupado = signal<string | null>(null);
  protected readonly errorFila = signal<{ id: string; mensaje: string } | null>(null);

  ngOnInit(): void {
    this.cargar();
  }

  protected cargar(): void {
    this.errorCarga.set(null);
    this.api.servicios(this.slug).subscribe({
      next: (lista) => this.servicios.set(lista),
      error: (e) => this.errorCarga.set(mensajeDeError(e, 'No pudimos cargar los servicios.')),
    });
  }

  protected precio(s: Servicio): string {
    return pesos(s.precio);
  }

  protected crear(evento: Event, form: HTMLFormElement): void {
    evento.preventDefault();
    if (this.enviando()) return;
    const campo = (n: string) => (form.elements.namedItem(n) as HTMLInputElement).value;

    this.enviando.set(true);
    this.error.set(null);
    this.api
      .crearServicio(this.slug, {
        nombre: campo('nombre').trim(),
        duracionMinutos: Number(campo('duracion')),
        precio: Number(campo('precio')),
      })
      .subscribe({
        next: (nuevo) => {
          this.servicios.update((lista) => [...(lista ?? []), nuevo]);
          this.enviando.set(false);
          form.reset();
        },
        error: (e) => {
          this.enviando.set(false);
          this.error.set(mensajeDeError(e, 'No pudimos crear el servicio.'));
        },
      });
  }

  protected editar(s: Servicio): void {
    this.editando.set(s.id);
    this.confirmando.set(null);
    this.errorFila.set(null);
  }

  protected cancelar(): void {
    this.editando.set(null);
    this.confirmando.set(null);
    this.errorFila.set(null);
  }

  protected guardarEdicion(evento: Event, form: HTMLFormElement, s: Servicio): void {
    evento.preventDefault();
    if (this.ocupado()) return;
    const campo = (n: string) => (form.elements.namedItem(n) as HTMLInputElement).value;

    this.ocupado.set(s.id);
    this.errorFila.set(null);
    this.api
      .editarServicio(this.slug, s.id, {
        nombre: campo('nombre').trim(),
        duracionMinutos: Number(campo('duracion')),
        precio: Number(campo('precio')),
      })
      .subscribe({
        next: (actualizado) => {
          this.servicios.update((lista) => lista?.map((x) => (x.id === actualizado.id ? actualizado : x)) ?? null);
          this.ocupado.set(null);
          this.editando.set(null);
        },
        error: (e) => {
          this.ocupado.set(null);
          this.errorFila.set({ id: s.id, mensaje: mensajeDeError(e, 'No pudimos guardar los cambios.') });
        },
      });
  }

  /** Dos pasos: el primer clic pide confirmación, el segundo quita. */
  protected quitar(s: Servicio): void {
    if (this.confirmando() !== s.id) {
      this.confirmando.set(s.id);
      this.editando.set(null);
      this.errorFila.set(null);
      return;
    }
    if (this.ocupado()) return;
    this.ocupado.set(s.id);
    this.api.quitarServicio(this.slug, s.id).subscribe({
      next: () => {
        this.servicios.update((lista) => lista?.filter((x) => x.id !== s.id) ?? null);
        this.ocupado.set(null);
        this.confirmando.set(null);
      },
      error: (e) => {
        this.ocupado.set(null);
        this.errorFila.set({ id: s.id, mensaje: mensajeDeError(e, 'No pudimos quitar el servicio.') });
      },
    });
  }
}
