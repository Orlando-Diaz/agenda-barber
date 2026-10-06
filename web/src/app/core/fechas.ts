/** Utilidades de fechas y dinero. Todo en español de Colombia. */

const nombreSemana = new Intl.DateTimeFormat('es-CO', { weekday: 'short' });
const semanaLarga = new Intl.DateTimeFormat('es-CO', { weekday: 'long' });
const mesCorto = new Intl.DateTimeFormat('es-CO', { month: 'short' });
const mesLargo = new Intl.DateTimeFormat('es-CO', { month: 'long' });
const pesosFmt = new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 });

/** 20000 -> "$ 20.000" */
export const pesos = (valor: number): string => pesosFmt.format(valor).replace(/\s/g, ' ');

const dos = (n: number) => String(n).padStart(2, '0');

/** Date -> "2026-10-05" (en hora local del dispositivo; así lo espera la API). */
export const aIso = (f: Date): string => `${f.getFullYear()}-${dos(f.getMonth() + 1)}-${dos(f.getDate())}`;

/** "2026-10-05" -> Date a medianoche local (sin los saltos de zona horaria de new Date("2026-10-05")). */
export function desdeIso(iso: string): Date {
  const [y, m, d] = iso.split('-').map(Number);
  return new Date(y, m - 1, d);
}

export interface DiaOpcion {
  iso: string;
  diaSemana: number; // 0 = domingo ... 6 = sábado
  semana: string; // "lun"
  numero: number; // 5
  mes: string; // "oct"
  esHoy: boolean;
}

/** Los próximos `cantidad` días, empezando por hoy. */
export function proximosDias(cantidad = 14, desde: Date = new Date()): DiaOpcion[] {
  const dias: DiaOpcion[] = [];
  for (let i = 0; i < cantidad; i++) {
    const f = new Date(desde.getFullYear(), desde.getMonth(), desde.getDate() + i);
    dias.push({
      iso: aIso(f),
      diaSemana: f.getDay(),
      semana: nombreSemana.format(f).replace('.', ''),
      numero: f.getDate(),
      mes: mesCorto.format(f).replace('.', ''),
      esHoy: i === 0,
    });
  }
  return dias;
}

/** "2026-10-05" -> "lunes 5 de octubre" */
export function fechaLarga(iso: string): string {
  const f = desdeIso(iso);
  return `${semanaLarga.format(f)} ${f.getDate()} de ${mesLargo.format(f)}`;
}

export const nombreMes = (iso: string): string => mesLargo.format(desdeIso(iso));
export const nombreSemanaLarga = (iso: string): string => semanaLarga.format(desdeIso(iso));

/** Día de la semana (0-6) -> "lun" */
export function semanaCorta(dia: number): string {
  // El 7 de enero de 2024 fue domingo: sirve de referencia.
  return nombreSemana.format(new Date(2024, 0, 7 + dia)).replace('.', '');
}

export interface HoraLegible {
  numero: string; // "2:30"
  sufijo: string; // "p. m."
}

/** "14:30" -> { numero: "2:30", sufijo: "p. m." } */
export function horaLegible(hora: string): HoraLegible {
  const [h, m] = hora.split(':').map(Number);
  return { numero: `${h % 12 || 12}:${dos(m)}`, sufijo: h < 12 ? 'a. m.' : 'p. m.' };
}

export const textoHora = (hora: string): string => {
  const l = horaLegible(hora);
  return `${l.numero} ${l.sufijo}`;
};

/** Día de la semana (0-6) -> "lunes" */
export function nombreDia(dia: number): string {
  return semanaLarga.format(new Date(2024, 0, 7 + dia));
}

/** Orden para mostrar la semana en Colombia: lunes primero, domingo al final. */
export const DIAS_LUNES_A_DOMINGO = [1, 2, 3, 4, 5, 6, 0] as const;
