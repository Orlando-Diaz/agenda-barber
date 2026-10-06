import { Component, OnInit, inject, signal } from '@angular/core';
import { ApiService, mensajeDeError } from '../../../core/api.service';
import { pesos } from '../../../core/fechas';
import { Servicio } from '../../../core/models';
import { SesionService } from '../../../core/sesion.service';

@Component({
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
}
