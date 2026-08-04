import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
  signal
} from '@angular/core';
import {
  ActivatedRoute,
  RouterLink
} from '@angular/router';

import { Order } from '../../Core/models/order.model';
import { OrderService } from '../../Core/services/order.service';

@Component({
  selector: 'app-order-details',

  // These imports will be used by the order-details template.
  imports: [
    CurrencyPipe,
    DatePipe,
    RouterLink
  ],

  templateUrl: './order-details.component.html',
  styleUrl: './order-details.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrderDetails implements OnInit {
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly orderService = inject(OrderService);

  // Contains the order returned by the API.
  readonly order = signal<Order | null>(null);

  // Controls the page's loading state.
  readonly isLoading = signal(true);

  // Contains validation or API error messages.
  readonly errorMessage = signal('');

  ngOnInit(): void {
    this.loadOrder();
  }

  // Reads the order ID from /my-orders/:orderId
  // and requests that specific customer order.
  loadOrder(): void {
    const orderIdParameter =
      this.activatedRoute.snapshot.paramMap.get('orderId');

    const orderId = Number(orderIdParameter);

    // Protects the API from invalid route values such as
    // /my-orders/abc or /my-orders/0.
    if (!Number.isInteger(orderId) || orderId <= 0) {
      this.order.set(null);
      this.isLoading.set(false);
      this.errorMessage.set('The order number is invalid.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.orderService.getMyOrderById(orderId).subscribe({
      next: (order) => {
        this.order.set(order);
        this.isLoading.set(false);
      },

      error: (error: HttpErrorResponse) => {
        this.order.set(null);
        this.errorMessage.set(this.getErrorMessage(error));
        this.isLoading.set(false);
      }
    });
  }

  // Converts backend failures into customer-friendly messages.
  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to connect to the server. Please try again later.';
    }

    if (error.status === 401) {
      return 'Your session has expired. Please log in again.';
    }

    if (error.status === 403 || error.status === 404) {
      return 'The requested order could not be found.';
    }

    if (
      error.error &&
      typeof error.error === 'object' &&
      typeof error.error.message === 'string'
    ) {
      return error.error.message;
    }

    return 'Unable to load the order details. Please try again.';
  }
}