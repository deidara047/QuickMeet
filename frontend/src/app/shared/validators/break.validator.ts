import { AbstractControl, ValidationErrors, ValidatorFn, FormGroup } from '@angular/forms';

export function BreakValidator(): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const breakStartTime = group.get('startTime')?.value;
    const breakEndTime = group.get('endTime')?.value;

    if (!breakStartTime || !breakEndTime) {
      return null;
    }

    const [bStartHour, bStartMin] = breakStartTime.split(':').map(Number);
    const [bEndHour, bEndMin] = breakEndTime.split(':').map(Number);

    const breakStartInMinutes = bStartHour * 60 + bStartMin;
    const breakEndInMinutes = bEndHour * 60 + bEndMin;

    if (breakStartInMinutes >= breakEndInMinutes) {
      return { breakTimeRange: { startTime: breakStartTime, endTime: breakEndTime } };
    }

    return null;
  };
}

export function BreakWithinWorkingHours(workingStartTime: string, workingEndTime: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const breakStartTime = group.get('startTime')?.value;
    const breakEndTime = group.get('endTime')?.value;

    if (!breakStartTime || !breakEndTime) {
      return null;
    }

    const [wsHour, wsMin] = workingStartTime.split(':').map(Number);
    const [weHour, weMin] = workingEndTime.split(':').map(Number);
    const [bsHour, bsMin] = breakStartTime.split(':').map(Number);
    const [beHour, beMin] = breakEndTime.split(':').map(Number);

    const workStartInMinutes = wsHour * 60 + wsMin;
    const workEndInMinutes = weHour * 60 + weMin;
    const breakStartInMinutes = bsHour * 60 + bsMin;
    const breakEndInMinutes = beHour * 60 + beMin;

    const isWithinWorkingHours =
      breakStartInMinutes >= workStartInMinutes &&
      breakEndInMinutes <= workEndInMinutes;

    if (!isWithinWorkingHours) {
      return {
        breakOutsideWorkingHours: {
          workingStart: workingStartTime,
          workingEnd: workingEndTime,
          breakStart: breakStartTime,
          breakEnd: breakEndTime
        }
      };
    }

    return null;
  };
}
