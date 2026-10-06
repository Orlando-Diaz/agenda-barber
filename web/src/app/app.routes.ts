import { Routes } from '@angular/router';
import { requiereSesion, soloSinSesion } from './core/autenticacion';

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
    path: 'admin/entrar',
    title: 'Entrar · AgendaBarber',
    canActivate: [soloSinSesion],
    loadComponent: () => import('./pages/admin/entrar/entrar').then((m) => m.Entrar),
  },
  {
    // Todo lo que cuelga de /admin exige sesión. El Panel es el marco; las páginas van dentro.
    path: 'admin',
    canActivate: [requiereSesion],
    loadComponent: () => import('./pages/admin/panel/panel').then((m) => m.Panel),
    children: [
      {
        path: '',
        title: 'Agenda · AgendaBarber',
        loadComponent: () => import('./pages/admin/agenda/agenda').then((m) => m.Agenda),
      },
    ],
  },
  {
    path: '**',
    title: 'No encontrada',
    loadComponent: () => import('./pages/no-encontrada/no-encontrada').then((m) => m.NoEncontrada),
  },
];
