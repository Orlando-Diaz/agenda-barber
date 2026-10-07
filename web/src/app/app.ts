import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TemaService } from './tema.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `
    <router-outlet />
    <button
      type="button"
      class="tema"
      (click)="tema.alternar()"
      [attr.aria-label]="tema.tema() === 'oscuro' ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro'"
      [attr.title]="tema.tema() === 'oscuro' ? 'Modo claro' : 'Modo oscuro'"
    >
      @if (tema.tema() === 'oscuro') {
        <!-- sol -->
        <svg viewBox="0 0 24 24" width="22" height="22" aria-hidden="true" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
          <circle cx="12" cy="12" r="4.5" /><path d="M12 2v2.5M12 19.5V22M2 12h2.5M19.5 12H22M4.9 4.9l1.8 1.8M17.3 17.3l1.8 1.8M19.1 4.9l-1.8 1.8M6.7 17.3l-1.8 1.8" />
        </svg>
      } @else {
        <!-- luna -->
        <svg viewBox="0 0 24 24" width="22" height="22" aria-hidden="true" fill="currentColor">
          <path d="M20.5 14.5A8.5 8.5 0 0 1 9.5 3.5a.6.6 0 0 0-.8-.7A9.5 9.5 0 1 0 21.2 15.3a.6.6 0 0 0-.7-.8Z" />
        </svg>
      }
    </button>
  `,
  styles: `
    .tema {
      position: fixed;
      right: 1rem;
      bottom: 1rem;
      z-index: 20;
      display: grid;
      place-items: center;
      width: 3rem;
      height: 3rem;
      color: var(--texto-claro);
      background: var(--accion);
      border: 0;
      border-radius: 50%;
      box-shadow: 0 3px 0 var(--cobalto-hondo);
      cursor: pointer;
    }

    .tema:active {
      transform: translateY(2px);
      box-shadow: 0 1px 0 var(--cobalto-hondo);
    }
  `,
})
export class App {
  protected readonly tema = inject(TemaService);
}
