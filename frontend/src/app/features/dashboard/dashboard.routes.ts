import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard';
import { ProfileEditorComponent } from '../../shared/components/profile-editor/profile-editor';
import { AvailabilityConfiguratorComponent } from '../../shared/components/availability-configurator/availability-configurator';

export const dashboardRoutes: Routes = [
  {
    path: '',
    component: DashboardComponent
  },
  {
    path: 'profile',
    component: ProfileEditorComponent
  },
  {
    path: 'availability',
    component: AvailabilityConfiguratorComponent
  }
];

