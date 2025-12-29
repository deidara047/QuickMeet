import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LogoComponent } from '../../../shared/components/logo/logo.component';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, LogoComponent],
  template: `
    <nav class="navbar">
      <div class="navbar-container">
        <div class="navbar-brand">
          <app-logo size="sm" [showText]="false"></app-logo>
          <span class="brand-text">QuickMeet</span>
        </div>
        
        <div class="navbar-menu">
          <a routerLink="/" class="nav-link">Inicio</a>
          <a routerLink="/appointments" class="nav-link">Citas</a>
          <a routerLink="/profile" class="nav-link">Perfil</a>
        </div>
        
        <div class="navbar-actions">
          <button class="btn-logout">Salir</button>
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      background-color: #ffffff;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
      padding: 1rem 0;
      position: sticky;
      top: 0;
      z-index: 100;
    }

    .navbar-container {
      max-width: 1280px;
      margin: 0 auto;
      padding: 0 1.5rem;
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .navbar-brand {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      text-decoration: none;
      cursor: pointer;
    }

    .brand-text {
      font-size: 1.5rem;
      font-weight: 600;
      color: #4B8BA8;
    }

    .navbar-menu {
      display: flex;
      gap: 2rem;
      align-items: center;
      flex: 1;
      margin-left: 3rem;
    }

    .nav-link {
      color: #333;
      text-decoration: none;
      font-weight: 500;
      transition: color 0.3s ease;
      font-size: 0.95rem;
    }

    .nav-link:hover {
      color: #4B8BA8;
    }

    .navbar-actions {
      display: flex;
      gap: 1rem;
      align-items: center;
    }

    .btn-logout {
      padding: 0.5rem 1rem;
      background-color: #f44336;
      color: white;
      border: none;
      border-radius: 0.375rem;
      font-weight: 500;
      cursor: pointer;
      transition: background-color 0.3s ease;
    }

    .btn-logout:hover {
      background-color: #da190b;
    }

    @media (max-width: 768px) {
      .navbar-menu {
        display: none;
      }

      .navbar-container {
        flex-wrap: wrap;
      }

      .brand-text {
        font-size: 1.25rem;
      }
    }
  `]
})
export class NavbarComponent {}
