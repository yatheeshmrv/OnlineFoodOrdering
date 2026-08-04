// Credentials sent to POST /api/Auth/login.
export interface LoginRequest {
  email: string;
  password: string;
}

// Successful response returned by the login endpoint.
export interface LoginResponse {
  message: string;
  token: string;
  tokenType: string;
}

// Customer information sent to POST /api/Auth/register.
export interface RegisterRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
  confirmPassword: string;
}

// Successful response returned by the registration endpoint.
export interface RegisterResponse {
  message: string;
  role: string;
}