import { describe, expect, it } from 'vitest';
import { aSlug } from './entrar';

describe('aSlug', () => {
  it('quita tildes, pasa a minúsculas y une con guiones', () => {
    expect(aSlug('Barbería El Patrón')).toBe('barberia-el-patron');
  });

  it('descarta símbolos y guiones sobrantes', () => {
    expect(aSlug('  ¡Corte & Barba! ')).toBe('corte-barba');
  });
});
