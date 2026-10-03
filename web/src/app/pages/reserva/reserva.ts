import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ApiService, mensajeDeError } from '../../core/api.service';
import { fechaLarga, pesos, proximosDias, semanaCorta, textoHora } from '../../core/fechas';
import { Barberia, Barbero, CitaConfirmada, HoraDisponible, Servicio } from '../../core/models';
import { TableroHoras } from './tablero-horas/tablero-horas';
import { Ticket } from './ticket/ticket';
import { TiraDias } from './tira-dias/tira-dias';

@Component({
  selector: 'app-reserva',
  imports: [TiraDias, TableroHoras, Ticket],
  templateUrl: './reserva.html',
  styleUrl: './reserva.css',
})
export class Reserva implements OnInit {
  private readonly api = inject(ApiService);
  private readonly slug = inject(ActivatedRoute).snapshot.paramMap.get('slug') ?? '';

  // --- Datos que vienen de la API ---
  protected readonly cargando = signal(true);
  protected readonly noEncontrada = signal(false);
  protected readonly errorCarga = signal<string | null>(null);
  protected readonly barberia = signal<Barberia | null>(null);
  protected readonly servicios = signal<Servicio[]>([]);
  protected readonly barberos = signal<Barbero[]>([]);

  // --- Lo que el cliente va eligiendo ---
  protected readonly servicioId = signal<string | null>(null);
  protected readonly barberoId = signal<string | null>(null);
  protected readonly dia = signal<string | null>(null);
  protected readonly horaSel = signal<HoraDisponible | null>(null);
  protected readonly nombre = signal('');
  protected readonly telefono = signal('');

  // --- Estado de las horas y de la reserva ---
  protected readonly horas = signal<HoraDisponible[]>([]);
  protected readonly cargandoHoras = signal(false);
  protected readonly errorHoras = signal<string | null>(null);
  protected readonly enviando = signal(false);
  protected readonly errorReserva = signal<string | null>(null);
  protected readonly cita = signal<CitaConfirmada | null>(null);

  // --- Valores calculados: se actualizan solos cuando cambian los signals de los que dependen ---
  protected readonly dias = proximosDias(14);
  protected readonly servicio = computed(() => this.servicios().find((s) => s.id === this.servicioId()) ?? null);
  protected readonly barbero = computed(() => this.barberos().find((b) => b.id === this.barberoId()) ?? null);
  protected readonly diasHabilitados = computed<ReadonlySet<number>>(
    () => new Set(this.barbero()?.horarios.map((h) => h.dia) ?? []),
  );
  protected readonly datosValidos = computed(() => {
    const digitos = this.telefono().replace(/\D/g, '');
    return this.nombre().trim().length >= 2 && digitos.length >= 7 && digitos.length <= 15;
  });
  protected readonly resumen = computed(() => {
    const s = this.servicio();
    const b = this.barbero();
    const d = this.dia();
    const h = this.horaSel();
    if (!s || !b || !d || !h) return null;
    return { servicio: s.nombre, barbero: b.nombre, cuando: `${fechaLarga(d)}, ${textoHora(h.hora)}`, total: pesos(s.precio) };
  });

  // Se exponen para usarlas en la plantilla
  protected readonly pesos = pesos;

  /** Para descartar respuestas viejas si el cliente cambia de día muy rápido. */
  private pedidoHoras = 0;

  ngOnInit(): void {
    this.cargar();
  }

  protected cargar(): void {
    this.cargando.set(true);
    this.noEncontrada.set(false);
    this.errorCarga.set(null);

    forkJoin({
      barberia: this.api.barberia(this.slug),
      servicios: this.api.servicios(this.slug),
      barberos: this.api.barberos(this.slug),
    }).subscribe({
      next: ({ barberia, servicios, barberos }) => {
        this.barberia.set(barberia);
        this.servicios.set(servicios);
        this.barberos.set(barberos);
        // Si solo hay una opción, no hace falta que el cliente la elija.
        if (servicios.length === 1) this.servicioId.set(servicios[0].id);
        const atienden = barberos.filter((b) => b.horarios.length > 0);
        if (atienden.length === 1) this.barberoId.set(atienden[0].id);
        this.cargando.set(false);
      },
      error: (e) => {
        if (e instanceof HttpErrorResponse && e.status === 404) this.noEncontrada.set(true);
        else this.errorCarga.set(mensajeDeError(e, 'No pudimos cargar la barbería. Intenta de nuevo.'));
        this.cargando.set(false);
      },
    });
  }

