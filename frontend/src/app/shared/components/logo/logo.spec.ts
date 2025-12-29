import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LogoComponent } from './logo';

describe('LogoComponent', () => {
  let component: LogoComponent;
  let fixture: ComponentFixture<LogoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LogoComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(LogoComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have default size md', () => {
    expect(component.size).toBe('md');
  });

  it('should have showText false by default', () => {
    expect(component.showText).toBe(false);
  });

  it('should accept size input', () => {
    component.size = 'lg';
    expect(component.size).toBe('lg');
  });

  it('should accept showText input', () => {
    component.showText = true;
    expect(component.showText).toBe(true);
  });

  it('should render logo image with correct src', () => {
    const img = fixture.nativeElement.querySelector('img');
    expect(img.src).toContain('/logo-qm.svg');
  });

  it('should render logo text when showText is true', () => {
    component.showText = true;
    fixture.detectChanges();
    const span = fixture.nativeElement.querySelector('.logo-text');
    expect(span).toBeTruthy();
    expect(span.textContent).toBe('QuickMeet');
  });

  it('should not render logo text when showText is false', () => {
    component.showText = false;
    fixture.detectChanges();
    const span = fixture.nativeElement.querySelector('.logo-text');
    expect(span).toBeFalsy();
  });

  it('should apply correct size class', () => {
    component.size = 'xl';
    fixture.detectChanges();
    const container = fixture.nativeElement.querySelector('.logo-container');
    expect(container.classList.contains('size-xl')).toBeTruthy();
  });
});
