import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { TableModule } from 'primeng/table';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { AvailabilityService } from '../../../core/services/availability.service';
import { AuthService } from '../../../core/services/auth.service';
import { AvailabilityConfig, DayConfig, BreakConfig, TimeSlot } from '../../../shared/models/availability.model';

interface DayRow {
  day: string;
  dayIndex: number;
  enabled: boolean;
  startTime: string;
  endTime: string;
  breaks: BreakRow[];
}

interface BreakRow {
  startTime: string;
  endTime: string;
}

@Component({
  selector: 'app-availability-configurator',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    CheckboxModule,
    ToastModule,
    TableModule,
    ProgressSpinnerModule,
    TagModule
  ],
  templateUrl: './availability-configurator.html',
  styleUrl: './availability-configurator.css',
  providers: [MessageService]
})
export class AvailabilityConfiguratorComponent implements OnInit {
  form!: FormGroup;
  loading = true;
  saving = false;
  generatingPreview = false;
  previewSlots: TimeSlot[] = [];

  durationOptions = [
    { label: '15 minutos', value: 15 },
    { label: '30 minutos', value: 30 },
    { label: '45 minutos', value: 45 },
    { label: '60 minutos', value: 60 },
    { label: '90 minutos', value: 90 },
    { label: '120 minutos', value: 120 }
  ];

  bufferOptions = [
    { label: 'Sin buffer', value: 0 },
    { label: '5 minutos', value: 5 },
    { label: '10 minutos', value: 10 },
    { label: '15 minutos', value: 15 },
    { label: '30 minutos', value: 30 }
  ];

  daysOfWeek = [
    { label: 'Lunes', value: 1 },
    { label: 'Martes', value: 2 },
    { label: 'Miércoles', value: 3 },
    { label: 'Jueves', value: 4 },
    { label: 'Viernes', value: 5 },
    { label: 'Sábado', value: 6 },
    { label: 'Domingo', value: 0 }
  ];

  constructor(
    private fb: FormBuilder,
    private availabilityService: AvailabilityService,
    private authService: AuthService,
    private messageService: MessageService,
    public router: Router
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadAvailability();
  }

  initForm(): void {
    this.form = this.fb.group({
      appointmentDurationMinutes: [30, [Validators.required, Validators.min(15), Validators.max(120)]],
      bufferMinutes: [0, [Validators.required, Validators.min(0), Validators.max(60)]],
      daysConfiguration: this.fb.array(this.createDaysArray())
    });
  }

  private createDaysArray(): FormGroup[] {
    return this.daysOfWeek.map(day =>
      this.fb.group({
        dayOfWeek: [day.value, Validators.required],
        enabled: [false],
        startTime: ['09:00'],
        endTime: ['18:00'],
        breaks: this.fb.array([])
      })
    );
  }

  get daysArray(): FormArray {
    return this.form.get('daysConfiguration') as FormArray;
  }

  getBreaksArray(dayIndex: number): FormArray {
    return this.daysArray.at(dayIndex).get('breaks') as FormArray;
  }

