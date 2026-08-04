import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
  signal
} from '@angular/core';
import {
  CurrencyPipe,
  DatePipe
} from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

import { Order } from '../../Core/models/order.model';
import { OrderService } from '../../Core/services/order.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-my-orders',

  // Pipes are imported because standalone components
  // explicitly declare everything used by their template.
  imports: [
  CurrencyPipe,
  DatePipe,
  RouterLink
  ],

  templateUrl: './my-orders.component.html',
  styleUrl: './my-orders.component.css',

  // Angular checks this component when its signals change.
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MyOrders implements OnInit {
  private readonly orderService = inject(OrderService);

  // Contains all orders belonging to the logged-in customer.
  readonly orders = signal<Order[]>([]);

  // Controls the loading message or loading animation.
  readonly isLoading = signal(true);

  // Contains a user-friendly API error message.
  readonly errorMessage = signal('');

  // Similar to running initialization logic when a C# class
  // or page is first loaded.
  ngOnInit(): void {
    this.loadOrders();
  }

  // Retrieves the logged-in customer's order history.
  loadOrders(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.orderService.getMyOrders().subscribe({
      next: (orders) => {
        // Display the newest orders first.
        const sortedOrders = [...orders].sort(
          (firstOrder, secondOrder) =>
            new Date(secondOrder.orderDate).getTime() -
            new Date(firstOrder.orderDate).getTime()
        );

        this.orders.set(sortedOrders);
        this.isLoading.set(false);
      },

      error: (error: HttpErrorResponse) => {
        this.orders.set([]);
        this.errorMessage.set(this.getErrorMessage(error));
        this.isLoading.set(false);
      }
    });
  }

  // Used by @for to identify each order efficiently.
  trackOrderById(
    _index: number,
    order: Order
  ): number {
    return order.id;
  }

  // Converts backend errors into a clear message for the customer.
  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to connect to the server. Please try again later.';
    }

    if (error.status === 401) {
      return 'Your session has expired. Please log in again.';
    }

    if (
      error.error &&
      typeof error.error === 'object' &&
      typeof error.error.message === 'string'
    ) {
      return error.error.message;
    }

    return 'Unable to load your orders. Please try again.';
  }
}