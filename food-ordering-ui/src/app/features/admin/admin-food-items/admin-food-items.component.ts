import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  inject,
  OnInit,
  signal
} from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { RouterLink } from '@angular/router';

import { FoodCategory } from '../../../Core/models/food-category.model';
import {
  CreateFoodItemRequest,
  FoodItem,
  UpdateFoodItemRequest
} from '../../../Core/models/food-item.model';
import { FoodCategoryService } from '../../../Core/services/food-category.service';
import { FoodItemService } from '../../../Core/services/food-item.service';

@Component({
  selector: 'app-admin-food-items',

  // CurrencyPipe formats prices.
  // ReactiveFormsModule provides reactive-form directives.
  // RouterLink provides navigation back to the Admin Dashboard.
  imports: [
    CurrencyPipe,
    ReactiveFormsModule,
    RouterLink
  ],

  templateUrl: './admin-food-items.component.html',
  styleUrl: './admin-food-items.component.css'
})
export class AdminFoodItems implements OnInit {
  private readonly formBuilder = inject(FormBuilder);

  private readonly foodItemService =
    inject(FoodItemService);

  private readonly foodCategoryService =
    inject(FoodCategoryService);

  // Holds the food items returned by the API.
  protected readonly foodItems =
    signal<FoodItem[]>([]);

  // Holds active categories used by the create and edit forms.
  protected readonly activeFoodCategories =
    signal<FoodCategory[]>([]);

  // Controls inventory-loading feedback.
  protected readonly isLoading = signal(true);

  // Controls category-loading feedback.
  protected readonly areCategoriesLoading = signal(true);

  // Indicates whether a create request is in progress.
  protected readonly isCreating = signal(false);

  // Indicates whether an update request is in progress.
  protected readonly isUpdating = signal(false);

  // Stores the ID of the item currently being deleted.
  // null means that no delete request is in progress.
  protected readonly deletingFoodItemId =
    signal<number | null>(null);

  // Stores the ID of the item currently being edited.
  // null means that no edit form is open.
  protected readonly editingFoodItemId =
    signal<number | null>(null);

  // Holds inventory-loading errors.
  protected readonly errorMessage = signal('');

  // Holds category-loading errors.
  protected readonly categoryErrorMessage = signal('');

  // Holds feedback for the create operation.
  protected readonly createSuccessMessage = signal('');
  protected readonly createErrorMessage = signal('');

  // Holds backend validation messages from a create request.
  protected readonly createValidationErrors =
    signal<string[]>([]);

  // Holds feedback for the update operation.
  protected readonly editSuccessMessage = signal('');
  protected readonly editErrorMessage = signal('');

  // Holds backend validation messages from an update request.
  protected readonly editValidationErrors =
    signal<string[]>([]);

  // Holds feedback for the delete operation.
  protected readonly deleteSuccessMessage = signal('');
  protected readonly deleteErrorMessage = signal('');

  /**
   * Reactive form used to create a food item.
   */
  protected readonly createFoodItemForm =
    this.formBuilder.nonNullable.group({
      name: [
        '',
        [
          Validators.required,
          Validators.maxLength(100)
        ]
      ],

      description: [
        '',
        [
          Validators.required,
          Validators.maxLength(250)
        ]
      ],

      price: [
        0,
        [
          Validators.required,
          Validators.min(1),
          Validators.max(10000)
        ]
      ],

      foodCategoryId: [
        0,
        [
          Validators.required,
          Validators.min(1)
        ]
      ],

      isAvailable: [true]
    });

  /**
   * Reactive form used to update an existing food item.
   */
  protected readonly editFoodItemForm =
    this.formBuilder.nonNullable.group({
      name: [
        '',
        [
          Validators.required,
          Validators.maxLength(100)
        ]
      ],

      description: [
        '',
        [
          Validators.required,
          Validators.maxLength(250)
        ]
      ],

      price: [
        0,
        [
          Validators.required,
          Validators.min(1),
          Validators.max(10000)
        ]
      ],

      foodCategoryId: [
        0,
        [
          Validators.required,
          Validators.min(1)
        ]
      ],

      isAvailable: [true]
    });

  /**
   * Angular calls ngOnInit after creating the component.
   */
  ngOnInit(): void {
    this.loadFoodItems();
    this.loadActiveFoodCategories();
  }

