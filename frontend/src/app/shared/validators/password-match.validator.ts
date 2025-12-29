import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const PasswordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password');
  const passwordConfirmation = control.get('passwordConfirmation');

  if (!password || !passwordConfirmation) {
    return null;
  }

  if (password.value !== passwordConfirmation.value) {
    passwordConfirmation.setErrors({ ...passwordConfirmation.errors, passwordMismatch: true });
    return { passwordMismatch: true };
  } else {
    if (passwordConfirmation.errors) {
      delete passwordConfirmation.errors['passwordMismatch'];
      if (Object.keys(passwordConfirmation.errors).length === 0) {
        passwordConfirmation.setErrors(null);
      }
    }
  }

  return null;
};
