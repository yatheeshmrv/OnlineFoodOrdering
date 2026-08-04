import { inject } from '@angular/core';
import {
  CanActivateFn,
  Router
} from '@angular/router';

import { AuthService } from '../services/auth.service';

/**
 * Protects routes that require an authenticated customer.
 *
 * Returns:
 * - true when a JWT exists
 * - a Login-page UrlTree when the customer is logged out
 */
export const authGuard: CanActivateFn = (
  _route,
  state
) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // Preserve the requested URL so the customer can return
  // to it after successfully logging in.
  return router.createUrlTree(
    ['/login'],
    {
      queryParams: {
        returnUrl: state.url
      }
    }
  );
};