import {
  Component,
  computed,
  inject,
  OnInit,
  signal
} from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

import { finalize } from 'rxjs';

import { FoodItem } from '../../Core/models/food-item.model';
import { FoodItemService } from '../../Core/services/food-item.service';
import { AuthService } from '../../Core/services/auth.service';
import { CartService } from '../../Core/services/cart.service';

/**
 * Represents one category displayed in the Menu category filter.
 */
interface MenuCategory {
  id: number;
  name: string;
  itemCount: number;
}

@Component({
  selector: 'app-menu',

  // Makes the currency pipe available in this component's HTML.
  imports: [CurrencyPipe],

  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})
export class Menu implements OnInit {
  // Resolves FoodItemService through Angular dependency injection.
  private readonly foodItemService = inject(FoodItemService);

  // Handles customer authentication state.
  private readonly authService = inject(AuthService);

  // Sends cart requests to the backend.
  private readonly cartService = inject(CartService);

  // Performs programmatic navigation.
  private readonly router = inject(Router);

  // Stores all food items received from the API.
  readonly foodItems = signal<FoodItem[]>([]);

  /**
   * Stores the selected category.
   *
   * It remains null only while the menu is loading or when the
   * menu contains no categories.
   */
  readonly selectedCategoryId = signal<number | null>(null);

  // Controls the loading state.
  readonly isLoading = signal<boolean>(true);

  // Stores an API error message.
  readonly errorMessage = signal<string>('');

  // Stores the food item currently being added.
  readonly addingFoodItemId = signal<number | null>(null);

  // Displays successful cart feedback.
  readonly cartMessage = signal('');

  // Displays an Add to Cart error.
  readonly cartErrorMessage = signal('');

  /**
   * Creates a unique, alphabetically sorted category list from
   * the loaded food items.
   */
  readonly foodCategories = computed<MenuCategory[]>(() => {
    const categories = new Map<number, MenuCategory>();

    for (const foodItem of this.foodItems()) {
      const existingCategory = categories.get(
        foodItem.foodCategoryId
      );

      if (existingCategory) {
        existingCategory.itemCount += 1;
        continue;
      }

      categories.set(foodItem.foodCategoryId, {
        id: foodItem.foodCategoryId,
        name: foodItem.foodCategoryName,
        itemCount: 1
      });
    }

    return Array.from(categories.values()).sort(
      (firstCategory, secondCategory) =>
        firstCategory.name.localeCompare(secondCategory.name)
    );
  });

  /**
   * Returns only the food items belonging to the selected
   * category.
   */
  readonly filteredFoodItems = computed<FoodItem[]>(() => {
    const selectedCategoryId = this.selectedCategoryId();

    if (selectedCategoryId === null) {
      return [];
    }

    return this.foodItems().filter(
      foodItem =>
        foodItem.foodCategoryId === selectedCategoryId
    );
  });

  /**
   * Provides the selected category name for the results heading.
   */
  readonly selectedCategoryName = computed<string>(() => {
    const selectedCategoryId = this.selectedCategoryId();

    if (selectedCategoryId === null) {
      return 'Menu';
    }

    return (
      this.foodCategories().find(
        category => category.id === selectedCategoryId
      )?.name ?? 'Selected Category'
    );
  });

  // Runs automatically when Angular creates this component.
  ngOnInit(): void {
    this.loadFoodItems();
  }

  /**
   * Selects a category and displays only its food items.
   */
  selectCategory(categoryId: number): void {
    this.selectedCategoryId.set(categoryId);

    // Remove previous cart feedback when navigating categories.
    this.cartMessage.set('');
    this.cartErrorMessage.set('');
  }

  /**
   * Adds one quantity of the selected food item to the cart.
   *
   * Logged-out visitors are redirected to Login.
   * Logged-in customers send the item to the Cart API.
   */
  addToCart(foodItem: FoodItem): void {
    if (!foodItem.isAvailable) {
      return;
    }

    // Only authenticated customers can maintain a server-side cart.
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login'], {
        queryParams: {
          returnUrl: '/menu'
        }
      });

      return;
    }

    // Prevent repeated requests while another item is being added.
    if (this.addingFoodItemId() !== null) {
      return;
    }

    this.addingFoodItemId.set(foodItem.id);
    this.cartMessage.set('');
    this.cartErrorMessage.set('');

    this.cartService
      .addCartItem({
        foodItemId: foodItem.id,
        quantity: 1
      })
      .pipe(
        finalize(() => {
          this.addingFoodItemId.set(null);
        })
      )
      .subscribe({
        next: () => {
          this.cartMessage.set(
            `${foodItem.name} was added to your cart.`
          );
        },
        error: (error: HttpErrorResponse) => {
          const backendMessage = error.error?.message;

          this.cartErrorMessage.set(
            typeof backendMessage === 'string'
              ? backendMessage
              : 'Unable to add this item to your cart.'
          );
        }
      });
  }

  // Requests food items from the FoodOrderAPI.
  private loadFoodItems(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.foodItemService.getFoodItems().subscribe({
      next: response => {
        this.foodItems.set(response.items);

        /**
         * Select the first category automatically so the Menu
         * never opens with every food item displayed together.
         */
        const firstCategory = this.foodCategories()[0];

        this.selectedCategoryId.set(
          firstCategory?.id ?? null
        );

        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        console.error('Unable to load food items.', error);

        this.errorMessage.set(
          'The food menu could not be loaded. Please try again.'
        );

        this.selectedCategoryId.set(null);
        this.isLoading.set(false);
      }
    });
  }
}