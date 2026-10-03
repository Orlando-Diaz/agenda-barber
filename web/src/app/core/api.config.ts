import { InjectionToken } from '@angular/core';

/** Dirección base de la API. En producción se reemplaza por la del servidor real. */
export const API_URL = new InjectionToken<string>('API_URL', {
  providedIn: 'root',
  factory: () => 'http://localhost:5068/api',
});
