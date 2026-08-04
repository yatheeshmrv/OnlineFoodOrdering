import {
  CurrencyPipe,
  DatePipe
} from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal
} from '@angular/core';
import {
  ActivatedRoute,
  RouterLink
} from '@angular/router';

import {
  Order,
  PaymentMethod,
  PaymentStatus
} from '../../Core/models/order.model';
import { OrderService } from '../../Core/services/order.service';

@Component({
  selector: 'app-order-confirmation',
  imports: [
    CurrencyPipe,
    DatePipe,
    RouterLink
  ],
  templateUrl: './order-confirmation.component.html',
  styleUrl: './order-confirmation.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrderConfirmation implements OnInit {
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly orderService = inject(OrderService);

  readonly order = signal<Order | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');

  readonly totalItemQuantity = computed(() =>
    this.order()?.items.reduce(
      (total, item) => total + item.quantity,
      0
    ) ?? 0
  );

  ngOnInit(): void {
    this.loadOrder();
  }

  loadOrder(): void {
    const orderIdParameter =
      this.activatedRoute.snapshot.paramMap.get('orderId');
    const orderId = Number(orderIdParameter);

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

  formatPaymentMethod(paymentMethod: PaymentMethod): string {
    switch (paymentMethod) {
      case 'CashOnDelivery':
        return 'Cash on Delivery';
    }
  }

  formatPaymentStatus(paymentStatus: PaymentStatus): string {
    switch (paymentStatus) {
      case 'Pending':
        return 'Pending';
      case 'Paid':
        return 'Paid';
      case 'Failed':
        return 'Failed';
      case 'Refunded':
        return 'Refunded';
    }
  }

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

    return 'Unable to load the order confirmation. Please try again.';
  }
}
