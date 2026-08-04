import {
  CurrencyPipe,
  DatePipe
} from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  computed,
  inject,
  OnInit,
  signal
} from '@angular/core';
import { RouterLink } from '@angular/router';

import {
  Order,
  UpdateOrderStatusRequest
} from '../../../Core/models/order.model';
import { OrderService } from '../../../Core/services/order.service';

/**
 * Represents every order status accepted by the backend
 * UpdateOrderStatusDtoValidator.
 */
type OrderStatus =
  | 'Pending'
  | 'Confirmed'
  | 'Preparing'
  | 'Out for Delivery'
  | 'Cancelled'
  | 'Delivered';

@Component({
  selector: 'app-admin-orders',

  // CurrencyPipe formats order totals and item prices.
  // DatePipe formats the ISO date returned by the API.
  // RouterLink provides navigation to the Admin Dashboard.
  imports: [
    CurrencyPipe,
    DatePipe,
    RouterLink
  ],

  templateUrl: './admin-orders.component.html',
  styleUrl: './admin-orders.component.css'
})
export class AdminOrders implements OnInit {
  private readonly orderService =
    inject(OrderService);

  /**
   * These values must remain aligned with the backend
   * UpdateOrderStatusDtoValidator.
   */
  protected readonly orderStatuses:
    readonly OrderStatus[] = [
      'Pending',
      'Confirmed',
      'Preparing',
      'Out for Delivery',
      'Cancelled',
      'Delivered'
    ];

  // Holds every customer order returned by the API.
  protected readonly orders =
    signal<Order[]>([]);

  // Controls the initial loading state.
  protected readonly isLoading = signal(true);

  // Holds errors encountered while loading orders.
  protected readonly loadErrorMessage = signal('');

  // Stores the current order-search text.
  protected readonly searchTerm = signal('');

  // Stores the selected status filter.
  // An empty string displays every status.
  protected readonly statusFilter =
    signal<OrderStatus | ''>('');

  // Stores the order whose item details are expanded.
  protected readonly expandedOrderId =
    signal<number | null>(null);

  // Stores the order whose status editor is open.
  protected readonly editingOrderId =
    signal<number | null>(null);

  // Holds the new status selected by the Admin.
  protected readonly selectedOrderStatus =
    signal<OrderStatus | ''>('');

  // Stores the order whose status is being updated.
  protected readonly updatingOrderId =
    signal<number | null>(null);

  // Holds feedback for status updates.
  protected readonly updateSuccessMessage = signal('');
  protected readonly updateErrorMessage = signal('');

  // Holds backend validation errors from a status update.
  protected readonly updateValidationErrors =
    signal<string[]>([]);

  /**
   * Filters orders using the search text and status filter.
   *
   * computed() behaves similarly to a calculated read-only
   * property in C#. It recalculates when its signals change.
   */
  protected readonly filteredOrders = computed(() => {
    const normalizedSearchTerm =
      this.searchTerm().trim().toLowerCase();

    const selectedStatus =
      this.statusFilter();

    return this.orders().filter((order) => {
      const matchesStatus =
        !selectedStatus ||
        order.orderStatus.toLowerCase() ===
          selectedStatus.toLowerCase();

      if (!matchesStatus) {
        return false;
      }

      if (!normalizedSearchTerm) {
        return true;
      }

      const searchableValues = [
        order.id.toString(),
        order.customerName,
        order.customerPhone ?? '',
        order.orderStatus,
        order.deliveryRecipientName ?? '',
        order.deliveryPhone ?? '',
        order.deliveryAddressLine1 ?? '',
        order.deliveryAddressLine2 ?? '',
        order.deliveryLandmark ?? '',
        order.deliveryCity ?? '',
        order.deliveryState ?? '',
        order.deliveryPostalCode ?? ''
      ];

      return searchableValues.some((value) =>
        value.toLowerCase().includes(
          normalizedSearchTerm
        )
      );
    });
  });

  // Summary values displayed above the order table.
  protected readonly totalOrderCount = computed(
    () => this.orders().length
  );

  protected readonly pendingOrderCount = computed(
    () =>
      this.orders().filter(
        (order) =>
          order.orderStatus.toLowerCase() === 'pending'
      ).length
  );

  protected readonly activeOrderCount = computed(
    () =>
      this.orders().filter((order) => {
        const normalizedStatus =
          order.orderStatus.toLowerCase();

        return [
          'confirmed',
          'preparing',
          'out for delivery'
        ].includes(normalizedStatus);
      }).length
  );

  protected readonly deliveredOrderCount = computed(
    () =>
      this.orders().filter(
        (order) =>
          order.orderStatus.toLowerCase() === 'delivered'
      ).length
  );

  /**
   * Angular calls ngOnInit after creating the component.
   */
  ngOnInit(): void {
    this.loadOrders();
  }

  /**
   * Retrieves all customer orders and sorts them newest first.
   */
  protected loadOrders(): void {
    this.isLoading.set(true);
    this.loadErrorMessage.set('');

    this.orderService
      .getAllOrders()
      .subscribe({
        next: (orders) => {
          this.orders.set(
            this.sortOrdersByNewest(orders)
          );

          this.isLoading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.loadErrorMessage.set(
            this.getApiErrorMessage(
              error,
              'Unable to load customer orders. Please try again.'
            )
          );

          this.isLoading.set(false);
        }
      });
  }

  /**
   * Updates the search signal from the search input.
   */
  protected updateSearchTerm(event: Event): void {
    const input =
      event.target as HTMLInputElement;

    this.searchTerm.set(input.value);
  }

