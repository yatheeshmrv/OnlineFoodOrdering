// Represents one food item returned by the FoodOrderAPI.
export interface FoodItem {
  id: number;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  foodCategoryId: number;
  foodCategoryName: string;
  isAvailable: boolean;
}

// Represents the paginated response returned by GET /api/FoodItems.
export interface PagedFoodItemResponse {
  items: FoodItem[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Matches the backend CreateFoodItemDto.
// The Admin sends this payload when creating a food item.
export interface CreateFoodItemRequest {
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  foodCategoryId: number;
  isAvailable: boolean;
}

// Matches the backend UpdateFoodItemDto.
export interface UpdateFoodItemRequest {
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  foodCategoryId: number;
  isAvailable: boolean;
}