import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
  signal
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { FoodCategoryService } from
  '../../../Core/services/food-category.service';
import { FoodItemService } from
  '../../../Core/services/food-item.service';
import { OrderService } from
  '../../../Core/services/order.service';

// Represents all statistics displayed on the Admin Dashboard.
interface AdminDashboardSummary {
  totalFoodItems: number;
  availableFoodItems: number;
  totalCategories: number;
  activeCategories: number;
  totalOrders: number;
  activeOrders: number;
  totalRevenue: number;
}

// Provides a safe initial value before API data is loaded.
const EMPTY_DASHBOARD_SUMMARY: AdminDashboardSummary = {
  totalFoodItems: 0,
  availableFoodItems: 0,
  totalCategories: 0,
  activeCategories: 0,
  totalOrders: 0,
  activeOrders: 0,
  totalRevenue: 0
};

@Component({
  selector: 'app-admin-dashboard',

  // Enables dashboard cards to navigate to Admin pages.
  imports: [
    RouterLink
  ],

  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css',

  // Signals notify Angular when dashboard state changes.
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminDashboard implements OnInit {
  private readonly foodItemService =
    inject(FoodItemService);

  private readonly foodCategoryService =
    inject(FoodCategoryService);

  private readonly orderService =
    inject(OrderService);

  // Contains the calculated dashboard statistics.
  readonly summary =
    signal<AdminDashboardSummary>(
      EMPTY_DASHBOARD_SUMMARY
    );

  // Controls the dashboard loading state.
  readonly isLoading = signal(false);

  // Contains an API error shown by the template.
  readonly errorMessage = signal('');

  // Runs after Angular creates the component.
  ngOnInit(): void {
    this.loadDashboardSummary();
  }

  // Loads all dashboard data using parallel API requests.
  loadDashboardSummary(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    /*
     * forkJoin runs these independent HTTP requests together.
     * It is similar to Task.WhenAll() in C#.
     */
    forkJoin({
      foodItemsResponse:
        this.foodItemService.getFoodItems(1, 100),

      categories:
        this.foodCategoryService.getFoodCategories(),

      orders:
        this.orderService.getAllOrders()
    }).subscribe({
      next: ({
        foodItemsResponse,
        categories,
        orders
      }) => {
        // Active orders are currently being processed.
        const activeStatuses = new Set([
          'confirmed',
          'preparing',
          'out for delivery'
        ]);

        const activeOrders = orders.filter(order =>
          activeStatuses.has(
            order.orderStatus.trim().toLowerCase()
          )
        ).length;

        /*
         * Revenue counts only successfully delivered orders.
         * Pending, active and cancelled orders are excluded.
         */
        const totalRevenue = orders
          .filter(
            order =>
              order.orderStatus
                .trim()
                .toLowerCase() === 'delivered'
          )
          .reduce(
            (total, order) =>
              total + order.totalAmount,
            0
          );

        this.summary.set({
          // totalCount comes from the paginated API response.
          totalFoodItems:
            foodItemsResponse.totalCount,

          availableFoodItems:
            foodItemsResponse.items.filter(
              foodItem => foodItem.isAvailable
            ).length,

          totalCategories:
            categories.length,

          activeCategories:
            categories.filter(
              category => category.isActive
            ).length,

          totalOrders:
            orders.length,

          activeOrders,

          totalRevenue
        });

        this.isLoading.set(false);
      },

      error: (error: unknown) => {
        this.isLoading.set(false);

        this.errorMessage.set(
          this.getErrorMessage(error)
        );
      }
    });
  }

  // Converts different API failures into one readable message.
  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const backendMessage =
        error.error?.message;

      if (
        typeof backendMessage === 'string' &&
        backendMessage.trim().length > 0
      ) {
        return backendMessage;
      }
    }

    return (
      'The dashboard statistics could not be loaded. ' +
      'Please try again.'
    );
  }
}