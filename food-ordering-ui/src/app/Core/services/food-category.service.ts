import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  CreateFoodCategoryRequest,
  FoodCategory,
  UpdateFoodCategoryRequest
} from '../models/food-category.model';

@Injectable({
  // Creates one shared service instance for the application.
  providedIn: 'root'
})
export class FoodCategoryService {
  private readonly httpClient = inject(HttpClient);

  private readonly apiUrl =
    'https://localhost:7068/api/FoodCategory';

  // Retrieves all food categories.
  getFoodCategories(): Observable<FoodCategory[]> {
    return this.httpClient.get<FoodCategory[]>(
      this.apiUrl
    );
  }

  // Retrieves one food category by its ID.
  getFoodCategoryById(
    foodCategoryId: number
  ): Observable<FoodCategory> {
    return this.httpClient.get<FoodCategory>(
      `${this.apiUrl}/${foodCategoryId}`
    );
  }

  // Creates a new food category.
  createFoodCategory(
    request: CreateFoodCategoryRequest
  ): Observable<FoodCategory> {
    return this.httpClient.post<FoodCategory>(
      this.apiUrl,
      request
    );
  }

  // Updates an existing food category.
  updateFoodCategory(
    foodCategoryId: number,
    request: UpdateFoodCategoryRequest
  ): Observable<FoodCategory> {
    return this.httpClient.put<FoodCategory>(
      `${this.apiUrl}/${foodCategoryId}`,
      request
    );
  }

  // Deletes an existing food category.
  deleteFoodCategory(
    foodCategoryId: number
  ): Observable<void> {
    return this.httpClient.delete<void>(
      `${this.apiUrl}/${foodCategoryId}`
    );
  }
}