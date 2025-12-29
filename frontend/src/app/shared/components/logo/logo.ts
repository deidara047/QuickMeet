import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-logo',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './logo.html',
  styleUrl: './logo.css',
})
export class LogoComponent {
  @Input() size: 'sm' | 'md' | 'lg' | 'xl' = 'md';
  @Input() showText = false;
  
  logoPath = '/logo-qm.svg';
  appName = 'QuickMeet';
  altText = 'QuickMeet - Agendamiento de Citas';

  getSizeClass(): string {
    const sizeMap = {
      sm: 'w-8 h-8',
      md: 'w-12 h-12',
      lg: 'w-16 h-16',
      xl: 'w-32 h-32'
    };
    return sizeMap[this.size];
  }
}
