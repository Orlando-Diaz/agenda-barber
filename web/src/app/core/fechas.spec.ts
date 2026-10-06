import { aIso, desdeIso, fechaLarga, horaLegible, nombreDia, proximosDias, semanaCorta } from './fechas';

describe('fechas', () => {
  it('lista los próximos días empezando por hoy', () => {
    const dias = proximosDias(3, new Date(2026, 9, 2));
    expect(dias.map((d) => d.iso)).toEqual(['2026-10-02', '2026-10-03', '2026-10-04']);
    expect(dias[0].esHoy).toBe(true);
    expect(dias[1].esHoy).toBe(false);
  });

  it('pasa de un mes al siguiente', () => {
    const dias = proximosDias(2, new Date(2026, 9, 31));
    expect(dias.map((d) => d.iso)).toEqual(['2026-10-31', '2026-11-01']);
  });

  it('convierte ida y vuelta entre Date y texto ISO', () => {
    expect(aIso(desdeIso('2026-10-05'))).toBe('2026-10-05');
  });

  it('escribe la fecha larga en español', () => {
    const texto = fechaLarga('2026-10-05');
    expect(texto).toContain('lunes');
    expect(texto).toContain('5 de octubre');
  });

  it('el día 1 de la semana es lunes', () => {
    expect(semanaCorta(1)).toBe('lun');
    expect(semanaCorta(0)).toBe('dom');
  });

  it('muestra la hora en formato de 12 horas', () => {
    expect(horaLegible('14:30')).toEqual({ numero: '2:30', sufijo: 'p. m.' });
    expect(horaLegible('09:00')).toEqual({ numero: '9:00', sufijo: 'a. m.' });
    expect(horaLegible('00:15')).toEqual({ numero: '12:15', sufijo: 'a. m.' });
    expect(horaLegible('12:00')).toEqual({ numero: '12:00', sufijo: 'p. m.' });
  });

  it('da el nombre del día de la semana (0 = domingo)', () => {
    expect(nombreDia(0)).toBe('domingo');
    expect(nombreDia(1)).toBe('lunes');
  });
});
