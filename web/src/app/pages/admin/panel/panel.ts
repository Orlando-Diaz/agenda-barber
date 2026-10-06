import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SesionService } from '../../../core/sesion.service';

/** El marco del panel: barra superior con el nombre de la barbería y las páginas del dueño dentro. */
@Component({
  selector: 'app-panel',
  imports: [RouterOutlet],
  templateUrl: './panel.html',
  styleUrl: './panel.css',
})
export class Panel {
  protected readonly sesionService = inject(SesionService);
  protected readonly sesion = this.sesionService.sesion;
}
