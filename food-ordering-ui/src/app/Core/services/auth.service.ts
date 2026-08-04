import { HttpClient } from '@angular/common/http';
import {
  computed,
  inject,
  Injectable,
  signal
} from '@angular/core';
import { Observable } from 'rxjs';

import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse
} from '../models/auth.model';

// Represents the JWT payload fields required by the frontend.
interface JwtPayload {
  exp?: number;
  role?: string | string[];
  roles?: string | string[];
  [claim: string]: unknown;
}

@Injectable({
  // Provides one shared AuthService instance throughout the application.
  providedIn: 'root'
})
export class AuthService {
  private readonly httpClient = inject(HttpClient);

  private readonly apiUrl =
    'https://localhost:7068/api/Auth';

  private readonly tokenStorageKey = 'access_token';

  // Stores the decoded payload of the current valid JWT.
  private readonly currentUserState = signal<JwtPayload | null>(
    this.getStoredTokenPayload()
  );

  // True when a valid, non-expired JWT is available.
  readonly isAuthenticated = computed(
    () => this.currentUserState() !== null
  );

  // True when the JWT contains the Admin role.
  readonly isAdmin = computed(
    () => this.hasRoleInPayload(
      this.currentUserState(),
      'Admin'
    )
  );

  // Sends credentials to the login endpoint.
  login(request: LoginRequest): Observable<LoginResponse> {
    return this.httpClient.post<LoginResponse>(
      `${this.apiUrl}/login`,
      request
    );
  }

  // Sends new customer details to the registration endpoint.
  register(
    request: RegisterRequest
  ): Observable<RegisterResponse> {
    return this.httpClient.post<RegisterResponse>(
      `${this.apiUrl}/register`,
      request
    );
  }

  // Stores the JWT and updates the reactive authentication
  // and role state.
  saveToken(token: string): void {
    localStorage.setItem(this.tokenStorageKey, token);

    this.currentUserState.set(
      this.getValidTokenPayload(token)
    );
  }

  // Returns the currently stored JWT.
  getToken(): string | null {
    return localStorage.getItem(this.tokenStorageKey);
  }

  // Checks whether a valid, non-expired JWT is stored.
  isLoggedIn(): boolean {
    return this.getStoredTokenPayload() !== null;
  }

  // Checks whether the current user has a specific role.
  hasRole(role: string): boolean {
    return this.hasRoleInPayload(
      this.getStoredTokenPayload(),
      role
    );
  }

  // Removes the JWT and clears all authentication state.
  logout(): void {
    localStorage.removeItem(this.tokenStorageKey);
    this.currentUserState.set(null);
  }

  // Reads and validates the stored token payload.
  private getStoredTokenPayload(): JwtPayload | null {
    return this.getValidTokenPayload(this.getToken());
  }

  // Returns the payload only when the token has a valid
  // structure and has not expired.
  private getValidTokenPayload(
    token: string | null
  ): JwtPayload | null {
    if (!token) {
      return null;
    }

    const payload = this.decodeToken(token);

    if (!payload || this.isTokenExpired(payload)) {
      return null;
    }

    return payload;
  }

  // Decodes the middle section of a JWT.
  // This does not validate the token signature—the API does that.
  private decodeToken(token: string): JwtPayload | null {
    try {
      const tokenParts = token.split('.');

      if (tokenParts.length !== 3) {
        return null;
      }

      const encodedPayload = tokenParts[1]
        .replace(/-/g, '+')
        .replace(/_/g, '/');

      const paddingLength =
        (4 - (encodedPayload.length % 4)) % 4;

      const paddedPayload =
        encodedPayload + '='.repeat(paddingLength);

      const decodedPayload = atob(paddedPayload);
      const parsedPayload: unknown =
        JSON.parse(decodedPayload);

      if (
        typeof parsedPayload !== 'object' ||
        parsedPayload === null
      ) {
        return null;
      }

      return parsedPayload as JwtPayload;
    } catch {
      return null;
    }
  }

  // JWT expiry values are stored as Unix timestamps in seconds.
  private isTokenExpired(payload: JwtPayload): boolean {
    if (typeof payload.exp !== 'number') {
      return false;
    }

    const currentUnixTime = Math.floor(Date.now() / 1000);

    return payload.exp <= currentUnixTime;
  }

  // Supports both short role claims and the full ASP.NET
  // Core ClaimTypes.Role claim name.
  private hasRoleInPayload(
    payload: JwtPayload | null,
    requiredRole: string
  ): boolean {
    if (!payload) {
      return false;
    }

    const roleClaims: unknown[] = [
      payload.role,
      payload.roles,
      payload[
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
      ]
    ];

    const roles = roleClaims.flatMap((claim) => {
      if (typeof claim === 'string') {
        return [claim];
      }

      if (
        Array.isArray(claim) &&
        claim.every((role) => typeof role === 'string')
      ) {
        return claim as string[];
      }

      return [];
    });

    return roles.some(
      (role) =>
        role.toLowerCase() === requiredRole.toLowerCase()
    );
  }
}