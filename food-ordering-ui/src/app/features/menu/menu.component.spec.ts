import { signal } from '@angular/core';
import {
  ComponentFixture,
  TestBed
} from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { of } from 'rxjs';

import {
  PagedFoodItemResponse
} from '../../Core/models/food-item.model';

import { AuthService } from '../../Core/services/auth.service';
import { CartService } from '../../Core/services/cart.service';
import {
  FoodItemService
} from '../../Core/services/food-item.service';

import { Menu } from './menu.component';

describe('Menu', () => {
  let component: Menu;
  let fixture: ComponentFixture<Menu>;

  const emptyFoodItemResponse:
    PagedFoodItemResponse = {
      items: [],
      pageNumber: 1,
      pageSize: 20,
      totalCount: 0,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false
    };

  const foodItemServiceMock = {
    getFoodItems: () =>
      of(emptyFoodItemResponse)
  };

  const authServiceMock = {
    isAuthenticated: signal(false)
  };

  const cartServiceMock = {
    addCartItem: () => of({})
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Menu],
      providers: [
        provideRouter([]),
        {
          provide: FoodItemService,
          useValue: foodItemServiceMock
        },
        {
          provide: AuthService,
          useValue: authServiceMock
        },
        {
          provide: CartService,
          useValue: cartServiceMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Menu);
    component = fixture.componentInstance;

    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});