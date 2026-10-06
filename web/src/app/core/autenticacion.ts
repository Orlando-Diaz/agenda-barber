import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { API_URL } from './api.config';
import { SesionService } from './sesion.service';

/**
 * Interceptor: se ejecuta en CADA petición HTTP. Si hay sesión, agrega "Authorization: Bearer <token>"
 * (solo hacia nuestra API: nunca se manda el token a otro sitio). Si la API responde 401, el token
 * ya no sirve: se cierra la sesión y se lleva al login.
 */
export const autenticacion: HttpInterceptorFn = (req, next) => {
  const sesion = inject(SesionService);
  const router = inject(Router);
  const api = inject(API_URL);

  const esNuestra = req.url.startsWith(api);
  const esAuth = req.url.startsWith(`${api}/auth/`); // login y registro no llevan token
  const token = sesion.sesion()?.token;

  const peticion = esNuestra && !esAuth && token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(peticion).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401 && esNuestra && !esAuth) {
        sesion.cerrar();
        void router.navigateByUrl('/admin/entrar');
      }
      return throwError(() => error);
    }),
  );
};

/** Guard: protege las rutas de /admin. Sin sesión, redirige al login. */
export const requiereSesion: CanActivateFn = () => {
  const sesion = inject(SesionService);
  return sesion.activa() ? true : inject(Router).createUrlTree(['/admin/entrar']);
};

/** Guard inverso: quien ya inició sesión no necesita ver el login. */
export const soloSinSesion: CanActivateFn = () => {
  const sesion = inject(SesionService);
  return sesion.activa() ? inject(Router).createUrlTree(['/admin']) : true;
};
