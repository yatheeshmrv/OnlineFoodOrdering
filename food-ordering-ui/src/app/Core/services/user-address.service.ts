import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import {
  SaveUserAddressRequest,
  UserAddress
} from '../models/user-address.model';

@Injectable({
  providedIn: 'root'
})
export class UserAddressService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    'https://localhost:7068/api/UserAddresses';

  /** Gets every saved address belonging to the customer. */
  getAddresses(): Observable<UserAddress[]> {
    return this.http.get<UserAddress[]>(this.apiUrl);
  }

  /** Creates a new saved address. */
  createAddress(
    request: SaveUserAddressRequest
  ): Observable<unknown> {
    return this.http.post<unknown>(
      this.apiUrl,
      request
    );
  }

  /** Updates an existing customer-owned address. */
  updateAddress(
    addressId: number,
    request: SaveUserAddressRequest
  ): Observable<unknown> {
    return this.http.put<unknown>(
      `${this.apiUrl}/${addressId}`,
      request
    );
  }

  /** Deletes a customer-owned address. */
  deleteAddress(addressId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${addressId}`
    );
  }

  /** Makes one saved address the customer's default address. */
  setDefaultAddress(
    addressId: number
  ): Observable<unknown> {
    return this.http.put<unknown>(
      `${this.apiUrl}/${addressId}/default`,
      {}
    );
  }
}
