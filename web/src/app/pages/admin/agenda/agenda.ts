import { NgTemplateOutlet } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ApiService, mensajeDeError } from '../../../core/api.service';
import { HoraLegible, aIso, desdeIso, fechaLarga, horaLegible, pesos, textoHora } from '../../../core/fechas';
import { AccionCita, EstadoCita, ItemAgenda, ItemProximo } from '../../../core/models';
import { SesionService } from '../../../core/sesion.service';
import { enlaceWhatsapp } from '../../../core/whatsapp';

interface BotonAccion {
  accion: AccionCita;
  etiqueta: string;
  principal?: boolean;
}

/** Qué puede hacer el dueño con una cita según su estado (las mismas reglas que valida la API). */
const ACCIONES: Record<EstadoCita, BotonAccion[]> = {
  Pendiente: [{ accion: 'confirmar', etiqueta: 'Confirmar', principal: true }, { accion: 'cancelar', etiqueta: 'Cancelar' }],
  Confirmada: [
    { accion: 'atendida', etiqueta: 'Atendida', principal: true },
    { accion: 'no-asistio', etiqueta: 'No asistió' },
    { accion: 'cancelar', etiqueta: 'Cancelar' },
  ],
  Cancelada: [],
  Atendida: [],
  NoAsistio: [],
};

const ETIQUETA_ESTADO: Record<EstadoCita, string> = {
  Pendiente: 'Por confirmar',
  Confirmada: 'Confirmada',
  Cancelada: 'Cancelada',
  Atendida: 'Atendida',
  NoAsistio: 'No asistió',
};

type Modo = 'dia' | 'proximas';

@Component({
  selector: 'app-agenda',
  imports: [NgTemplateOutlet],
  templateUrl: './agenda.html',
  styleUrl: './agenda.css',
})
export class Agenda implements OnInit {
  private readonly api = inject(ApiService);
  private readonly sesion = inject(SesionService);

  protected readonly modo = signal<Modo>('proximas');
  protected readonly proximas = signal<ItemProximo[] | null>(null);
  protected readonly errorProximas = signal<string | null>(null);

