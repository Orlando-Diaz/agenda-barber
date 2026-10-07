import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService, mensajeDeError } from '../../../core/api.service';
import { SesionService } from '../../../core/sesion.service';

const ANCHO_MAXIMO = 1000; // px: de sobra para una tarjeta en el celular
const LIMITE_BYTES = 380_000; // la API acepta hasta 400 000

/** Achica la imagen en el navegador (JPEG) hasta que pese menos que el límite de la API. */
async function reducir(archivo: File): Promise<Blob> {
  const bitmap = await createImageBitmap(archivo);
  const escala = Math.min(1, ANCHO_MAXIMO / bitmap.width);
  const lienzo = document.createElement('canvas');
  lienzo.width = Math.round(bitmap.width * escala);
  lienzo.height = Math.round(bitmap.height * escala);
  lienzo.getContext('2d')!.drawImage(bitmap, 0, 0, lienzo.width, lienzo.height);
  bitmap.close();

  for (const calidad of [0.85, 0.75, 0.65, 0.5, 0.4]) {
    const blob = await new Promise<Blob | null>((ok) => lienzo.toBlob(ok, 'image/jpeg', calidad));
    if (blob && blob.size <= LIMITE_BYTES) return blob;
  }
  throw new Error('demasiado grande');
}

@Component({
  imports: [FormsModule, RouterLink],
  selector: 'app-perfil',
  templateUrl: './perfil.html',
  styleUrl: './perfil.css',
})
export class Perfil implements OnInit {
  private readonly api = inject(ApiService);
  private readonly slug = inject(SesionService).sesion()?.slug ?? '';

  protected readonly cargando = signal(true);
  protected readonly direccion = signal('');
  protected readonly descripcion = signal('');
  protected readonly foto = signal<string | null>(null);

  protected readonly guardando = signal(false);
  protected readonly subiendo = signal(false);
  protected readonly guardado = signal(false);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.api.barberia(this.slug).subscribe({
      next: (b) => {
        this.direccion.set(b.direccion ?? '');
        this.descripcion.set(b.descripcion ?? '');
        this.foto.set(b.fotoVersion === null ? null : this.api.urlFoto(this.slug, b.fotoVersion));
        this.cargando.set(false);
      },
      error: (e) => {
        this.error.set(mensajeDeError(e, 'No pudimos cargar tu perfil.'));
        this.cargando.set(false);
      },
    });
  }

  protected guardar(): void {
    this.error.set(null);
    this.guardado.set(false);
    this.guardando.set(true);
    this.api.actualizarPerfil(this.slug, this.direccion().trim() || null, this.descripcion().trim() || null).subscribe({
      next: () => {
        this.guardando.set(false);
        this.guardado.set(true);
      },
      error: (e) => {
        this.guardando.set(false);
        this.error.set(mensajeDeError(e, 'No pudimos guardar los cambios.'));
      },
    });
  }

  protected async elegirFoto(evento: Event): Promise<void> {
    const entrada = evento.target as HTMLInputElement;
    const archivo = entrada.files?.[0];
    entrada.value = ''; // permite volver a elegir el mismo archivo
    if (!archivo) return;

    this.error.set(null);
    this.subiendo.set(true);
    try {
      const imagen = await reducir(archivo);
      this.api.subirFoto(this.slug, imagen).subscribe({
        next: (b) => {
          this.foto.set(b.fotoVersion === null ? null : this.api.urlFoto(this.slug, b.fotoVersion));
          this.subiendo.set(false);
        },
        error: (e) => {
          this.subiendo.set(false);
          this.error.set(mensajeDeError(e, 'No pudimos subir la foto.'));
        },
      });
    } catch {
      this.subiendo.set(false);
      this.error.set('No pudimos leer esa imagen. Prueba con otra foto (JPG o PNG).');
    }
  }

  protected quitarFoto(): void {
    this.error.set(null);
    this.subiendo.set(true);
    this.api.quitarFoto(this.slug).subscribe({
      next: () => {
        this.foto.set(null);
        this.subiendo.set(false);
      },
      error: (e) => {
        this.subiendo.set(false);
        this.error.set(mensajeDeError(e, 'No pudimos quitar la foto.'));
      },
    });
  }
}
