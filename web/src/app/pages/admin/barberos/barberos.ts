import { RouterLink } from '@angular/router';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { concatMap, from, last } from 'rxjs';
import { ApiService, mensajeDeError } from '../../../core/api.service';
import { DIAS_LUNES_A_DOMINGO, nombreDia, textoHora } from '../../../core/fechas';
import { Barbero } from '../../../core/models';
import { SesionService } from '../../../core/sesion.service';

interface DiaAgrupado {
  nombre: string;
  tramos: { id: string; texto: string }[];
}

@Component({
  imports: [RouterLink],
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

  /** Barbero que se renombra / que espera confirmación de quitar / ocupado, y el error de esa tarjeta. */
  protected readonly renombrando = signal<string | null>(null);
  protected readonly confirmando = signal<string | null>(null);
  protected readonly ocupado = signal<string | null>(null);
  protected readonly errorBarberoFila = signal<{ id: string; mensaje: string } | null>(null);

  /** Cada barbero con sus horarios agrupados por día, en orden de semana. */
  protected readonly vista = computed(() =>
    (this.barberos() ?? []).map((b) => ({
      ...b,
      porDia: DIAS_LUNES_A_DOMINGO.flatMap((dia): DiaAgrupado[] => {
        const tramos = b.horarios
          .filter((h) => h.dia === dia)
          .map((h) => ({ id: h.id, texto: `${textoHora(h.inicio)} – ${textoHora(h.fin)}` }));
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

  protected renombrar(b: Barbero): void {
    this.renombrando.set(b.id);
    this.confirmando.set(null);
    this.errorBarberoFila.set(null);
  }

  protected cancelar(): void {
    this.renombrando.set(null);
    this.confirmando.set(null);
    this.errorBarberoFila.set(null);
  }

  protected guardarNombre(evento: Event, form: HTMLFormElement, b: Barbero): void {
    evento.preventDefault();
    if (this.ocupado()) return;
    const nombre = (form.elements.namedItem('nombre') as HTMLInputElement).value.trim();

    this.ocupado.set(b.id);
    this.errorBarberoFila.set(null);
    this.api.editarBarbero(this.slug, b.id, nombre).subscribe({
      next: (actualizado) => {
        this.reemplazar(actualizado);
        this.ocupado.set(null);
        this.renombrando.set(null);
      },
      error: (e) => {
        this.ocupado.set(null);
        this.errorBarberoFila.set({ id: b.id, mensaje: mensajeDeError(e, 'No pudimos cambiar el nombre.') });
      },
    });
  }

  /** Dos pasos: el primer clic pide confirmación, el segundo quita. */
  protected quitar(b: Barbero): void {
    if (this.confirmando() !== b.id) {
      this.confirmando.set(b.id);
      this.renombrando.set(null);
      this.errorBarberoFila.set(null);
      return;
    }
    if (this.ocupado()) return;
    this.ocupado.set(b.id);
    this.api.quitarBarbero(this.slug, b.id).subscribe({
      next: () => {
        this.barberos.update((lista) => lista?.filter((x) => x.id !== b.id) ?? null);
        this.ocupado.set(null);
        this.confirmando.set(null);
      },
      error: (e) => {
        this.ocupado.set(null);
        this.errorBarberoFila.set({ id: b.id, mensaje: mensajeDeError(e, 'No pudimos quitar al barbero.') });
      },
    });
  }

  protected quitarHorario(b: Barbero, horarioId: string): void {
    if (this.ocupado()) return;
    this.ocupado.set(b.id);
    this.errorBarberoFila.set(null);
    this.api.quitarHorario(this.slug, b.id, horarioId).subscribe({
      next: (actualizado) => {
        this.reemplazar(actualizado);
        this.ocupado.set(null);
      },
      error: (e) => {
        this.ocupado.set(null);
        this.errorBarberoFila.set({ id: b.id, mensaje: mensajeDeError(e, 'No pudimos quitar el horario.') });
      },
    });
  }

  private reemplazar(actualizado: Barbero): void {
    this.barberos.update((lista) => lista?.map((b) => (b.id === actualizado.id ? actualizado : b)) ?? null);
  }
}
