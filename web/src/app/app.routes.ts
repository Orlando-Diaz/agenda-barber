import { Routes } from '@angular/router';

// loadComponent carga cada página solo cuando se visita (el cliente descarga menos).
export const routes: Routes = [
  {
    path: '',
    title: 'AgendaBarber',
    loadComponent: () => import('./pages/inicio/inicio').then((m) => m.Inicio),
  },
  {
    path: 'b/:slug',
    title: 'Reservar turno',
    loadComponent: () => import('./pages/reserva/reserva').then((m) => m.Reserva),
  },
  {
    path: '**',
    title: 'No encontrada',
    loadComponent: () => import('./pages/no-encontrada/no-encontrada').then((m) => m.NoEncontrada),
  },
];