  /**
   * Retrieves the complete food-item inventory.
   */
  protected loadFoodItems(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.foodItemService
      .getFoodItems(1, 100)
      .subscribe({
        next: (response) => {
          this.foodItems.set(response.items);
          this.isLoading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            error.error?.message ??
              'Unable to load food items. Please try again.'
          );

          this.isLoading.set(false);
        }
      });
  }

  /**
   * Retrieves food categories and keeps only active categories.
   */
  protected loadActiveFoodCategories(): void {
    this.areCategoriesLoading.set(true);
    this.categoryErrorMessage.set('');

    this.foodCategoryService
      .getFoodCategories()
      .subscribe({
        next: (categories) => {
          const activeCategories = categories
            .filter((category) => category.isActive)
            .sort((firstCategory, secondCategory) =>
              firstCategory.categoryName.localeCompare(
                secondCategory.categoryName
              )
            );

          this.activeFoodCategories.set(activeCategories);
          this.areCategoriesLoading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.categoryErrorMessage.set(
            error.error?.message ??
              'Unable to load food categories.'
          );

          this.areCategoriesLoading.set(false);
        }
      });
  }

  /**
   * Validates and submits the Create Food Item form.
   */
  protected submitCreateFoodItem(): void {
    if (this.isCreating()) {
      return;
    }

    this.createSuccessMessage.set('');
    this.createErrorMessage.set('');
    this.createValidationErrors.set([]);

    if (this.createFoodItemForm.invalid) {
      this.createFoodItemForm.markAllAsTouched();
      return;
    }

    const request: CreateFoodItemRequest =
      this.createFoodItemForm.getRawValue();

    this.isCreating.set(true);

    this.foodItemService
      .createFoodItem(request)
      .subscribe({
        next: (createdFoodItem) => {
          this.createSuccessMessage.set(
            `${createdFoodItem.name} was created successfully.`
          );

          this.createFoodItemForm.reset({
            name: '',
            description: '',
            price: 0,
            foodCategoryId: 0,
            isAvailable: true
          });

          this.isCreating.set(false);
          this.loadFoodItems();
        },
        error: (error: HttpErrorResponse) => {
          this.createValidationErrors.set(
            this.extractValidationErrors(error)
          );

          this.createErrorMessage.set(
            error.error?.message ??
              'Unable to create the food item. Please try again.'
          );

          this.isCreating.set(false);
        }
      });
  }

  /**
   * Opens the edit form and copies the selected item's
   * current values into the controls.
   */
  protected startEditingFoodItem(item: FoodItem): void {
    if (
      this.isUpdating() ||
      this.deletingFoodItemId() !== null
    ) {
      return;
    }

    this.editSuccessMessage.set('');
    this.editErrorMessage.set('');
    this.editValidationErrors.set([]);

    this.editingFoodItemId.set(item.id);

    this.editFoodItemForm.reset({
      name: item.name,
      description: item.description,
      price: item.price,
      foodCategoryId: item.foodCategoryId,
      isAvailable: item.isAvailable
    });
  }

  /**
   * Closes the edit form without saving changes.
   */
  protected cancelEditingFoodItem(): void {
    if (this.isUpdating()) {
      return;
    }

    this.editingFoodItemId.set(null);
    this.editSuccessMessage.set('');
    this.editErrorMessage.set('');
    this.editValidationErrors.set([]);

    this.resetEditFoodItemForm();
  }

  /**
   * Validates and submits changes for the selected food item.
   */
  protected submitEditFoodItem(): void {
    if (this.isUpdating()) {
      return;
    }

    const foodItemId = this.editingFoodItemId();

    if (foodItemId === null) {
      return;
    }

    this.editSuccessMessage.set('');
    this.editErrorMessage.set('');
    this.editValidationErrors.set([]);

    if (this.editFoodItemForm.invalid) {
      this.editFoodItemForm.markAllAsTouched();
      return;
    }

    const request: UpdateFoodItemRequest =
      this.editFoodItemForm.getRawValue();

    this.isUpdating.set(true);

    this.foodItemService
      .updateFoodItem(foodItemId, request)
      .subscribe({
        next: (updatedFoodItem) => {
          this.editSuccessMessage.set(
            `${updatedFoodItem.name} was updated successfully.`
          );

          this.editingFoodItemId.set(null);
          this.resetEditFoodItemForm();
          this.isUpdating.set(false);

          this.loadFoodItems();
        },
        error: (error: HttpErrorResponse) => {
          this.editValidationErrors.set(
            this.extractValidationErrors(error)
          );

          this.editErrorMessage.set(
            error.error?.message ??
              'Unable to update the food item. Please try again.'
          );

          this.isUpdating.set(false);
        }
      });
  }

  /**
   * Requests confirmation and deletes the selected food item.
   */
  protected deleteFoodItem(item: FoodItem): void {
    if (
      this.deletingFoodItemId() !== null ||
      this.isUpdating()
    ) {
      return;
    }

    const confirmed = window.confirm(
      `Delete "${item.name}"?\n\n` +
      'This action cannot be undone.'
    );

    if (!confirmed) {
      return;
    }

    this.deleteSuccessMessage.set('');
    this.deleteErrorMessage.set('');
    this.deletingFoodItemId.set(item.id);

    this.foodItemService
      .deleteFoodItem(item.id)
      .subscribe({
        next: () => {
          this.deleteSuccessMessage.set(
            `${item.name} was deleted successfully.`
          );

          if (this.editingFoodItemId() === item.id) {
            this.editingFoodItemId.set(null);
            this.resetEditFoodItemForm();
          }

          this.deletingFoodItemId.set(null);
          this.loadFoodItems();
        },
        error: (error: HttpErrorResponse) => {
          this.deleteErrorMessage.set(
            error.error?.message ??
              'Unable to delete the food item. Please try again.'
          );

          this.deletingFoodItemId.set(null);
        }
      });
  }

  /**
   * Restores the edit form to its initial values.
   */
  private resetEditFoodItemForm(): void {
    this.editFoodItemForm.reset({
      name: '',
      description: '',
      price: 0,
      foodCategoryId: 0,
      isAvailable: true
    });
  }

  /**
   * Extracts FluentValidation messages from the API response.
   */
  private extractValidationErrors(
    error: HttpErrorResponse
  ): string[] {
    const validationErrors: unknown =
      error.error?.errors;

    if (
      !validationErrors ||
      typeof validationErrors !== 'object' ||
      Array.isArray(validationErrors)
    ) {
      return [];
    }

    return Object.values(validationErrors)
      .flatMap((messages) => {
        if (Array.isArray(messages)) {
          return messages.filter(
            (message): message is string =>
              typeof message === 'string'
          );
        }

        return typeof messages === 'string'
          ? [messages]
          : [];
      });
  }
}