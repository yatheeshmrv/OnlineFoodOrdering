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

import {
  CreateFoodCategoryRequest,
  FoodCategory,
  UpdateFoodCategoryRequest
} from '../../../Core/models/food-category.model';
import { FoodCategoryService } from '../../../Core/services/food-category.service';

@Component({
  selector: 'app-admin-food-categories',

  // ReactiveFormsModule provides formGroup and formControlName.
  // RouterLink provides navigation back to the Admin Dashboard.
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],

  templateUrl: './admin-food-categories.component.html',
  styleUrl: './admin-food-categories.component.css'
})
export class AdminFoodCategories implements OnInit {
  private readonly formBuilder = inject(FormBuilder);

  private readonly foodCategoryService =
    inject(FoodCategoryService);

  // Holds all categories returned by the API.
  protected readonly foodCategories =
    signal<FoodCategory[]>([]);

  // Controls the initial inventory-loading state.
  protected readonly isLoading = signal(true);

  // Holds errors encountered while loading the categories.
  protected readonly loadErrorMessage = signal('');

  // Controls the create request state.
  protected readonly isCreating = signal(false);

  // Stores the category currently being edited.
  // null means that no edit form is open.
  protected readonly editingFoodCategoryId =
    signal<number | null>(null);

  // Controls the update request state.
  protected readonly isUpdating = signal(false);

  // Stores the category currently being deleted.
  // null means that no delete request is running.
  protected readonly deletingFoodCategoryId =
    signal<number | null>(null);

  // Create-operation feedback.
  protected readonly createSuccessMessage = signal('');
  protected readonly createErrorMessage = signal('');
  protected readonly createValidationErrors =
    signal<string[]>([]);

  // Update-operation feedback.
  protected readonly editSuccessMessage = signal('');
  protected readonly editErrorMessage = signal('');
  protected readonly editValidationErrors =
    signal<string[]>([]);

  // Delete-operation feedback.
  protected readonly deleteSuccessMessage = signal('');
  protected readonly deleteErrorMessage = signal('');

  /**
   * Reactive form used to create a food category.
   */
  protected readonly createFoodCategoryForm =
    this.formBuilder.nonNullable.group({
      categoryName: [
        '',
        [
          Validators.required,
          Validators.maxLength(100)
        ]
      ],

      isActive: [true]
    });

  /**
   * Reactive form used to update an existing category.
   */
  protected readonly editFoodCategoryForm =
    this.formBuilder.nonNullable.group({
      categoryName: [
        '',
        [
          Validators.required,
          Validators.maxLength(100)
        ]
      ],

      isActive: [true]
    });

  /**
   * Angular calls ngOnInit after creating the component.
   */
  ngOnInit(): void {
    this.loadFoodCategories();
  }

  /**
   * Retrieves all food categories from the API.
   */
  protected loadFoodCategories(): void {
    this.isLoading.set(true);
    this.loadErrorMessage.set('');

    this.foodCategoryService
      .getFoodCategories()
      .subscribe({
        next: (categories) => {
          const sortedCategories = [...categories]
            .sort((firstCategory, secondCategory) =>
              firstCategory.categoryName.localeCompare(
                secondCategory.categoryName
              )
            );

          this.foodCategories.set(sortedCategories);
          this.isLoading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.loadErrorMessage.set(
            this.getApiErrorMessage(
              error,
              'Unable to load food categories. Please try again.'
            )
          );

          this.isLoading.set(false);
        }
      });
  }

  /**
   * Validates and submits the Create Food Category form.
   */
  protected submitCreateFoodCategory(): void {
    if (
      this.isCreating() ||
      this.isUpdating() ||
      this.deletingFoodCategoryId() !== null
    ) {
      return;
    }

    this.clearCreateMessages();

    if (this.createFoodCategoryForm.invalid) {
      this.createFoodCategoryForm.markAllAsTouched();
      return;
    }

    const formValue =
      this.createFoodCategoryForm.getRawValue();

    const request: CreateFoodCategoryRequest = {
      categoryName: formValue.categoryName.trim(),
      isActive: formValue.isActive
    };

    // Prevents whitespace-only category names.
    if (!request.categoryName) {
      this.createFoodCategoryForm.controls.categoryName
        .setErrors({
          required: true
        });

      this.createFoodCategoryForm.controls.categoryName
        .markAsTouched();

      return;
    }

    this.isCreating.set(true);

    this.foodCategoryService
      .createFoodCategory(request)
      .subscribe({
        next: (createdCategory) => {
          this.createSuccessMessage.set(
            `${createdCategory.categoryName} was created successfully.`
          );

          this.createFoodCategoryForm.reset({
            categoryName: '',
            isActive: true
          });

          this.isCreating.set(false);
          this.loadFoodCategories();
        },
        error: (error: HttpErrorResponse) => {
          this.createValidationErrors.set(
            this.extractValidationErrors(error)
          );

          this.createErrorMessage.set(
            this.getApiErrorMessage(
              error,
              'Unable to create the food category. Please try again.'
            )
          );

          this.isCreating.set(false);
        }
      });
  }

