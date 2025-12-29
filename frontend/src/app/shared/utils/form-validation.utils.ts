import { FormGroup, AbstractControl } from '@angular/forms';
import { z, ZodError } from 'zod';

/**
 * Utility para integrar Zod con Angular Reactive Forms
 * Proporciona validación type-safe y manejo de errores estructurado
 */

/**
 * Valida un FormGroup usando un schema Zod
 * @param formGroup - FormGroup de Angular a validar
 * @param schema - Zod schema para validación
 * @returns { valid: boolean, errors: Record<string, string> }
 */
export function validateFormWithZod<T extends z.ZodType>(
  formGroup: FormGroup,
  schema: T
): { valid: boolean; errors: Record<string, string> } {
  const formData = formGroup.value;

  try {
    schema.parse(formData);
    return { valid: true, errors: {} };
  } catch (error) {
    if (error instanceof ZodError) {
      const errors: Record<string, string> = {};
      error.issues.forEach((err) => {
        const path = err.path.join('.');
        errors[path] = err.message;
      });
      return { valid: false, errors };
    }
    return { valid: false, errors: { form: 'Validation error' } };
  }
}

/**
 * Obtiene el mensaje de error para un campo específico
 * Útil para mostrar errores en templates de manera limpia
 * @param control - AbstractControl del campo
 * @param fieldName - Nombre del campo para identificar
 * @returns string con el mensaje de error, o empty string si no hay error
 */
export function getFieldError(
  control: AbstractControl | null,
  fieldName: string
): string {
  if (!control || !control.errors || !control.touched) {
    return '';
  }

  // Si Zod validó el campo, el error estará en control.errors.zodError
  if (control.errors['zodError']) {
    return control.errors['zodError'][0];
  }

  // Fallback para otros tipos de errores
  if (control.errors['required']) {
    return `${fieldName} es requerido`;
  }
  if (control.errors['email']) {
    return 'Ingresa un email válido';
  }
  if (control.errors['minlength']) {
    return `${fieldName} debe tener al menos ${control.errors['minlength'].requiredLength} caracteres`;
  }

  return '';
}

/**
 * Valida un objeto usando Zod y retorna los errores formateados
 * @param data - Objeto a validar
 * @param schema - Zod schema
 * @returns Objeto con errores formateados por campo
 */
export function validateWithZod<T>(
  data: unknown,
  schema: z.ZodType<T>
): { success: boolean; data?: T; errors?: Record<string, string[]> } {
  try {
    const validData = schema.parse(data);
    return { success: true, data: validData };
  } catch (error) {
    if (error instanceof ZodError) {
      const errors: Record<string, string[]> = {};
      error.issues.forEach((err) => {
        const path = err.path.join('.');
        if (!errors[path]) {
          errors[path] = [];
        }
        errors[path].push(err.message);
      });
      return { success: false, errors };
    }
    return {
      success: false,
      errors: { form: ['Unknown validation error'] },
    };
  }
}
