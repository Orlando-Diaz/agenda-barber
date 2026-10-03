import { Component, input, output } from '@angular/core';
import { DiaOpcion } from '../../../core/fechas';

/** Una fila de días para elegir. Los días en que el barbero no trabaja salen deshabilitados. */
@Component({
  selector: 'app-tira-dias',
  templateUrl: './tira-dias.html',
  styleUrl: './tira-dias.css',
})
export class TiraDias {
  readonly dias = input.required<DiaOpcion[]>();
  /** Días de la semana (0-6) en que el barbero trabaja. */
  readonly habilitados = input.required<ReadonlySet<number>>();
  readonly seleccionado = input<string | null>(null);
  readonly elegido = output<string>();
}
