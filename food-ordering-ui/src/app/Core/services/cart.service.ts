import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import {
  AddCartItemRequest,
  Cart,
  UpdateCartItemQuantityRequest
} from '../models/cart.model';
import {
  CheckoutRequest,
  CreateOrderResponse
} from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    'https://localhost:7068/api/Cart';

  /** Gets the logged-in customer's current cart. */
  getCart(): Observable<Cart> {
    return this.http.get<Cart>(this.apiUrl);
  }

  /** Adds a food item to the customer's cart. */
  addCartItem(
    request: AddCartItemRequest
  ): Observable<Cart> {
    return this.http.post<Cart>(
      `${this.apiUrl}/items`,
      request
    );
  }

  /** Updates the quantity of an existing cart item. */
  updateCartItemQuantity(
    cartItemId: number,
    request: UpdateCartItemQuantityRequest
  ): Observable<Cart> {
    return this.http.put<Cart>(
      `${this.apiUrl}/items/${cartItemId}`,
      request
    );
  }

  /** Removes one item from the cart. */
  removeCartItem(
    cartItemId: number
  ): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/items/${cartItemId}`
    );
  }

  /** Removes every item from the cart. */
  clearCart(): Observable<void> {
    return this.http.delete<void>(this.apiUrl);
  }

  /**
   * Creates an order from the current cart using the selected
   * saved delivery address.
   */
  checkout(
    request: CheckoutRequest
  ): Observable<CreateOrderResponse> {
    return this.http.post<CreateOrderResponse>(
      `${this.apiUrl}/checkout`,
      request
    );
  }
}
