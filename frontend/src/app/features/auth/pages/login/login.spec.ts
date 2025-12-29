import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { LoginComponent } from './login';
import { LogoComponent } from '../../../../shared/components/logo/logo.component';

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
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with empty email and password', () => {
    expect(component.loginForm.get('email')?.value).toBe('');
    expect(component.loginForm.get('password')?.value).toBe('');
  });

  it('should have invalid form when fields are empty', () => {
    expect(component.loginForm.invalid).toBeTruthy();
  });

  it('should have invalid form with invalid email', () => {
    component.loginForm.patchValue({
      email: 'invalid-email',
      password: 'password123'
    });
    expect(component.loginForm.invalid).toBeTruthy();
  });

  it('should have invalid form with short password', () => {
    component.loginForm.patchValue({
      email: 'test@example.com',
      password: '123'
    });
    expect(component.loginForm.invalid).toBeTruthy();
  });

  it('should have valid form with correct email and password', () => {
    component.loginForm.patchValue({
      email: 'test@example.com',
      password: 'password123'
    });
    expect(component.loginForm.valid).toBeTruthy();
  });

  it('should set isLoading to true on submit with valid form', () => {
    component.loginForm.patchValue({
      email: 'test@example.com',
      password: 'password123'
    });
    component.onSubmit();
    expect(component.isLoading).toBeTruthy();
  });

  it('should not submit with invalid form', () => {
    spyOn(console, 'log');
    component.onSubmit();
    expect(console.log).not.toHaveBeenCalled();
  });
});
