/**
 * Payment methods currently accepted by the checkout API.
 *
 * Add new values here when the backend begins supporting
 * additional payment options.
 */
export type PaymentMethod = 'CashOnDelivery';

/**
 * Payment states returned by the backend for an order.
 */
export type PaymentStatus =
  | 'Pending'
  | 'Paid'
  | 'Failed'
  | 'Refunded';

/**
 * Request sent to POST /api/Cart/checkout.
 */
export interface CheckoutRequest {
  userAddressId: number;
  deliveryInstructions: string | null;
  paymentMethod: PaymentMethod;
}

/**
 * Represents one item in an order returned by the API.
 */
export interface OrderItem {
  foodItemId: number;
  foodItemName: string;
  imageUrl: string;
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
  paymentMethod: PaymentMethod;
  paymentStatus: PaymentStatus;
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
