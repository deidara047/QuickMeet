import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { SelectModule } from 'primeng/select';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { ProfileService } from '../../../core/services/profile.service';
import { AuthService } from '../../../core/services/auth.service';
import { ProviderProfile } from '../../../shared/models/availability.model';

@Component({
  selector: 'app-profile-editor',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    TextareaModule,
    ToastModule,
    SelectModule,
    ProgressSpinnerModule
  ],
  templateUrl: './profile-editor.html',
  styleUrl: './profile-editor.css',
  providers: [MessageService]
})
export class ProfileEditorComponent implements OnInit {
  form!: FormGroup;
  profile: ProviderProfile | null = null;
  loading = true;
  saving = false;
  photoPreview: string | null = null;

  durationOptions = [
    { label: '15 minutos', value: 15 },
    { label: '30 minutos', value: 30 },
    { label: '45 minutos', value: 45 },
    { label: '60 minutos', value: 60 }
  ];

  constructor(
    private fb: FormBuilder,
    private profileService: ProfileService,
    private authService: AuthService,
    private messageService: MessageService,
    public router: Router
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadProfile();
  }

  initForm(): void {
    this.form = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(1000)]],
      phoneNumber: ['', [Validators.pattern(/^[+]?[(]?[0-9]{1,4}[)]?[-\s.]?[(]?[0-9]{1,4}[)]?[-\s.]?[0-9]{1,9}$/)]],
      appointmentDurationMinutes: [30, [Validators.required, Validators.min(15), Validators.max(120)]]
    });
  }

  loadProfile(): void {
    const userId = this.authService.getCurrentUserId();
    
    if (!userId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Usuario no autenticado',
        life: 3000
      });
      this.router.navigate(['/login']);
      return;
    }

    this.profileService.getProfile(userId).subscribe({
      next: (profile) => {
        this.profile = profile;
        this.form.patchValue({
          fullName: profile.fullName,
          description: profile.description || '',
          phoneNumber: profile.phoneNumber || '',
          appointmentDurationMinutes: profile.appointmentDurationMinutes
        });
        if (profile.photoUrl) {
          this.photoPreview = profile.photoUrl;
        }
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        const errorMsg = err.error?.error || 'Error al cargar el perfil';
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: errorMsg,
          life: 3000
        });
      }
    });
  }

  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];

      // Validación de tamaño (5MB)
      const maxSizeInBytes = 5 * 1024 * 1024;
      if (file.size > maxSizeInBytes) {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'La foto no puede exceder 5MB',
          life: 3000
        });
        return;
      }

      // Validación de extensión
      const allowedExtensions = ['image/jpeg', 'image/png', 'image/webp'];
      if (!allowedExtensions.includes(file.type)) {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Solo se permiten JPEG, PNG o WebP',
          life: 3000
        });
        return;
      }
      
      const reader = new FileReader();
      reader.onload = (e) => {
        this.photoPreview = e.target?.result as string;
      };
      reader.readAsDataURL(file);

      const userId = this.authService.getCurrentUserId();
      if (userId) {
        this.profileService.uploadPhoto(userId, file).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Éxito',
              detail: 'Foto actualizada correctamente',
              life: 2000
            });
          },
          error: (err) => {
            const errorMsg = err.error?.error || 'Error al subir la foto';
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: errorMsg,
              life: 3000
            });
          }
        });
      }
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validación',
        detail: 'Por favor completa todos los campos correctamente',
        life: 3000
      });
      return;
    }

    this.saving = true;
    const userId = this.authService.getCurrentUserId();
    
    if (!userId) {
      this.saving = false;
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Usuario no autenticado',
        life: 3000
      });
      return;
    }

    this.profileService.updateProfile(userId, this.form.value).subscribe({
      next: () => {
        this.saving = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Perfil actualizado correctamente',
          life: 2000
        });
        setTimeout(() => {
          this.router.navigate(['/dashboard']);
        }, 2000);
      },
      error: (err) => {
        this.saving = false;
        const errorMsg = err.error?.error || 'Error al actualizar el perfil';
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: errorMsg,
          life: 3000
        });
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getErrorMessage(fieldName: string): string {
    const field = this.form.get(fieldName);

    if (!field?.errors) {
      return '';
    }

    if (field.errors['required']) {
      return `${this.getFieldLabel(fieldName)} es requerido`;
    }

    if (field.errors['minlength']) {
      const minLength = field.errors['minlength'].requiredLength;
      return `Mínimo ${minLength} caracteres`;
    }

    if (field.errors['maxlength']) {
      const maxLength = field.errors['maxlength'].requiredLength;
      return `Máximo ${maxLength} caracteres`;
    }

    if (field.errors['pattern']) {
      return 'Formato inválido';
    }

    return 'Campo inválido';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      fullName: 'Nombre completo',
      description: 'Descripción',
      phoneNumber: 'Teléfono',
      appointmentDurationMinutes: 'Duración de cita'
    };
    return labels[fieldName] || fieldName;
  }
}