  /**
   * Opens the inline edit form and copies the selected
   * category values into it.
   */
  protected startEditingFoodCategory(
    category: FoodCategory
  ): void {
    if (
      this.isUpdating() ||
      this.isCreating() ||
      this.deletingFoodCategoryId() !== null
    ) {
      return;
    }

    this.clearEditMessages();
    this.editingFoodCategoryId.set(category.id);

    this.editFoodCategoryForm.reset({
      categoryName: category.categoryName,
      isActive: category.isActive
    });
  }

  /**
   * Closes the inline edit form without saving changes.
   */
  protected cancelEditingFoodCategory(): void {
    if (this.isUpdating()) {
      return;
    }

    this.editingFoodCategoryId.set(null);
    this.clearEditMessages();
    this.resetEditFoodCategoryForm();
  }

  /**
   * Validates and submits changes for the selected category.
   */
  protected submitEditFoodCategory(): void {
    if (
      this.isUpdating() ||
      this.isCreating() ||
      this.deletingFoodCategoryId() !== null
    ) {
      return;
    }

    const foodCategoryId =
      this.editingFoodCategoryId();

    if (foodCategoryId === null) {
      return;
    }

    this.clearEditMessages();

    if (this.editFoodCategoryForm.invalid) {
      this.editFoodCategoryForm.markAllAsTouched();
      return;
    }

    const formValue =
      this.editFoodCategoryForm.getRawValue();

    const request: UpdateFoodCategoryRequest = {
      categoryName: formValue.categoryName.trim(),
      isActive: formValue.isActive
    };

    // Prevents whitespace-only category names.
    if (!request.categoryName) {
      this.editFoodCategoryForm.controls.categoryName
        .setErrors({
          required: true
        });

      this.editFoodCategoryForm.controls.categoryName
        .markAsTouched();

      return;
    }

    this.isUpdating.set(true);

    this.foodCategoryService
      .updateFoodCategory(foodCategoryId, request)
      .subscribe({
        next: (updatedCategory) => {
          this.editSuccessMessage.set(
            `${updatedCategory.categoryName} was updated successfully.`
          );

          this.editingFoodCategoryId.set(null);
          this.resetEditFoodCategoryForm();
          this.isUpdating.set(false);

          this.loadFoodCategories();
        },
        error: (error: HttpErrorResponse) => {
          this.editValidationErrors.set(
            this.extractValidationErrors(error)
          );

          this.editErrorMessage.set(
            this.getApiErrorMessage(
              error,
              'Unable to update the food category. Please try again.'
            )
          );

          this.isUpdating.set(false);
        }
      });
  }

  /**
   * Requests confirmation and deletes the selected category.
   */
  protected deleteFoodCategory(
    category: FoodCategory
  ): void {
    if (
      this.deletingFoodCategoryId() !== null ||
      this.isCreating() ||
      this.isUpdating()
    ) {
      return;
    }

    const confirmed = window.confirm(
      `Delete "${category.categoryName}"?\n\n` +
      'This action cannot be undone. Categories associated ' +
      'with food items may not be removable.'
    );

    if (!confirmed) {
      return;
    }

    this.deleteSuccessMessage.set('');
    this.deleteErrorMessage.set('');
    this.deletingFoodCategoryId.set(category.id);

    this.foodCategoryService
      .deleteFoodCategory(category.id)
      .subscribe({
        next: () => {
          this.deleteSuccessMessage.set(
            `${category.categoryName} was deleted successfully.`
          );

          if (
            this.editingFoodCategoryId() === category.id
          ) {
            this.editingFoodCategoryId.set(null);
            this.resetEditFoodCategoryForm();
          }

          this.deletingFoodCategoryId.set(null);
          this.loadFoodCategories();
        },
        error: (error: HttpErrorResponse) => {
          this.deleteErrorMessage.set(
            this.getApiErrorMessage(
              error,
              'Unable to delete the food category. It may still be used by one or more food items.'
            )
          );

          this.deletingFoodCategoryId.set(null);
        }
      });
  }

  /**
   * Returns true when any create, update or delete request
   * is currently running.
   */
  protected isMutationInProgress(): boolean {
    return (
      this.isCreating() ||
      this.isUpdating() ||
      this.deletingFoodCategoryId() !== null
    );
  }

  /**
   * Restores the edit form to its initial values.
   */
  private resetEditFoodCategoryForm(): void {
    this.editFoodCategoryForm.reset({
      categoryName: '',
      isActive: true
    });
  }

  /**
   * Clears feedback related to creating a category.
   */
  private clearCreateMessages(): void {
    this.createSuccessMessage.set('');
    this.createErrorMessage.set('');
    this.createValidationErrors.set([]);
  }

  /**
   * Clears feedback related to editing a category.
   */
  private clearEditMessages(): void {
    this.editSuccessMessage.set('');
    this.editErrorMessage.set('');
    this.editValidationErrors.set([]);
  }

  /**
   * Returns the API message when available, otherwise the
   * supplied fallback message.
   */
  private getApiErrorMessage(
    error: HttpErrorResponse,
    fallbackMessage: string
  ): string {
    const apiMessage: unknown =
      error.error?.message;

    return typeof apiMessage === 'string' &&
      apiMessage.trim().length > 0
      ? apiMessage
      : fallbackMessage;
  }

  /**
   * Converts the backend FluentValidation error dictionary
   * into a simple array that can be displayed by the template.
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