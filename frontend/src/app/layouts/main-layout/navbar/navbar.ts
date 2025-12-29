import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { MenubarModule } from 'primeng/menubar';
import { LogoComponent } from '../../../shared/components/logo/logo';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonModule, TooltipModule, MenubarModule, LogoComponent],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class NavbarComponent {
  navItems: MenuItem[] = [
    {
      label: 'Inicio',
      icon: 'pi pi-home',
      routerLink: '/'
    },
    {
      label: 'Citas',
      icon: 'pi pi-calendar',
      routerLink: '/appointments'
    },
    {
      label: 'Perfil',
      icon: 'pi pi-user',
      routerLink: '/profile'
    }
  ];

  onLogout() {
    console.log('Logout clicked');
  }
}
