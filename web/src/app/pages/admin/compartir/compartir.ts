import { RouterLink } from '@angular/router';
import { Component, OnInit, inject, signal } from '@angular/core';
import QRCode from 'qrcode';
import { SesionService } from '../../../core/sesion.service';

@Component({
  imports: [RouterLink],
  selector: 'app-compartir',
  templateUrl: './compartir.html',
  styleUrl: './compartir.css',
})
export class Compartir implements OnInit {
  private readonly sesion = inject(SesionService).sesion();

  /** El enlace que los clientes usan para reservar. */
  protected readonly enlace = `${window.location.origin}/b/${this.sesion?.slug ?? ''}`;
  protected readonly nombre = this.sesion?.nombreBarberia ?? '';
  protected readonly archivoQr = `qr-${this.sesion?.slug ?? 'barberia'}.png`;

  protected readonly qr = signal<string | null>(null);
  protected readonly copiado = signal(false);
  protected readonly errorCopiar = signal(false);

  /** Mensaje listo para mandar por WhatsApp a un cliente. */
  protected readonly whatsapp = `https://wa.me/?text=${encodeURIComponent(
    `Reserva tu turno en ${this.nombre} sin llamar: ${this.enlace}`,
  )}`;

  ngOnInit(): void {
    QRCode.toDataURL(this.enlace, { width: 640, margin: 2, color: { dark: '#14307f', light: '#ffffff' } })
      .then((url) => this.qr.set(url))
      .catch(() => this.qr.set(null));
  }

  protected async copiar(): Promise<void> {
    this.errorCopiar.set(false);
    try {
      await navigator.clipboard.writeText(this.enlace);
      this.copiado.set(true);
      setTimeout(() => this.copiado.set(false), 2500);
    } catch {
      this.errorCopiar.set(true); // sin permiso de portapapeles: el enlace está visible para copiarlo a mano
    }
  }
}
