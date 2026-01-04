import { DisplaySlotPipe } from './display-slot.pipe';
import { TimeSlot } from '../models/availability.model';

/**
 * DisplaySlotPipe Test Suite
 * 
 * Tests for the DisplaySlotPipe transformation function.
 * NOTE: No TestBed required - pipe is a pure function
 * This results in very fast test execution (~1-2ms per test)
 */
describe('DisplaySlotPipe', () => {
  let pipe: DisplaySlotPipe;

  beforeEach(() => {
    // Direct instantiation - no TestBed needed for pure pipes
    pipe = new DisplaySlotPipe();
  });

  describe('Formateo básico de horarios', () => {
    it('debería formatear TimeSlot en formato "HH:mm - HH:mm"', () => {
      // Arrange
      const slot: TimeSlot = {
        id: 1,
        startTime: new Date('2026-01-15T09:00:00Z'),
        endTime: new Date('2026-01-15T09:30:00Z'),
        status: 'Available',
        providerId: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      // Act
      const result = pipe.transform(slot);

      // Assert
      expect(result).toBe('09:00 - 09:30');
    });

    it('debería manejar diferentes valores de tiempo correctamente', () => {
      const slot: TimeSlot = {
        id: 1,
        startTime: new Date('2026-01-15T14:45:00Z'),
        endTime: new Date('2026-01-15T15:30:00Z'),
        status: 'Available',
        providerId: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      const result = pipe.transform(slot);

      expect(result).toBe('14:45 - 15:30');
    });

    it('debería manejar horarios nocturnos (después de 18:00)', () => {
      const slot: TimeSlot = {
        id: 1,
        startTime: new Date('2026-01-15T18:00:00Z'),
        endTime: new Date('2026-01-15T19:00:00Z'),
        status: 'Available',
        providerId: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      const result = pipe.transform(slot);

      expect(result).toBe('18:00 - 19:00');
    });
  });

  describe('Casos especiales - Transiciones a medianoche', () => {
    it('debería manejar slot que cruza medianoche (día siguiente)', () => {
      const slot: TimeSlot = {
        id: 1,
        startTime: new Date('2026-01-15T23:00:00Z'),
        endTime: new Date('2026-01-16T00:30:00Z'),
        status: 'Available',
        providerId: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      const result = pipe.transform(slot);

      expect(result).toBe('23:00 - 00:30');
    });
  });

  describe('Casos especiales - Entradas en String ISO 8601', () => {
    it('debería parsear formato ISO 8601 con notación Z', () => {
      const slot: TimeSlot = {
        id: 1,
        startTime: '2026-01-15T09:00:00Z' as any,
        endTime: '2026-01-15T09:30:00Z' as any,
        status: 'Available',
        providerId: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      const result = pipe.transform(slot);

      expect(result).toBe('09:00 - 09:30');
    });

    it('debería parsear formato ISO 8601 con notación de offset', () => {
      const slot: TimeSlot = {
        id: 1,
        startTime: '2026-01-15T09:00:00+00:00' as any,
        endTime: '2026-01-15T09:30:00+00:00' as any,
        status: 'Available',
        providerId: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      const result = pipe.transform(slot);

      expect(result).toBe('09:00 - 09:30');
    });
  });

  describe('Manejo de errores', () => {
    it('debería retornar string vacío para slot nulo', () => {
      const result = pipe.transform(null as any);

      expect(result).toBe('');
    });

    it('debería retornar string vacío para slot indefinido', () => {
      const result = pipe.transform(undefined as any);

      expect(result).toBe('');
    });

    it('debería retornar string vacío para slot sin startTime', () => {
      const slot: any = {
        id: 1,
        startTime: null,
        endTime: new Date('2026-01-15T09:30:00Z'),
        status: 'Available',
        providerId: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      const result = pipe.transform(slot);

      expect(result).toBe('');
    });

    it('debería retornar string vacío para slot sin endTime', () => {
      const slot: any = {
        id: 1,
        startTime: new Date('2026-01-15T09:00:00Z'),
        endTime: null,
        status: 'Available',
        providerId: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      const result = pipe.transform(slot);

      expect(result).toBe('');
    });
  });

  describe('Localización española y formato de horario', () => {
    it('debería usar formato 24-horas consistentemente', () => {
      const testCases = [
        {
          input: {
            startTime: new Date('2026-01-15T00:00:00Z'),
            endTime: new Date('2026-01-15T01:00:00Z')
          },
          expected: '00:00 - 01:00'
        },
        {
          input: {
            startTime: new Date('2026-01-15T12:00:00Z'),
            endTime: new Date('2026-01-15T13:00:00Z')
          },
          expected: '12:00 - 13:00'
        },
        {
          input: {
            startTime: new Date('2026-01-15T23:00:00Z'),
            endTime: new Date('2026-01-16T00:00:00Z')
          },
          expected: '23:00 - 00:00'
        }
      ];

      testCases.forEach(testCase => {
        const slot: TimeSlot = {
          id: 1,
          startTime: testCase.input.startTime,
          endTime: testCase.input.endTime,
          status: 'Available',
          providerId: 1,
          createdAt: new Date(),
          updatedAt: new Date()
        };

        const result = pipe.transform(slot);
        expect(result).toBe(testCase.expected);
      });
    });
  });

  describe('Variaciones en duración de slots', () => {
    it('debería manejar slots de 15 minutos', () => {
      const slot: TimeSlot = {
        id: 1,
        startTime: new Date('2026-01-15T09:00:00Z'),
        endTime: new Date('2026-01-15T09:15:00Z'),
        status: 'Available',
        providerId: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      const result = pipe.transform(slot);

      expect(result).toBe('09:00 - 09:15');
    });

    it('debería manejar slots de 30 minutos', () => {
      const slot: TimeSlot = {
        id: 1,
        startTime: new Date('2026-01-15T09:00:00Z'),
        endTime: new Date('2026-01-15T09:30:00Z'),
        status: 'Available',
        providerId: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      const result = pipe.transform(slot);

      expect(result).toBe('09:00 - 09:30');
    });

    it('debería manejar slots de 60 minutos (1 hora)', () => {
      const slot: TimeSlot = {
        id: 1,
        startTime: new Date('2026-01-15T09:00:00Z'),
        endTime: new Date('2026-01-15T10:00:00Z'),
        status: 'Available',
        providerId: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      const result = pipe.transform(slot);

      expect(result).toBe('09:00 - 10:00');
    });
  });
});
