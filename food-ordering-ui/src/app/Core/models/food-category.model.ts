// Represents one food category returned by the API.
export interface FoodCategory {
  id: number;
  categoryName: string;
  isActive: boolean;
}

// Matches the backend CreateFoodCategoryDto.
export interface CreateFoodCategoryRequest {
  categoryName: string;
  isActive: boolean;
}

// Matches the backend UpdateFoodCategoryDto.
export interface UpdateFoodCategoryRequest {
  categoryName: string;
  isActive: boolean;
}