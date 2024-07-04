import {
  Directive,
  ElementRef,
  HostListener,
  Input,
  OnDestroy,
  OnInit,
  Renderer2,
  input,
} from '@angular/core';
import { NavigationStart, Router } from '@angular/router';
import { Subscription, filter } from 'rxjs';

@Directive({
  selector: '[appTooltip]',
  standalone: true,
})
export class TooltipDirective implements OnInit, OnDestroy {
  @Input('appTooltip') tooltipText: string = '';
  private tooltipElement: HTMLElement | null = null;
  hoverDelay = 500;
  hoverDelayTimeout?: any;
  private routerSubscription: Subscription | undefined;

  constructor(
    private el: ElementRef,
    private renderer: Renderer2,
    private router: Router,
  ) {}

  ngOnDestroy(): void {
    if (this.routerSubscription) {
      this.routerSubscription.unsubscribe();
    }
  }
  ngOnInit() {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationStart))
      .subscribe(() => {
        this.hideTooltip(); // Call your method to remove the tooltip
      });
  }

  @HostListener('mouseenter') onMouseEnter() {
    // Clear any existing timeout to prevent multiple tooltips
    if (this.hoverDelayTimeout) {
      clearTimeout(this.hoverDelayTimeout);
    }

    // Start the delay timer
    this.hoverDelayTimeout = setTimeout(() => {
      this.showTooltip();
    }, this.hoverDelay);
  }

  @HostListener('mouseleave') onMouseLeave() {
    // If leaving before the delay, clear the timeout and don't show the tooltip
    if (this.hoverDelayTimeout) {
      clearTimeout(this.hoverDelayTimeout);
    }
    this.hideTooltip();
  }

  showTooltip() {
    this.tooltipElement = this.renderer.createElement('span');
    const text = this.renderer.createText(this.tooltipText);
    this.renderer.appendChild(this.tooltipElement, text);
    this.renderer.addClass(this.tooltipElement, 'tooltip');

    // Set initial style to ensure dimensions can be measured
    this.renderer.setStyle(this.tooltipElement, 'position', 'fixed');
    this.renderer.setStyle(this.tooltipElement, 'visibility', 'hidden');
    this.renderer.appendChild(document.body, this.tooltipElement);

    // Calculate position
    const hostPos = this.el.nativeElement.getBoundingClientRect();
    const tooltipPos = this.tooltipElement!.getBoundingClientRect();
    const top = hostPos.bottom + 10;
    const left =
      hostPos.left + (hostPos.width - tooltipPos.width) / 2 + window.scrollX;

    // Apply calculated position
    this.renderer.setStyle(this.tooltipElement, 'top', `${top}px`);
    this.renderer.setStyle(this.tooltipElement, 'left', `${left}px`);
    this.renderer.setStyle(this.tooltipElement, 'visibility', 'visible');
  }

  hideTooltip() {
    if (this.tooltipElement && document.body.contains(this.tooltipElement)) {
      document.body.removeChild(this.tooltipElement);
    }
  }
  removeTooltip() {
    // Check if the tooltip element exists and remove it
    if (this.tooltipElement) {
      document.body.removeChild(this.tooltipElement);
    }
  }
}
