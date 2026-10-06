import { describe, expect, it } from 'vitest';
import { enlaceWhatsapp } from './whatsapp';

describe('enlaceWhatsapp', () => {
  it('agrega el 57 a un celular colombiano de 10 dígitos', () => {
    expect(enlaceWhatsapp('300 123 4567', 'Hola')).toBe('https://wa.me/573001234567?text=Hola');
  });

  it('respeta un número que ya trae indicativo', () => {
    expect(enlaceWhatsapp('+57 300 123 4567', 'Hola')).toBe('https://wa.me/573001234567?text=Hola');
  });

  it('codifica el mensaje', () => {
    expect(enlaceWhatsapp('3001234567', 'Hola Juan, ¿vienes?')).toContain('text=Hola%20Juan%2C%20%C2%BFvienes%3F');
  });

  it('devuelve null si no hay un teléfono utilizable', () => {
    expect(enlaceWhatsapp(null, 'Hola')).toBeNull();
    expect(enlaceWhatsapp('123', 'Hola')).toBeNull();
  });
});
