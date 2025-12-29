import { Routes } from '@angular/router';
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: '<p>Dashboard component</p>'
})
class DashboardComponent {}

export const dashboardRoutes: Routes = [
  {
    path: '',
    component: DashboardComponent
  }
];

