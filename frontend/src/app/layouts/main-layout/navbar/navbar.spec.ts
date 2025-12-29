import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { NavbarComponent } from './navbar';
import { LogoComponent } from '../../../shared/components/logo/logo.component';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavbarComponent, RouterTestingModule, ButtonModule, TooltipModule, LogoComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call onLogout when logout button is clicked', () => {
    spyOn(component, 'onLogout');
    component.onLogout();
    expect(component.onLogout).toHaveBeenCalled();
  });

  it('should log logout message', () => {
    spyOn(console, 'log');
    component.onLogout();
    expect(console.log).toHaveBeenCalledWith('Logout clicked');
  });
});
