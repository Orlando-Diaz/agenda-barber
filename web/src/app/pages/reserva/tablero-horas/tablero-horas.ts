import { NgTemplateOutlet } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { HoraDisponible } from '../../../core/models';
import { horaLegible } from '../../../core/fechas';

interface Celda {
  hora: HoraDisponible;
  numero: string;
  sufijo: string;
}

/** Las horas libres, como un piso de azulejos: cada hora es una baldosa. */
@Component({
  selector: 'app-tablero-horas',
  imports: [NgTemplateOutlet],
  templateUrl: './tablero-horas.html',
  styleUrl: './tablero-horas.css',
})
export class TableroHoras {
  readonly horas = input.required<HoraDisponible[]>();
  readonly seleccionada = input<string | null>(null);
  readonly elegida = output<HoraDisponible>();

  private readonly celdas = computed<Celda[]>(() =>
    this.horas().map((hora) => ({ hora, ...horaLegible(hora.hora) })),
  );

  // Se separan en mañana y tarde para que la lista se lea más rápido.
  protected readonly manana = computed(() => this.celdas().filter((c) => c.sufijo === 'a. m.'));
  protected readonly tarde = computed(() => this.celdas().filter((c) => c.sufijo === 'p. m.'));
}
