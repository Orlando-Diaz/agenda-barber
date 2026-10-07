import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-no-encontrada',
  imports: [RouterLink],
  template: `
    <main class="pagina nada">
      <h1 class="rotulo">Esta página no existe</h1>
      <p>Revisa que el enlace esté completo, o busca tu barbería desde el inicio.</p>
      <a class="atras" routerLink="/"><span aria-hidden="true">‹</span> Ir al inicio</a>
    </main>
  `,
  styles: `
    .nada { padding: 4rem 1.25rem; display: grid; gap: 1rem; justify-items: start; }
    h1 { font-size: clamp(2.75rem, 14vw, 4.5rem); color: var(--cobalto); }
  `,
})
export class NoEncontrada {}
