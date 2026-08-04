import {
  inject,
  Injectable
} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  CreateFoodItemRequest,
  FoodItem,
  PagedFoodItemResponse,
  UpdateFoodItemRequest
} from '../models/food-item.model';

@Injectable({
  // Registers one shared instance of this service for the application.
  providedIn: 'root'
})
export class FoodItemService {
  private readonly httpClient = inject(HttpClient);

  // FoodItems endpoint exposed by the ASP.NET Core API.
  private readonly apiUrl =
    'https://localhost:7068/api/FoodItems';

  // Retrieves a paginated collection of food items from the API.
  getFoodItems(
    pageNumber: number = 1,
    pageSize: number = 100
  ): Observable<PagedFoodItemResponse> {
    return this.httpClient.get<PagedFoodItemResponse>(
      this.apiUrl,
      {
        params: {
          pageNumber,
          pageSize
        }
      }
    );
  }

  // Sends a new food item to the Admin-protected API endpoint.
  createFoodItem(
    request: CreateFoodItemRequest
  ): Observable<FoodItem> {
    return this.httpClient.post<FoodItem>(
      this.apiUrl,
      request
    );
  }

  // Replaces the editable values of an existing food item.
  updateFoodItem(
    foodItemId: number,
    request: UpdateFoodItemRequest
  ): Observable<FoodItem> {
    return this.httpClient.put<FoodItem>(
      `${this.apiUrl}/${foodItemId}`,
      request
    );
  }

  // Permanently removes a food item.
  deleteFoodItem(
    foodItemId: number
  ): Observable<void> {
    return this.httpClient.delete<void>(
      `${this.apiUrl}/${foodItemId}`
    );
  }
}