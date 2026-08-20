import {
  ComponentFixture,
  TestBed
} from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { of } from 'rxjs';

import {
  AuthService
} from '../../../Core/services/auth.service';

import { Register } from './register.component';

describe('Register', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;

  const authServiceMock = {
    register: () =>
      of({
        message: 'Registration successful.'
      })
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: authServiceMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;

    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});