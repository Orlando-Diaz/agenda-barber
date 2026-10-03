# AgendaBarber · web

Frontend en Angular de AgendaBarber. Hoy incluye la página pública de reserva (`/b/:slug`).

## Requisitos
- Node.js 24 LTS (o 22.22.3 o superior) y npm 10 o superior.
- La API corriendo en `http://localhost:5068` (ver el README de la raíz).

## Comandos
```
npm install      # una sola vez
npm start        # abre http://localhost:4200
npm test         # pruebas unitarias (Vitest)
npm run build    # compila para producción en dist/
```

Para apuntar a otra API, cambia `factory` en `src/app/core/api.config.ts`.
