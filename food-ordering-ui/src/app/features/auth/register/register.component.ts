import { HttpErrorResponse } from '@angular/common/http';

import {
  Component,
  inject,
  signal
} from '@angular/core';

import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';

import {
  Router,
  RouterLink
} from '@angular/router';

import { AuthService } from '../../../Core/services/auth.service';

// Form-level validator that compares both password fields.
const passwordsMatchValidator: ValidatorFn = (
  form: AbstractControl
): ValidationErrors | null => {
  const password = form.get('password')?.value;
  const confirmPassword = form.get('confirmPassword')?.value;

  return password === confirmPassword
    ? null
    : { passwordsMismatch: true };
};

@Component({
  selector: 'app-register',

  // Provides reactive-form directives and Angular navigation.
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],

  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class Register {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // Mirrors the backend RegisterDto and FluentValidation rules.
  readonly registerForm = this.formBuilder.nonNullable.group(
    {
      fullName: [
        '',
        [
          Validators.required,
          Validators.maxLength(100)
        ]
      ],
      email: [
        '',
        [
          Validators.required,
          Validators.email
        ]
      ],
      phoneNumber: [
        '',
        [
          Validators.required,
          Validators.pattern(/^[6-9]\d{9}$/)
        ]
      ],
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(6),
          Validators.maxLength(100),
          Validators.pattern(/[a-z]/),
          Validators.pattern(/[A-Z]/),
          Validators.pattern(/\d/)
        ]
      ],
      confirmPassword: [
        '',
        Validators.required
      ]
    },
    {
      validators: passwordsMatchValidator
    }
  );

  readonly isSubmitting = signal<boolean>(false);
  readonly errorMessage = signal<string>('');

  // Validates the form and creates a Customer account.
  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const formValue = this.registerForm.getRawValue();

    // Trims text fields without changing either password.
    const request = {
      ...formValue,
      fullName: formValue.fullName.trim(),
      email: formValue.email.trim(),
      phoneNumber: formValue.phoneNumber.trim()
    };

    this.authService
      .register(request)
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);

          // Registration does not return a JWT, so the customer
          // must sign in using the newly created account.
          void this.router.navigateByUrl('/login', {
  state: {
    registrationSuccess: true
  }
});
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            error.error?.message ??
              'Registration failed. Please try again.'
          );

          this.isSubmitting.set(false);
        }
      });
  }
}