  protected diasTexto(barbero: Barbero): string {
    const dias = [...new Set(barbero.horarios.map((h) => h.dia))].sort((a, b) => (a || 7) - (b || 7)); // lunes primero
    return dias.length ? `Atiende ${dias.map(semanaCorta).join(', ')}` : 'Sin horarios cargados';
  }

  // --- Elecciones del cliente ---
  protected elegirServicio(id: string): void {
    this.servicioId.set(id);
    this.recargarHoras();
  }

  protected elegirBarbero(id: string): void {
    this.barberoId.set(id);
    // Si el día elegido no lo trabaja este barbero, se descarta.
    const d = this.dia();
    if (d && !this.diasHabilitados().has(this.dias.find((x) => x.iso === d)?.diaSemana ?? -1)) this.dia.set(null);
    this.recargarHoras();
  }

  protected elegirDia(iso: string): void {
    this.dia.set(iso);
    this.recargarHoras();
  }

  protected elegirHora(hora: HoraDisponible): void {
    this.horaSel.set(hora);
    this.errorReserva.set(null);
  }

  protected escribirNombre(e: Event): void {
    this.nombre.set((e.target as HTMLInputElement).value);
  }

  protected escribirTelefono(e: Event): void {
    this.telefono.set((e.target as HTMLInputElement).value);
  }

  private recargarHoras(): void {
    const pedido = ++this.pedidoHoras;
    this.horaSel.set(null);
    this.horas.set([]);
    this.errorHoras.set(null);

    const servicioId = this.servicioId();
    const barberoId = this.barberoId();
    const dia = this.dia();
    if (!servicioId || !barberoId || !dia) {
      this.cargandoHoras.set(false);
      return;
    }

    this.cargandoHoras.set(true);
    this.api.disponibilidad(this.slug, barberoId, servicioId, dia).subscribe({
      next: (horas) => {
        if (pedido !== this.pedidoHoras) return; // llegó tarde: el cliente ya cambió de día
        this.horas.set(horas);
        this.cargandoHoras.set(false);
      },
      error: (e) => {
        if (pedido !== this.pedidoHoras) return;
        this.errorHoras.set(mensajeDeError(e, 'No pudimos buscar las horas. Intenta de nuevo.'));
        this.cargandoHoras.set(false);
      },
    });
  }

  // --- Reservar ---
  protected reservar(evento: Event): void {
    evento.preventDefault();
    const hora = this.horaSel();
    const servicio = this.servicio();
    const barbero = this.barbero();
    const dia = this.dia();
    if (!hora || !servicio || !barbero || !dia || !this.datosValidos() || this.enviando()) return;

    this.enviando.set(true);
    this.errorReserva.set(null);
    const cliente = this.nombre().trim();

    this.api
      .reservar(this.slug, {
        barberoId: barbero.id,
        servicioId: servicio.id,
        inicioUtc: hora.inicioUtc,
        clienteNombre: cliente,
        clienteTelefono: this.telefono().trim(),
      })
      .subscribe({
        next: (creada) => {
          this.cita.set({ servicio: servicio.nombre, barbero: barbero.nombre, dia, hora: hora.hora, precio: creada.precio, cliente });
          this.enviando.set(false);
        },
        error: (e) => {
          this.enviando.set(false);
          if (e instanceof HttpErrorResponse && e.status === 409) {
            // Otro cliente se llevó esa hora un instante antes: se muestran las horas actualizadas.
            this.errorReserva.set('Esa hora acaba de ser tomada. Elige otra.');
            this.recargarHoras();
          } else {
            this.errorReserva.set(mensajeDeError(e, 'No pudimos reservar tu turno. Intenta de nuevo.'));
          }
        },
      });
  }

  protected nuevaReserva(): void {
    this.cita.set(null);
    this.servicioId.set(this.servicios().length === 1 ? this.servicios()[0].id : null);
    this.dia.set(null);
    this.nombre.set('');
    this.telefono.set('');
    this.errorReserva.set(null);
    this.recargarHoras();
    window.scrollTo({ top: 0 });
  }
}
