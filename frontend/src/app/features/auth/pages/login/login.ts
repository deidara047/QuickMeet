import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { LogoComponent } from '../../../../shared/components/logo/logo';
import { loginSchema, type LoginFormData } from '../../schemas/auth.schema';
import { validateWithZod } from '../../../../shared/utils/form-validation.utils';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    ButtonModule,
    InputTextModule,
    LogoComponent
  ],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent {
  loginForm: FormGroup;
  isLoading = false;
  formErrors: Record<string, string> = {};

  constructor(private fb: FormBuilder) {
    this.loginForm = this.fb.group({
      email: [''],
      password: ['']
    });
  }

  onSubmit() {
    // Validar con Zod
    const validation = validateWithZod(this.loginForm.value, loginSchema);
    
    if (!validation.success) {
      // Mostrar errores del formulario
      if (validation.errors) {
        this.formErrors = {};
        Object.entries(validation.errors).forEach(([field, messages]) => {
          this.formErrors[field] = messages[0]; // Mostrar primer error
        });
      }
      return;
    }

    // Si llegamos aquí, los datos son válidos
    const validatedData: LoginFormData = validation.data!;
    this.isLoading = true;
    
    console.log('Login submitted with validated data:', validatedData);
    // Aquí iría la llamada al servicio de autenticación
    // this.authService.login(validatedData).subscribe(...)
  }

  getFieldError(fieldName: string): string {
    return this.formErrors[fieldName] || '';
  }

  hasFieldError(fieldName: string): boolean {
    return !!this.formErrors[fieldName];
  }
}
