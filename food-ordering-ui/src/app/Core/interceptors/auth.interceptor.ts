import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  // Functional interceptors do not have constructors.
  // inject() gets AuthService from Angular's dependency injection system.
  const authService = inject(AuthService);

  // Read the JWT previously saved after successful login.
  const token = authService.getToken();

  // If the user is not logged in, send the original request unchanged.
  if (!token) {
    return next(request);
  }

  // Angular HTTP requests are immutable.
  // clone() creates a new request containing the Authorization header.
  const authenticatedRequest = request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });

  return next(authenticatedRequest);
};