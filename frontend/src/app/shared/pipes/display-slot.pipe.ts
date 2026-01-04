import { Pipe, PipeTransform } from '@angular/core';
import { TimeSlot } from '../models/availability.model';

/**
 * DisplaySlotPipe
 * 
 * Transforms a TimeSlot object into a human-readable string format "HH:mm - HH:mm" (24-hour format)
 * 
 * Usage: {{ slot | displaySlot }}
 * Output: "09:30 - 10:00"
 * 
 * Benefits of pipe architecture:
 * - ✅ Pure function - no side effects, easy to test (no TestBed required)
 * - ✅ Reusable across components (Dashboard, Reports, etc.)
 * - ✅ Separation of concerns - presentation logic isolated from components
 * - ✅ Performance - cached results for unchanged inputs (pure=true)
 */
@Pipe({
  name: 'displaySlot',
  standalone: true,
  pure: true
})
export class DisplaySlotPipe implements PipeTransform {
  /**
   * Transforms a TimeSlot into formatted string
   * @param slot TimeSlot object with startTime and endTime (ISO 8601 strings)
   * @returns String in format "HH:mm - HH:mm" (Spanish locale, 24-hour)
   * 
   * @example
   * slot = { startTime: "2026-01-15T09:30:00Z", endTime: "2026-01-15T10:00:00Z" }
   * transform(slot) // returns "09:30 - 10:00"
   */
  transform(slot: TimeSlot): string {
    if (!slot || !slot.startTime || !slot.endTime) {
      return '';
    }

    try {
      const start = new Date(slot.startTime);
      const end = new Date(slot.endTime);

      const startStr = start.toLocaleTimeString('es-ES', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
        timeZone: 'UTC'
      });

      const endStr = end.toLocaleTimeString('es-ES', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
        timeZone: 'UTC'
      });

      return `${startStr} - ${endStr}`;
    } catch (error) {
      console.error('DisplaySlotPipe: Error transforming slot', error);
      return '';
    }
  }
}
