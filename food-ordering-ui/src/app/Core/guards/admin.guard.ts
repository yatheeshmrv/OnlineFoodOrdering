import { inject } from '@angular/core';
import {
  CanActivateFn,
  Router
} from '@angular/router';

import { AuthService } from '../services/auth.service';

// Allows only authenticated users with the Admin role
// to open protected Admin routes.
export const adminGuard: CanActivateFn = (
  _route,
  routerState
) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // A logged-out visitor is sent to Login.
  // The requested Admin URL is preserved for after login.
  if (!authService.isLoggedIn()) {
    return router.createUrlTree(
      ['/login'],
      {
        queryParams: {
          returnUrl: routerState.url
        }
      }
    );
  }

  // A logged-in Admin can open the requested route.
  if (authService.hasRole('Admin')) {
    return true;
  }

  // A logged-in customer cannot enter the Admin area.
  return router.createUrlTree(['/menu']);
};