/**
 * Represents one item inside the customer's shopping cart.
 *
 * Similar to a CartItemDto in ASP.NET Core.
 */
export interface CartItem {
  id: number;
  foodItemId: number;
  foodItemName: string;
  unitPrice: number;
  quantity: number;
  isAvailable: boolean;
  subtotal: number;
}

/**
 * Represents the complete shopping cart returned by the API.
 */
export interface Cart {
  id: number;
  items: CartItem[];
  totalAmount: number;
}

/**
 * Request sent when adding a food item to the cart.
 */
export interface AddCartItemRequest {
  foodItemId: number;
  quantity: number;
}

/**
 * Request sent when updating the quantity of a cart item.
 */
export interface UpdateCartItemQuantityRequest {
  quantity: number;
}

/**
 * Represents a simple response returned by cart actions.
 */
export interface CartActionResponse {
  message: string;
}