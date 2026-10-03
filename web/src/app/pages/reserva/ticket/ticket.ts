import { Component, ElementRef, afterNextRender, computed, input, output, viewChild } from '@angular/core';
import { Barberia, CitaConfirmada } from '../../../core/models';
import { desdeIso, fechaLarga, horaLegible, nombreMes, nombreSemanaLarga, pesos, textoHora } from '../../../core/fechas';

/** La confirmación: un tiquete de turno. */
@Component({
  selector: 'app-ticket',
  templateUrl: './ticket.html',
  styleUrl: './ticket.css',
})
export class Ticket {
  readonly cita = input.required<CitaConfirmada>();
  readonly barberia = input.required<Barberia>();
  readonly otra = output<void>();

  private readonly titulo = viewChild.required<ElementRef<HTMLElement>>('titulo');

  protected readonly numeroDia = computed(() => desdeIso(this.cita().dia).getDate());
  protected readonly mes = computed(() => nombreMes(this.cita().dia));
  protected readonly semana = computed(() => nombreSemanaLarga(this.cita().dia));
  protected readonly hora = computed(() => horaLegible(this.cita().hora));
  protected readonly total = computed(() => pesos(this.cita().precio));

  /** Enlace para escribirle a la barbería por WhatsApp con el turno ya descrito. */
  protected readonly whatsapp = computed(() => {
    const telefono = this.barberia().telefono?.replace(/\D/g, '');
    if (!telefono) return null;
    const c = this.cita();
    const numero = telefono.length === 10 ? `57${telefono}` : telefono; // 57 = Colombia
    const mensaje = `Hola, soy ${c.cliente}. Reservé turno para ${c.servicio} con ${c.barbero} el ${fechaLarga(c.dia)} a las ${textoHora(c.hora)}.`;
    return `https://wa.me/${numero}?text=${encodeURIComponent(mensaje)}`;
  });

  constructor() {
    // Al aparecer el tiquete, el foco y la vista van a él (así lo anuncian los lectores de pantalla).
    afterNextRender(() => {
      this.titulo().nativeElement.focus();
      window.scrollTo({ top: 0 });
    });
  }
}
