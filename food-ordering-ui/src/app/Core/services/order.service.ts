import { HttpClient } from '@angular/common/http';
import {
  inject,
  Injectable
} from '@angular/core';
import { Observable } from 'rxjs';

import {
  CreateOrderRequest,
  CreateOrderResponse,
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
    'https://localhost:7068/api/Order';

  // ---------------------------------------------------------
  // CUSTOMER ORDER OPERATIONS
  // ---------------------------------------------------------

  // Creates an order for the currently logged-in customer.
  createOrder(
    request: CreateOrderRequest
  ): Observable<CreateOrderResponse> {
    return this.httpClient.post<CreateOrderResponse>(
      this.apiUrl,
      request
    );
  }

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