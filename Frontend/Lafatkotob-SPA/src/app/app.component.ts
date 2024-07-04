import { Component } from '@angular/core';
import {
  NavigationEnd,
  Router,
  RouterLink,
  RouterOutlet,
} from '@angular/router';
import { LoginRegisterComponent } from './Auth/components/login-register/login-register.component';
import { NavbarComponent } from './shared/components/navbar/navbar.component';
import { AddBookComponent } from './shared/components/add-book/add-book.component';
import { RecommendationComponent } from './shared/components/recommendation/recommendation.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    LoginRegisterComponent,
    NavbarComponent,
    AddBookComponent,
    RouterLink,
    RecommendationComponent,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  constructor(private router: Router) {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        window.scrollTo(0, 0);
      }
    });
  }
  title = 'Lafatkotob-SPA';
}
