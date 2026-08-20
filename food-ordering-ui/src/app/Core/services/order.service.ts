import { HttpClient } from '@angular/common/http';
import {
  inject,
  Injectable
} from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import {
  Order,
  UpdateOrderStatusRequest
} from '../models/order.model';

@Injectable({
  // Creates one shared OrderService instance for the application.
  providedIn: 'root'
})
export class OrderService {
  private readonly httpClient = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiUrl}/Order`;

  // ---------------------------------------------------------
  // CUSTOMER ORDER OPERATIONS
  // ---------------------------------------------------------

  // Retrieves all orders belonging to the logged-in customer.
  getMyOrders(): Observable<Order[]> {
    return this.httpClient.get<Order[]>(
      `${this.apiUrl}/my-orders`
    );
  }

  // Retrieves one order only when it belongs to
  // the currently logged-in customer.
  getMyOrderById(
    orderId: number
  ): Observable<Order> {
    return this.httpClient.get<Order>(
      `${this.apiUrl}/my-orders/${orderId}`
    );
  }

  // ---------------------------------------------------------
  // ADMIN ORDER OPERATIONS
  // ---------------------------------------------------------

  // Retrieves all customer orders for Admin management.
  getAllOrders(): Observable<Order[]> {
    return this.httpClient.get<Order[]>(
      this.apiUrl
    );
  }

  // Retrieves one order by ID for an Admin.
  getOrderById(
    orderId: number
  ): Observable<Order> {
    return this.httpClient.get<Order>(
      `${this.apiUrl}/${orderId}`
    );
  }

  // Updates an order's status and returns the updated order.
  updateOrderStatus(
    orderId: number,
    request: UpdateOrderStatusRequest
  ): Observable<Order> {
    return this.httpClient.put<Order>(
      `${this.apiUrl}/${orderId}/status`,
      request
    );
  }
}