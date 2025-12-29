import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { LoginComponent } from './login';
import { LogoComponent } from '../../../../shared/components/logo/logo';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        LoginComponent,
        ReactiveFormsModule,
        ButtonModule,
        InputTextModule,
        LogoComponent
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize form with empty email and password', () => {
      expect(component.loginForm.get('email')?.value).toBe('');
      expect(component.loginForm.get('password')?.value).toBe('');
    });

    it('should initialize formErrors as empty object', () => {
      expect(component.formErrors).toEqual({});
    });

    it('should initialize isLoading as false', () => {
      expect(component.isLoading).toBeFalsy();
    });
  });

  describe('Form Validation with Zod', () => {
    it('should fail validation with empty email', () => {
      component.loginForm.patchValue({
        email: '',
        password: 'password123'
      });
      component.onSubmit();
      expect(component.formErrors['email']).toBeTruthy();
    });

    it('should fail validation with invalid email format', () => {
      component.loginForm.patchValue({
        email: 'invalid-email',
        password: 'password123'
      });
      component.onSubmit();
      expect(component.formErrors['email']).toBeTruthy();
    });

    it('should fail validation with empty password', () => {
      component.loginForm.patchValue({
        email: 'test@example.com',
        password: ''
      });
      component.onSubmit();
      expect(component.formErrors['password']).toBeTruthy();
    });

    it('should fail validation with password less than 6 characters', () => {
      component.loginForm.patchValue({
        email: 'test@example.com',
        password: '12345'
      });
      component.onSubmit();
      expect(component.formErrors['password']).toBeTruthy();
      expect(component.formErrors['password']).toContain('al menos 6');
    });

    it('should pass validation with valid email and password', () => {
      component.loginForm.patchValue({
        email: 'test@example.com',
        password: 'password123'
      });
      component.onSubmit();
      expect(component.formErrors).toEqual({});
    });

    it('should trim and lowercase email during validation', () => {
      component.loginForm.patchValue({
        email: '  TEST@EXAMPLE.COM  ',
        password: 'password123'
      });
      component.onSubmit();
      // No error means email was accepted
      expect(component.formErrors['email']).toBeFalsy();
    });
  });

  describe('Form Submission', () => {
    it('should set isLoading to true on successful validation', () => {
      component.loginForm.patchValue({
        email: 'test@example.com',
        password: 'password123'
      });
      component.onSubmit();
      expect(component.isLoading).toBeTruthy();
    });

    it('should not set isLoading on failed validation', () => {
      component.loginForm.patchValue({
        email: 'invalid',
        password: '123'
      });
      component.onSubmit();
      expect(component.isLoading).toBeFalsy();
    });

    it('should console.log validated data on successful submission', () => {
      spyOn(console, 'log');
      component.loginForm.patchValue({
        email: 'test@example.com',
        password: 'password123'
      });
      component.onSubmit();
      expect(console.log).toHaveBeenCalledWith(
        'Login submitted with validated data:',
        jasmine.objectContaining({
          email: 'test@example.com',
          password: 'password123'
        })
      );
    });
  });

  describe('Error Handling Methods', () => {
    it('should return error message for field with error', () => {
      component.formErrors['email'] = 'Ingresa un email válido';
      expect(component.getFieldError('email')).toBe('Ingresa un email válido');
    });

    it('should return empty string for field without error', () => {
      expect(component.getFieldError('email')).toBe('');
    });

    it('should return true when field has error', () => {
      component.formErrors['password'] = 'La contraseña debe tener al menos 6 caracteres';
      expect(component.hasFieldError('password')).toBeTruthy();
    });

    it('should return false when field has no error', () => {
      expect(component.hasFieldError('password')).toBeFalsy();
    });
  });
});
