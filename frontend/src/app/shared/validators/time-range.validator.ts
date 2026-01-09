import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function TimeRangeValidator(): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const startTime = group.get('startTime')?.value;
    const endTime = group.get('endTime')?.value;

    if (!startTime || !endTime) {
      return null;
    }

    const [startHour, startMin] = startTime.split(':').map(Number);
    const [endHour, endMin] = endTime.split(':').map(Number);

    const startInMinutes = startHour * 60 + startMin;
    const endInMinutes = endHour * 60 + endMin;

    if (startInMinutes >= endInMinutes) {
      return { timeRange: { startTime, endTime } };
    }

    return null;
  };
}
