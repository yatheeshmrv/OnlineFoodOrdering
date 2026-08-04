import {
  Component,
  inject
} from '@angular/core';

import {
  Router,
  RouterLink,
  RouterLinkActive
} from '@angular/router';

import { AuthService } from '../../../Core/services/auth.service';

@Component({
  selector: 'app-navbar',

  // Enables Angular navigation and active-link styling.
  imports: [
    RouterLink,
    RouterLinkActive
  ],

  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class Navbar {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // Exposes AuthService's read-only authentication signal
  // so the template can react to login and logout.
  protected readonly isAuthenticated =
    this.authService.isAuthenticated;

  // Controls Admin-only navigation elements.
  // This value updates automatically after login or logout.
  protected readonly isAdmin =
    this.authService.isAdmin;

  // Removes the JWT and redirects the user to Login.
  protected logout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }
}