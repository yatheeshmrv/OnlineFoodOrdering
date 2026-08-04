/**
 * Represents one reusable delivery address belonging to a customer.
 */
export interface UserAddress {
  id: number;
  addressLabel: string;
  recipientName: string;
  recipientPhone: string;
  addressLine1: string;
  addressLine2: string | null;
  landmark: string | null;
  city: string;
  state: string;
  postalCode: string;
  isDefault: boolean;
}

/**
 * Request used when creating or updating a saved address.
 */
export interface SaveUserAddressRequest {
  addressLabel: string;
  recipientName: string;
  recipientPhone: string;
  addressLine1: string;
  addressLine2: string | null;
  landmark: string | null;
  city: string;
  state: string;
  postalCode: string;
  isDefault: boolean;
}
