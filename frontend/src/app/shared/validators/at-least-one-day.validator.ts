import { AbstractControl, ValidationErrors, ValidatorFn, FormArray } from '@angular/forms';

export function AtLeastOneDayValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const formArray = control as FormArray;

    if (!formArray || !Array.isArray(formArray.value)) {
      return null;
    }

    const hasAtLeastOneEnabledDay = formArray.value.some((day: any) => day.enabled === true);

    if (!hasAtLeastOneEnabledDay) {
      return { atLeastOneDay: true };
    }

    return null;
  };
}
