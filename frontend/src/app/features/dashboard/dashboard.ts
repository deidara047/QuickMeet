import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../core/services/auth.service';
import { ProfileService } from '../../core/services/profile.service';
import { AvailabilityService } from '../../core/services/availability.service';
import { ProviderProfile } from '../../shared/models/availability.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    CardModule,
    ButtonModule,
    ToastModule,
    ProgressSpinnerModule
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  providers: [MessageService]
})
export class DashboardComponent implements OnInit {
  profile: ProviderProfile | null = null;
  loading = true;
  publicLink = '';

  constructor(
    private authService: AuthService,
    private profileService: ProfileService,
    private messageService: MessageService,
    public router: Router
  ) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    const userId = this.authService.getCurrentUserId();
    
    if (!userId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'No se pudo cargar el perfil',
        life: 3000
      });
      this.router.navigate(['/login']);
      return;
    }

    this.profileService.getProfile(userId).subscribe({
      next: (profile) => {
        this.profile = profile;
        this.publicLink = `${window.location.origin}/${profile.username}`;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        const errorMsg = err.error?.error || 'Error al cargar el perfil';
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: errorMsg,
          life: 3000
        });
      }
    });
  }

  copyToClipboard(): void {
    navigator.clipboard.writeText(this.publicLink).then(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Copiado',
        detail: 'Enlace público copiado al portapapeles',
        life: 2000
      });
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
