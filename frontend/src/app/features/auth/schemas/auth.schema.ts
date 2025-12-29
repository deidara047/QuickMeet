import { z } from 'zod';

/**
 * Zod Schema para validación de credenciales de login
 * Proporciona validación type-safe compartible entre cliente y servidor
 */
export const loginSchema = z.object({
  email: z
    .string()
    .email('Ingresa un email válido')
    .toLowerCase()
    .trim(),
  password: z
    .string()
    .min(6, 'La contraseña debe tener al menos 6 caracteres')
    .max(128, 'La contraseña no puede exceder 128 caracteres'),
});

/**
 * Type derivado del schema para type-safety en TypeScript
 */
export type LoginFormData = z.infer<typeof loginSchema>;

/**
 * Zod Schema para validación de registro
 */
export const registerSchema = z
  .object({
    email: z
      .string()
      .email('Ingresa un email válido')
      .toLowerCase()
      .trim(),
    password: z
      .string()
      .min(6, 'La contraseña debe tener al menos 6 caracteres')
      .max(128, 'La contraseña no puede exceder 128 caracteres'),
    confirmPassword: z
      .string()
      .min(6, 'La contraseña debe tener al menos 6 caracteres'),
    username: z
      .string()
      .min(3, 'El nombre debe tener al menos 3 caracteres')
      .max(50, 'El nombre no puede exceder 50 caracteres')
      .trim(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Las contraseñas no coinciden',
    path: ['confirmPassword'], // Especifica dónde mostrar el error
  });

/**
 * Type derivado del schema de registro
 */
export type RegisterFormData = z.infer<typeof registerSchema>;
