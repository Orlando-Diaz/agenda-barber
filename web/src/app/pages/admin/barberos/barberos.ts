import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { concatMap, from, last } from 'rxjs';
import { ApiService, mensajeDeError } from '../../../core/api.service';
import { DIAS_LUNES_A_DOMINGO, nombreDia, textoHora } from '../../../core/fechas';
import { Barbero } from '../../../core/models';
import { SesionService } from '../../../core/sesion.service';

interface DiaAgrupado {
  nombre: string;
  tramos: string[];
}

@Component({
  selector: 'app-barberos',
  templateUrl: './barberos.html',
  styleUrl: './barberos.css',
})
export class Barberos implements OnInit {
  private readonly api = inject(ApiService);
  private readonly slug = inject(SesionService).sesion()?.slug ?? '';

  protected readonly dias = DIAS_LUNES_A_DOMINGO.map((numero) => ({ numero, nombre: nombreDia(numero) }));

  protected readonly barberos = signal<Barbero[] | null>(null);
  protected readonly errorCarga = signal<string | null>(null);

  protected readonly enviandoBarbero = signal(false);
  protected readonly errorBarbero = signal<string | null>(null);

  /** Barbero al que se le está guardando un horario, y el error de ese formulario. */
  protected readonly guardando = signal<string | null>(null);
  protected readonly errorHorario = signal<{ id: string; mensaje: string } | null>(null);

  /** Cada barbero con sus horarios agrupados por día, en orden de semana. */
  protected readonly vista = computed(() =>
    (this.barberos() ?? []).map((b) => ({
      ...b,
      porDia: DIAS_LUNES_A_DOMINGO.flatMap((dia): DiaAgrupado[] => {
        const tramos = b.horarios
          .filter((h) => h.dia === dia)
          .map((h) => `${textoHora(h.inicio)} – ${textoHora(h.fin)}`);
        return tramos.length ? [{ nombre: nombreDia(dia), tramos }] : [];
      }),
    })),
  );

  ngOnInit(): void {
    this.cargar();
  }

  protected cargar(): void {
    this.errorCarga.set(null);
    this.api.barberos(this.slug).subscribe({
      next: (lista) => this.barberos.set(lista),
      error: (e) => this.errorCarga.set(mensajeDeError(e, 'No pudimos cargar los barberos.')),
    });
  }

  protected crearBarbero(evento: Event, form: HTMLFormElement): void {
    evento.preventDefault();
    if (this.enviandoBarbero()) return;
    const nombre = (form.elements.namedItem('nombre') as HTMLInputElement).value.trim();

    this.enviandoBarbero.set(true);
    this.errorBarbero.set(null);
    this.api.crearBarbero(this.slug, nombre).subscribe({
      next: (nuevo) => {
        this.barberos.update((lista) => [...(lista ?? []), nuevo]);
        this.enviandoBarbero.set(false);
        form.reset();
      },
      error: (e) => {
        this.enviandoBarbero.set(false);
        this.errorBarbero.set(mensajeDeError(e, 'No pudimos crear el barbero.'));
      },
    });
  }

  /** Un mismo horario para varios días: la API recibe un día por llamada, así que se envían una tras otra. */
  protected agregarHorario(evento: Event, form: HTMLFormElement, barbero: Barbero): void {
    evento.preventDefault();
    if (this.guardando()) return;

    const dias = Array.from(form.querySelectorAll<HTMLInputElement>('input[name="dia"]:checked')).map((c) => Number(c.value));
    const inicio = (form.elements.namedItem('inicio') as HTMLInputElement).value;
    const fin = (form.elements.namedItem('fin') as HTMLInputElement).value;

    if (dias.length === 0) {
      this.errorHorario.set({ id: barbero.id, mensaje: 'Marca al menos un día.' });
      return;
    }
    if (!inicio || !fin) {
      this.errorHorario.set({ id: barbero.id, mensaje: 'Indica la hora de inicio y la de fin.' });
      return;
    }

    this.guardando.set(barbero.id);
    this.errorHorario.set(null);

    from(dias)
      .pipe(
        concatMap((dia) => this.api.agregarHorario(this.slug, barbero.id, dia, inicio, fin)),
        last(),
      )
      .subscribe({
        next: (actualizado) => {
          this.reemplazar(actualizado);
          this.guardando.set(null);
          form.reset();
        },
        error: (e) => {
          this.guardando.set(null);
          this.errorHorario.set({ id: barbero.id, mensaje: mensajeDeError(e, 'No pudimos guardar el horario.') });
          this.cargar(); // si falló a la mitad, algunos días ya quedaron guardados: se muestra el estado real
        },
      });
  }

  private reemplazar(actualizado: Barbero): void {
    this.barberos.update((lista) => lista?.map((b) => (b.id === actualizado.id ? actualizado : b)) ?? null);
  }
}
