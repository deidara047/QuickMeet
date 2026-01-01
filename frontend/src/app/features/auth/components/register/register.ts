import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../../../core/services/auth.service';
import { PasswordMatchValidator } from '../../../../shared/validators/password-match.validator';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    CardModule,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    ToastModule
  ],
  templateUrl: './register.html',
  styleUrl: './register.css',
  providers: [MessageService]
})
export class RegisterComponent implements OnInit {
  form!: FormGroup;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50), Validators.pattern(/^[a-zA-Z0-9-_]+$/)]],
      fullName: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(256)]],
      password: ['', [Validators.required, Validators.minLength(8), this.passwordStrengthValidator()]],
      passwordConfirmation: ['', Validators.required]
    }, {
      validators: PasswordMatchValidator
    });
  }

  private passwordStrengthValidator(): (control: AbstractControl) => ValidationErrors | null {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) return null;

      const hasUpperCase = /[A-Z]/.test(value);
      const hasNumber = /[0-9]/.test(value);
      const hasSpecialChar = /[^a-zA-Z0-9]/.test(value);

      if (!hasUpperCase || !hasNumber || !hasSpecialChar) {
        return { passwordStrength: true };
      }

      return null;
    };
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

    this.loading = true;
    const formData = this.form.value;

    this.authService.register(formData).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        console.log(err);
        const errorMsg = err.error?.error || 'Error al crear la cuenta';
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
    const formErrors = this.form.errors;

    if (!field?.errors && !formErrors) {
      return '';
    }

    if (field?.errors) {
      if (field.errors['required']) {
        return `${fieldName} es requerido`;
      }

      if (field.errors['email']) {
        return 'Email inválido';
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
        if (fieldName === 'username') {
          return 'Solo letras, números, guiones y guiones bajos';
        }
        return 'Formato inválido';
      }

      if (field.errors['passwordStrength']) {
        return 'Debe contener mayúscula, número y carácter especial';
      }
    }

    if (fieldName === 'passwordConfirmation' && formErrors?.['passwordMismatch']) {
      return 'Las contraseñas no coinciden';
    }

    return 'Campo inválido';
  }
}
