import { CommonModule, NgStyle } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  OnInit,
  Renderer2,
  ViewChildren,
} from '@angular/core';

@Component({
  selector: 'app-magic-text',
  standalone: true,
  imports: [NgStyle, CommonModule],
  templateUrl: './magic-text.component.html',
  styleUrl: './magic-text.component.css',
})
export class MagicTextComponent implements AfterViewInit {
  stars: any[] = Array(3).fill(null);

  constructor(
    private el: ElementRef,
    private renderer: Renderer2,
  ) {}

  ngAfterViewInit(): void {
    this.animateStars();
  }

  private animateStars(): void {
    const stars = this.el.nativeElement.querySelectorAll('.magic-star');

    // Initialize stars with random positions
    stars.forEach((star: HTMLElement) => {
      this.setPosition(star);
    });

    // Update positions every 2 seconds
    setInterval(() => {
      stars.forEach((star: HTMLElement) => {
        this.setPosition(star);
      });
    }, 3000);
  }
  private setPosition(star: HTMLElement): void {
    const left = this.rand(0, 100);
    const top = this.rand(0, 100);

    this.renderer.setStyle(star, 'left', `${left}%`);
    this.renderer.setStyle(star, 'top', `${top}%`);
  }

  private animate(star: HTMLElement): void {
    // Reset the animation
    this.renderer.setStyle(star, 'animation', 'none');

    // Force reflow
    void star.offsetWidth;

    // Update position
    this.setPosition(star);
  }
  private generateStars(count: number): any[] {
    return Array.from({ length: count }, () => ({
      left: this.rand(-10, 110), // Giving a bit more range for demonstration
      top: this.rand(-40, 80),
    }));
  }

  private rand(min: number, max: number): number {
    // Helper function to generate a random number within a given range
    return Math.floor(Math.random() * (max - min + 1)) + min;
  }
}
