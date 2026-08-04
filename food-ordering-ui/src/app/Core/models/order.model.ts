/**
 * Represents one food item included in an order request.
 *
 * This model is retained for compatibility with any existing
 * OrderService code that still references CreateOrderRequest.
 */
export interface OrderItemRequest {
  foodItemId: number;
  quantity: number;
}

/**
 * Legacy direct-order request model.
 *
 * Cart checkout now uses CheckoutRequest instead. This interface
 * remains temporarily so existing unused code does not break.
 */
export interface CreateOrderRequest {
  items: OrderItemRequest[];
}

/**
 * Request sent to POST /api/Cart/checkout.
 */
export interface CheckoutRequest {
  userAddressId: number;
  deliveryInstructions: string | null;
}

/**
 * Represents one item in an order returned by the API.
 */
export interface OrderItem {
  foodItemId: number;
  foodItemName: string;
  quantity: number;
  unitPrice: number;
}

/**
 * Represents an order returned by the backend.
 *
 * Delivery fields are immutable snapshots copied from the saved
 * address that the customer selected during checkout.
 */
export interface Order {
  id: number;
  customerName: string;
  customerPhone: string | null;
  totalAmount: number;
  orderStatus: string;
  orderDate: string;
  items: OrderItem[];

  deliveryRecipientName: string | null;
  deliveryPhone: string | null;
  deliveryAddressLine1: string | null;
  deliveryAddressLine2: string | null;
  deliveryLandmark: string | null;
  deliveryCity: string | null;
  deliveryState: string | null;
  deliveryPostalCode: string | null;
  deliveryInstructions: string | null;
}

/**
 * Response returned after attempting cart checkout.
 */
export interface CreateOrderResponse {
  isSuccess: boolean;
  message: string;
  order: Order | null;
}

/**
 * Request used by an Admin to change an order's status.
 */
export interface UpdateOrderStatusRequest {
  orderStatus: string;
}