  /** Las próximas citas vigentes, agrupadas por día (el título de cada grupo es la fecha en palabras). */
  protected readonly grupos = computed(() => {
    const hoy = aIso(new Date());
    const porDia = new Map<string, ItemProximo[]>();
    for (const c of this.proximas() ?? []) {
      if (c.estado === 'Cancelada' || c.estado === 'NoAsistio') continue;
      porDia.set(c.fecha, [...(porDia.get(c.fecha) ?? []), c]);
    }
    return [...porDia.entries()]
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([fecha, citas]) => ({
        fecha,
        titulo: `${fecha === hoy ? 'Hoy · ' : ''}${fechaLarga(fecha)}`,
        citas: [...citas].sort((a, b) => a.horaInicio.localeCompare(b.horaInicio)),
      }));
  });

  protected readonly fecha = signal(aIso(new Date()));
  protected readonly citas = signal<ItemAgenda[] | null>(null);
  protected readonly cargando = signal(false);
  protected readonly errorCarga = signal<string | null>(null);

  /** Cita con una acción en curso, la que pide confirmar una cancelación y el último error de una acción. */
  protected readonly ocupada = signal<string | null>(null);
  protected readonly confirmandoCancelar = signal<string | null>(null);
  protected readonly errorAccion = signal<{ id: string; mensaje: string } | null>(null);

  protected readonly esHoy = computed(() => this.fecha() === aIso(new Date()));
  protected readonly titulo = computed(() => fechaLarga(this.fecha()));

  protected readonly ordenadas = computed(() =>
    [...(this.citas() ?? [])].sort((a, b) => a.horaInicio.localeCompare(b.horaInicio)),
  );

  protected readonly resumen = computed(() => {
    const lista = this.citas() ?? [];
    const vigentes = lista.filter((c) => c.estado !== 'Cancelada' && c.estado !== 'NoAsistio');
    return {
      citas: vigentes.length,
      porConfirmar: lista.filter((c) => c.estado === 'Pendiente').length,
      total: pesos(vigentes.reduce((suma, c) => suma + c.precio, 0)),
    };
  });

  /** Para ignorar la respuesta de una consulta vieja si el dueño ya cambió de día. */
  private consulta = 0;

  ngOnInit(): void {
    this.cargarProximas(); // al entrar se ve lo que viene; "Por día" carga la agenda del día al elegirlo
  }

  protected irA(iso: string): void {
    if (!iso || iso === this.fecha()) return;
    this.fecha.set(iso);
    this.confirmandoCancelar.set(null);
    this.errorAccion.set(null);
    this.cargar();
  }

  protected mover(dias: number): void {
    const f = desdeIso(this.fecha());
    f.setDate(f.getDate() + dias);
    this.irA(aIso(f));
  }

  protected escribirFecha(evento: Event): void {
    this.irA((evento.target as HTMLInputElement).value);
  }

  protected elegirModo(modo: Modo): void {
    if (modo === this.modo()) return;
    this.modo.set(modo);
    this.confirmandoCancelar.set(null);
    this.errorAccion.set(null);
    if (modo === 'proximas') this.cargarProximas();
    else this.cargar();
  }

  protected cargarProximas(): void {
    const slug = this.sesion.sesion()?.slug;
    if (!slug) return;
    this.errorProximas.set(null);
    this.api.proximas(slug).subscribe({
      next: (lista) => this.proximas.set(lista),
      error: (e) => this.errorProximas.set(mensajeDeError(e, 'No pudimos cargar las próximas citas.')),
    });
  }

  protected cargar(): void {
    const slug = this.sesion.sesion()?.slug;
    if (!slug) return;
    const esta = ++this.consulta;
    this.cargando.set(true);
    this.errorCarga.set(null);

    this.api.agenda(slug, this.fecha()).subscribe({
      next: (lista) => {
        if (esta !== this.consulta) return;
        this.citas.set(lista);
        this.cargando.set(false);
      },
      error: (e) => {
        if (esta !== this.consulta) return;
        this.cargando.set(false);
        this.errorCarga.set(mensajeDeError(e, 'No pudimos cargar la agenda.'));
      },
    });
  }

  protected acciones(c: ItemAgenda): BotonAccion[] {
    return ACCIONES[c.estado] ?? [];
  }

  protected estado(c: ItemAgenda): string {
    return ETIQUETA_ESTADO[c.estado] ?? c.estado;
  }

  protected hora(c: ItemAgenda): HoraLegible {
    return horaLegible(c.horaInicio);
  }

  protected precio(c: ItemAgenda): string {
    return pesos(c.precio);
  }

  /** Recordatorio listo para enviar por WhatsApp al cliente. */
  protected recordatorio(c: ItemAgenda | ItemProximo): string | null {
    const s = this.sesion.sesion();
    if (!s) return null;
    const mensaje =
      `Hola ${c.clienteNombre}, te recordamos tu turno en ${s.nombreBarberia}: ${c.servicio} con ${c.barbero} ` +
      `el ${fechaLarga('fecha' in c ? c.fecha : this.fecha())} a las ${textoHora(c.horaInicio)}. ¿Nos confirmas que vienes?`;
    return enlaceWhatsapp(c.clienteTelefono, mensaje);
  }

  protected pulsar(c: ItemAgenda, boton: BotonAccion): void {
    // Cancelar es lo único que no se puede deshacer: pide una segunda pulsación.
    if (boton.accion === 'cancelar' && this.confirmandoCancelar() !== c.id) {
      this.confirmandoCancelar.set(c.id);
      return;
    }
    this.ejecutar(c, boton.accion);
  }

  protected cancelarConfirmado(c: ItemAgenda): void {
    this.ejecutar(c, 'cancelar');
  }

  protected hoyIso(): string {
    return aIso(new Date());
  }

  protected noCancelar(): void {
    this.confirmandoCancelar.set(null);
  }

  private ejecutar(c: ItemAgenda, accion: AccionCita): void {
    const slug = this.sesion.sesion()?.slug;
    if (!slug || this.ocupada()) return;
    this.ocupada.set(c.id);
    this.confirmandoCancelar.set(null);
    this.errorAccion.set(null);

    this.api.cambiarEstado(slug, c.id, accion).subscribe({
      next: (r) => {
        // Se actualiza solo esa cita con el estado que respondió la API (no hace falta recargar todo).
        const nuevo = r.estado as EstadoCita;
        this.citas.update((lista) => lista?.map((x) => (x.id === c.id ? { ...x, estado: nuevo } : x)) ?? null);
        this.proximas.update((lista) => lista?.map((x) => (x.id === c.id ? { ...x, estado: nuevo } : x)) ?? null);
        this.ocupada.set(null);
      },
      error: (e) => {
        this.ocupada.set(null);
        this.errorAccion.set({ id: c.id, mensaje: mensajeDeError(e, 'No pudimos actualizar la cita.') });
      },
    });
  }
}