  loadAvailability(): void {
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

    this.availabilityService.getAvailability(userId).subscribe({
      next: (availability) => {
        this.populateForm(availability);
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        // Si no existe configuración, continuamos con valores por defecto
        if (err.status === 404) {
          this.messageService.add({
            severity: 'info',
            summary: 'Información',
            detail: 'Configura tu disponibilidad por primera vez',
            life: 2000
          });
        } else {
          const errorMsg = err.error?.error || 'Error al cargar la disponibilidad';
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: errorMsg,
            life: 3000
          });
        }
      }
    });
  }

  private populateForm(availability: AvailabilityConfig): void {
    this.form.patchValue({
      appointmentDurationMinutes: availability.appointmentDurationMinutes,
      bufferMinutes: availability.bufferMinutes
    });

    const daysArray = this.form.get('daysConfiguration') as FormArray;
    daysArray.clear();

    this.daysOfWeek.forEach((day) => {
      const dayConfig = availability.daysConfiguration.find(d => d.dayOfWeek === day.value);
      const breakConfigs = dayConfig?.breaks || [];

      const dayForm = this.fb.group({
        dayOfWeek: [day.value, Validators.required],
        enabled: [!!dayConfig && dayConfig.enabled],
        startTime: [dayConfig?.startTime || '09:00'],
        endTime: [dayConfig?.endTime || '18:00'],
        breaks: this.fb.array(breakConfigs.map(b => this.createBreakForm(b)))
      });

      daysArray.push(dayForm);
    });
  }

  private createBreakForm(breakConfig?: BreakConfig): FormGroup {
    return this.fb.group({
      startTime: [breakConfig?.startTime || '12:00', Validators.required],
      endTime: [breakConfig?.endTime || '13:00', Validators.required]
    });
  }

  toggleDay(dayIndex: number): void {
    const day = this.daysArray.at(dayIndex);
    const enabled = day.get('enabled')?.value;
    day.patchValue({ enabled: !enabled });
  }

  addBreak(dayIndex: number): void {
    const breaksArray = this.getBreaksArray(dayIndex);
    breaksArray.push(this.createBreakForm());
  }

  removeBreak(dayIndex: number, breakIndex: number): void {
    const breaksArray = this.getBreaksArray(dayIndex);
    breaksArray.removeAt(breakIndex);
  }

  generatePreview(): void {
    if (this.form.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validación',
        detail: 'Por favor completa todos los campos correctamente',
        life: 3000
      });
      return;
    }

    const enabledDays = this.daysArray.value.filter((day: any) => day.enabled);
    if (enabledDays.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validación',
        detail: 'Selecciona al menos un día de trabajo',
        life: 3000
      });
      return;
    }

    this.generatingPreview = true;
    const userId = this.authService.getCurrentUserId();
    
    if (!userId) {
      this.generatingPreview = false;
      return;
    }

    // Obtener slots para los próximos 3 días
    const today = new Date();
    const previewDates = [
      new Date(today),
      new Date(today.getTime() + 86400000),
      new Date(today.getTime() + 172800000)
    ];

    let allSlots: TimeSlot[] = [];
    let completed = 0;

    previewDates.forEach(date => {
      this.availabilityService.getAvailableSlots(userId, date.toISOString().split('T')[0]).subscribe({
        next: (slots) => {
          allSlots = allSlots.concat(slots);
          completed++;
          if (completed === previewDates.length) {
            this.previewSlots = allSlots;
            this.generatingPreview = false;
            this.messageService.add({
              severity: 'info',
              summary: 'Preview',
              detail: `${allSlots.length} slots disponibles para los próximos 3 días`,
              life: 2000
            });
          }
        },
        error: () => {
          completed++;
          if (completed === previewDates.length) {
            this.generatingPreview = false;
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'No se pudo generar el preview',
              life: 3000
            });
          }
        }
      });
    });
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

    const enabledDays = this.daysArray.value.filter((day: any) => day.enabled);
    if (enabledDays.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validación',
        detail: 'Selecciona al menos un día de trabajo',
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

    const config: AvailabilityConfig = {
      appointmentDurationMinutes: this.form.get('appointmentDurationMinutes')?.value,
      bufferMinutes: this.form.get('bufferMinutes')?.value,
      daysConfiguration: enabledDays.map((day: any) => ({
        dayOfWeek: day.dayOfWeek,
        enabled: true,
        startTime: day.startTime,
        endTime: day.endTime,
        breaks: day.breaks || []
      }))
    };

    this.availabilityService.configureAvailability(config).subscribe({
      next: () => {
        this.saving = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Disponibilidad configurada correctamente',
          life: 2000
        });
        setTimeout(() => {
          this.router.navigate(['/dashboard']);
        }, 2000);
      },
      error: (err) => {
        this.saving = false;
        const errorMsg = err.error?.error || 'Error al configurar la disponibilidad';
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: errorMsg,
          life: 3000
        });
      }
    });
  }

  isTimeRangeInvalid(dayIndex: number): boolean {
    const day = this.daysArray.at(dayIndex);
    const start = day.get('startTime')?.value;
    const end = day.get('endTime')?.value;
    return start && end && start >= end;
  }

  isBreakTimeInvalid(dayIndex: number, breakIndex: number): boolean {
    const breakForm = this.getBreaksArray(dayIndex).at(breakIndex);
    const start = breakForm.get('startTime')?.value;
    const end = breakForm.get('endTime')?.value;
    return start && end && start >= end;
  }

  getSlotDisplay(slot: TimeSlot): string {
    const start = new Date(slot.startTime);
    const end = new Date(slot.endTime);
    return `${start.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' })} - ${end.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' })}`;
  }
}
