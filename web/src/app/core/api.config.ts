import { InjectionToken } from '@angular/core';

/** API en producción (Render). Si al crear el servicio te dio otro nombre, cambia solo esta línea. */
const API_PRODUCCION = 'https://agendabarber-api.onrender.com/api';
const API_LOCAL = 'http://localhost:5068/api';

/** Dirección base de la API: la local mientras desarrollas en tu PC y la de Render cuando la web está publicada. */
export const API_URL = new InjectionToken<string>('API_URL', {
  providedIn: 'root',
  factory: () => (['localhost', '127.0.0.1'].includes(window.location.hostname) ? API_LOCAL : API_PRODUCCION),
});
