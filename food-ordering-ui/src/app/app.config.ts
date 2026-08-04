import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection
} from '@angular/core';

import {
  provideHttpClient,
  withInterceptors
} from '@angular/common/http';

import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { authInterceptor } from './Core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),

    // Makes HttpClient available and runs authInterceptor
    // for every outgoing HTTP request.
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),

    provideRouter(routes)
  ]
};