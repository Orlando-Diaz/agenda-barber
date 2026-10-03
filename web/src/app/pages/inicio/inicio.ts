import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-inicio',
  templateUrl: './inicio.html',
  styleUrl: './inicio.css',
})
export class Inicio {
  private readonly router = inject(Router);
  protected readonly enlace = signal('');

  protected escribir(evento: Event): void {
    this.enlace.set((evento.target as HTMLInputElement).value);
  }

  protected ir(evento: Event): void {
    evento.preventDefault();
    const slug = this.enlace().trim().toLowerCase();
    if (slug) this.router.navigate(['/b', slug]);
  }
}
