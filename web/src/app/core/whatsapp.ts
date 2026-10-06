/** Enlace de WhatsApp (wa.me) con un mensaje ya escrito. Null si el teléfono no sirve. */
export function enlaceWhatsapp(telefono: string | null | undefined, mensaje: string): string | null {
  const digitos = (telefono ?? '').replace(/\D/g, '');
  if (digitos.length < 7) return null;
  const numero = digitos.length === 10 ? `57${digitos}` : digitos; // 10 dígitos = celular colombiano: se agrega el 57
  return `https://wa.me/${numero}?text=${encodeURIComponent(mensaje)}`;
}
