import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  inject,
  signal
} from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import {
  ActivatedRoute,
  Router,
  RouterLink
} from '@angular/router';

import { AuthService } from '../../../Core/services/auth.service';

@Component({
  selector: 'app-login',

  // Provides reactive-form directives and Angular navigation.
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],

  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class Login {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // Provides access to query parameters such as returnUrl.
  private readonly route = inject(ActivatedRoute);

  // Creates a strongly typed login form.
  readonly loginForm = this.formBuilder.nonNullable.group({
    email: [
      '',
      [
        Validators.required,
        Validators.email
      ]
    ],
    password: [
      '',
      Validators.required
    ]
  });

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');

  // Displays temporary feedback after successful registration.
  readonly successMessage = signal(
    history.state?.['registrationSuccess'] === true
      ? 'Registration successful. Please sign in with your new account.'
      : ''
  );

  /**
   * Validates the form and sends the credentials to the API.
   */
  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    this.authService
      .login(this.loginForm.getRawValue())
      .subscribe({
        next: (response) => {
          // Saves the JWT and immediately updates the
          // authentication and role signals.
          this.authService.saveToken(response.token);

          this.isSubmitting.set(false);

          // Reads the protected page requested before login.
          const returnUrl =
            this.route.snapshot.queryParamMap.get('returnUrl');

          // Selects a role-appropriate default destination
          // when Login was opened directly.
          const defaultUrl =
            this.authService.isAdmin()
              ? '/admin'
              : '/menu';

          // Allows only internal application routes.
          // A valid protected return URL takes priority.
          const safeReturnUrl =
            returnUrl?.startsWith('/') &&
            !returnUrl.startsWith('//')
              ? returnUrl
              : defaultUrl;

          void this.router.navigateByUrl(safeReturnUrl);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            error.error?.message ??
              'Login failed. Check your email and password.'
          );

          this.isSubmitting.set(false);
        }
      });
  }
}