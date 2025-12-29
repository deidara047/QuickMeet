import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-logo',
  standalone: true,
  template: `
    <div class="logo-container" [class]="'size-' + size">
      <img 
        [src]="logoPath" 
        [alt]="altText"
        class="logo-image"
      />
      <span *ngIf="showText" class="logo-text">{{ appName }}</span>
    </div>
  `,
  styles: [`
    .logo-container {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      cursor: pointer;
      transition: opacity 0.3s ease;
    }

    .logo-container:hover {
      opacity: 0.8;
    }

    .logo-image {
      width: 100%;
      height: 100%;
      object-fit: contain;
    }

    .size-sm {
      width: 32px;
      height: 32px;
    }

    .size-md {
      width: 48px;
      height: 48px;
    }

    .size-lg {
      width: 64px;
      height: 64px;
    }

    .size-xl {
      width: 128px;
      height: 128px;
    }

    .logo-text {
      font-weight: 700;
      font-size: 1.2rem;
      color: inherit;
      white-space: nowrap;
    }
  `]
})
export class LogoComponent {
  @Input() size: 'sm' | 'md' | 'lg' | 'xl' = 'md';
  @Input() showText = false;
  
  logoPath = 'assets/logo/logo-qm.svg';
  appName = 'QuickMeet';
  altText = 'QuickMeet - Agendamiento de Citas';
}
