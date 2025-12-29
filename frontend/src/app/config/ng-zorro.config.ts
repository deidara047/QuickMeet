import { provideAnimations } from '@angular/platform-browser/animations';
import { NzConfig, provideNzI18n, es_ES } from 'ng-zorro-antd/i18n';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

const nzConfig: NzConfig = {
  nz: {
    message: { nzTop: 24, nzMaxStack: 7, nzPauseOnHover: true, nzDuration: 4500 },
    notification: { nzTop: 24, nzRight: 0, nzMaxStack: 7, nzPauseOnHover: true, nzDuration: 4500 },
    select: { nzMaxTagPlaceholder: 'Más...' },
  }
};

export const PRIMENG_CONFIG_PROVIDERS = [
  provideAnimations(),
  provideNzI18n(es_ES, nzConfig),
];
