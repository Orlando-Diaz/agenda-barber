import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService, mensajeDeError } from '../../core/api.service';
import { pesos } from '../../core/fechas';
import { BarberiaResumen } from '../../core/models';

/** Para buscar sin importar mayúsculas ni tildes: "patron" encuentra "El Patrón". */
function limpiar(texto: string): string {
  return texto.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().trim();
}

@Component({
  selector: 'app-inicio',
  imports: [RouterLink],
  templateUrl: './inicio.html',
  styleUrl: './inicio.css',
})
export class Inicio implements OnInit {
  private readonly api = inject(ApiService);

  protected readonly barberias = signal<BarberiaResumen[] | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly busqueda = signal('');

  protected readonly visibles = computed(() => {
    const q = limpiar(this.busqueda());
    return (this.barberias() ?? []).filter((b) => !q || limpiar(b.nombre).includes(q));
  });

  /** Los servidores gratuitos se duermen: si tarda, se avisa que está despertando. */
  protected readonly tardando = signal(false);

  ngOnInit(): void {
    this.cargar();
  }

  protected cargar(): void {
    this.error.set(null);
    this.tardando.set(false);
    const aviso = setTimeout(() => this.tardando.set(true), 4000);
    this.api.barberias().subscribe({
      next: (lista) => {
        clearTimeout(aviso);
        this.tardando.set(false);
        this.barberias.set(lista);
      },
      error: (e) => {
        clearTimeout(aviso);
        this.tardando.set(false);
        this.error.set(mensajeDeError(e, 'No pudimos cargar las barberías.'));
      },
    });
  }

  protected precio(valor: number): string {
    return pesos(valor);
  }

  protected arriba(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  protected foto(b: BarberiaResumen): string | null {
    return b.fotoVersion == null ? null : this.api.urlFoto(b.slug, b.fotoVersion);
  }

  protected escribir(evento: Event): void {
    this.busqueda.set((evento.target as HTMLInputElement).value);
  }
}