  /**
   * Updates the selected status filter.
   */
  protected updateStatusFilter(event: Event): void {
    const select =
      event.target as HTMLSelectElement;

    const selectedStatus = select.value;

    if (!selectedStatus) {
      this.statusFilter.set('');
      return;
    }

    if (this.isAcceptedOrderStatus(selectedStatus)) {
      this.statusFilter.set(selectedStatus);
    }
  }

  /**
   * Clears the search and status filters.
   */
  protected clearFilters(): void {
    this.searchTerm.set('');
    this.statusFilter.set('');
  }

  /**
   * Opens or closes an order's item details.
   */
  protected toggleOrderDetails(orderId: number): void {
    this.expandedOrderId.update((currentOrderId) =>
      currentOrderId === orderId
        ? null
        : orderId
    );
  }

  /**
   * Opens the status editor using the order's current status.
   */
  protected startEditingOrderStatus(
    order: Order
  ): void {
    if (this.isMutationInProgress()) {
      return;
    }

    this.clearUpdateMessages();
    this.editingOrderId.set(order.id);

    if (
      this.isAcceptedOrderStatus(order.orderStatus)
    ) {
      this.selectedOrderStatus.set(
        order.orderStatus
      );
    } else {
      this.selectedOrderStatus.set('');
    }
  }

  /**
   * Updates the status selected in the inline editor.
   */
  protected updateSelectedOrderStatus(
    event: Event
  ): void {
    const select =
      event.target as HTMLSelectElement;

    const selectedStatus = select.value;

    if (this.isAcceptedOrderStatus(selectedStatus)) {
      this.selectedOrderStatus.set(selectedStatus);
    } else {
      this.selectedOrderStatus.set('');
    }
  }

  /**
   * Closes the status editor without saving changes.
   */
  protected cancelEditingOrderStatus(): void {
    if (this.isMutationInProgress()) {
      return;
    }

    this.editingOrderId.set(null);
    this.selectedOrderStatus.set('');
    this.clearUpdateMessages();
  }

  /**
   * Validates and submits the selected order status.
   */
  protected submitOrderStatus(
    order: Order
  ): void {
    if (
      this.isMutationInProgress() ||
      this.editingOrderId() !== order.id
    ) {
      return;
    }

    this.clearUpdateMessages();

    const selectedStatus =
      this.selectedOrderStatus();

    if (!selectedStatus) {
      this.updateErrorMessage.set(
        'Please select an order status.'
      );

      return;
    }

    const request: UpdateOrderStatusRequest = {
      orderStatus: selectedStatus
    };

    this.updatingOrderId.set(order.id);

    this.orderService
      .updateOrderStatus(order.id, request)
      .subscribe({
        next: (updatedOrder) => {
          this.orders.update((orders) =>
            this.sortOrdersByNewest(
              orders.map((existingOrder) =>
                existingOrder.id === updatedOrder.id
                  ? updatedOrder
                  : existingOrder
              )
            )
          );

          this.updateSuccessMessage.set(
            `Order #${updatedOrder.id} was updated to ` +
              `${updatedOrder.orderStatus}.`
          );

          this.editingOrderId.set(null);
          this.selectedOrderStatus.set('');
          this.updatingOrderId.set(null);
        },
        error: (error: HttpErrorResponse) => {
          this.updateValidationErrors.set(
            this.extractValidationErrors(error)
          );

          this.updateErrorMessage.set(
            this.getApiErrorMessage(
              error,
              'Unable to update the order status. Please try again.'
            )
          );

          this.updatingOrderId.set(null);
        }
      });
  }

  /**
   * Returns true while a status-update request is running.
   */
  protected isMutationInProgress(): boolean {
    return this.updatingOrderId() !== null;
  }

  /**
   * Returns a CSS class corresponding to an order status.
   */
  protected getOrderStatusClass(
    orderStatus: string
  ): string {
    switch (orderStatus.toLowerCase()) {
      case 'pending':
        return 'status-pending';

      case 'confirmed':
        return 'status-confirmed';

      case 'preparing':
        return 'status-preparing';

      case 'out for delivery':
        return 'status-out-for-delivery';

      case 'cancelled':
        return 'status-cancelled';

      case 'delivered':
        return 'status-delivered';

      default:
        return 'status-unknown';
    }
  }

  /**
   * Checks whether a string is accepted by the backend
   * order-status validator.
   */
  private isAcceptedOrderStatus(
    status: string
  ): status is OrderStatus {
    return this.orderStatuses.includes(
      status as OrderStatus
    );
  }

  /**
   * Sorts orders using their ISO order dates.
   */
  private sortOrdersByNewest(
    orders: Order[]
  ): Order[] {
    return [...orders].sort(
      (firstOrder, secondOrder) =>
        new Date(secondOrder.orderDate).getTime() -
        new Date(firstOrder.orderDate).getTime()
    );
  }

  /**
   * Clears feedback related to status updates.
   */
  private clearUpdateMessages(): void {
    this.updateSuccessMessage.set('');
    this.updateErrorMessage.set('');
    this.updateValidationErrors.set([]);
  }

  /**
   * Returns an API-provided message when available.
   */
  private getApiErrorMessage(
    error: HttpErrorResponse,
    fallbackMessage: string
  ): string {
    const apiMessage: unknown =
      error.error?.message;

    if (
      typeof apiMessage === 'string' &&
      apiMessage.trim().length > 0
    ) {
      return apiMessage;
    }

    const validationTitle: unknown =
      error.error?.title;

    return typeof validationTitle === 'string' &&
      validationTitle.trim().length > 0
      ? validationTitle
      : fallbackMessage;
  }

  /**
   * Converts backend validation errors into an array.
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