import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LogoComponent } from '../../../shared/components/logo/logo.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LogoComponent],
  template: `
    <div class="login-container">
      <div class="login-card">
        <div class="logo-section">
          <app-logo size="lg" [showText]="true"></app-logo>
        </div>

        <h1>Inicia Sesión</h1>
        <p class="subtitle">Accede a tu cuenta para agendar citas</p>

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label for="email">Email</label>
            <input
              id="email"
              type="email"
              formControlName="email"
              placeholder="correo@ejemplo.com"
              class="form-control"
            />
          </div>

          <div class="form-group">
            <label for="password">Contraseña</label>
            <input
              id="password"
              type="password"
              formControlName="password"
              placeholder="Tu contraseña"
              class="form-control"
            />
          </div>

          <button type="submit" class="btn-primary" [disabled]="!loginForm.valid">
            Inicia Sesión
          </button>
        </form>

        <p class="signup-link">
          ¿No tienes cuenta? <a href="/register">Regístrate aquí</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: calc(100vh - 70px);
      background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
      padding: 2rem;
    }

    .login-card {
      background: white;
      border-radius: 0.5rem;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
      padding: 3rem;
      width: 100%;
      max-width: 400px;
    }

    .logo-section {
      display: flex;
      justify-content: center;
      margin-bottom: 2rem;
    }

    h1 {
      font-size: 1.875rem;
      font-weight: 600;
      color: #1f2937;
      margin: 0 0 0.5rem;
      text-align: center;
    }

    .subtitle {
      color: #6b7280;
      text-align: center;
      margin-bottom: 2rem;
      font-size: 0.875rem;
    }

    .form-group {
      margin-bottom: 1.5rem;
    }

    label {
      display: block;
      font-weight: 500;
      color: #374151;
      margin-bottom: 0.5rem;
      font-size: 0.875rem;
    }

    .form-control {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #d1d5db;
      border-radius: 0.375rem;
      font-size: 0.875rem;
      transition: border-color 0.3s ease, box-shadow 0.3s ease;
      box-sizing: border-box;
    }

    .form-control:focus {
      outline: none;
      border-color: #4B8BA8;
      box-shadow: 0 0 0 3px rgba(75, 139, 168, 0.1);
    }

    .btn-primary {
      width: 100%;
      padding: 0.75rem;
      background-color: #4B8BA8;
      color: white;
      border: none;
      border-radius: 0.375rem;
      font-weight: 600;
      font-size: 0.875rem;
      cursor: pointer;
      transition: background-color 0.3s ease;
      margin-top: 1rem;
    }

    .btn-primary:hover:not(:disabled) {
      background-color: #3a6a80;
    }

    .btn-primary:disabled {
      background-color: #9ca3af;
      cursor: not-allowed;
    }

    .signup-link {
      text-align: center;
      margin-top: 1.5rem;
      color: #6b7280;
      font-size: 0.875rem;
    }

    .signup-link a {
      color: #4B8BA8;
      text-decoration: none;
      font-weight: 600;
    }

    .signup-link a:hover {
      text-decoration: underline;
    }

    @media (max-width: 768px) {
      .login-card {
        padding: 2rem;
      }

      h1 {
        font-size: 1.5rem;
      }
    }
  `]
})
export class LoginComponent {
  loginForm: FormGroup;

  constructor(private fb: FormBuilder) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      console.log('Form submitted:', this.loginForm.value);
      // TODO: Implement actual login logic
    }
  }
}
