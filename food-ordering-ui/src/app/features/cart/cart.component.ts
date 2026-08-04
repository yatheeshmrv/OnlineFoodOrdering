import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  computed,
  inject,
  OnInit,
  signal
} from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import {
  Cart as CartModel,
  CartItem
} from '../../Core/models/cart.model';
import { CheckoutRequest } from '../../Core/models/order.model';
import {
  SaveUserAddressRequest,
  UserAddress
} from '../../Core/models/user-address.model';
import { CartService } from '../../Core/services/cart.service';
import { UserAddressService } from '../../Core/services/user-address.service';

@Component({
  selector: 'app-cart',
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class Cart implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly userAddressService =
    inject(UserAddressService);
  private readonly formBuilder = inject(FormBuilder);

  readonly cart = signal<CartModel | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly processingItemId = signal<number | null>(null);
  readonly isClearing = signal(false);

  readonly isCheckingOut = signal(false);
  readonly checkoutMessage = signal('');
  readonly checkoutErrorMessage = signal('');

  readonly addresses = signal<UserAddress[]>([]);
  readonly isLoadingAddresses = signal(true);
  readonly addressErrorMessage = signal('');
  readonly addressSuccessMessage = signal('');
  readonly selectedAddressId = signal<number | null>(null);

  readonly isAddressFormOpen = signal(false);
  readonly editingAddressId = signal<number | null>(null);
  readonly isSavingAddress = signal(false);
  readonly deletingAddressId = signal<number | null>(null);
  readonly settingDefaultAddressId = signal<number | null>(null);

  readonly deliveryInstructions = signal('');

  readonly addressForm = this.formBuilder.nonNullable.group({
    addressLabel: [
      '',
      [
        Validators.required,
        Validators.maxLength(50)
      ]
    ],
    recipientName: [
      '',
      [
        Validators.required,
        Validators.maxLength(100)
      ]
    ],
    recipientPhone: [
      '',
      [
        Validators.required,
        Validators.pattern(/^[6-9]\d{9}$/)
      ]
    ],
    addressLine1: [
      '',
      [
        Validators.required,
        Validators.maxLength(200)
      ]
    ],
    addressLine2: [
      '',
      Validators.maxLength(200)
    ],
    landmark: [
      '',
      Validators.maxLength(100)
    ],
    city: [
      '',
      [
        Validators.required,
        Validators.maxLength(100)
      ]
    ],
    state: [
      '',
      [
        Validators.required,
        Validators.maxLength(100)
      ]
    ],
    postalCode: [
      '',
      [
        Validators.required,
        Validators.pattern(/^\d{6}$/)
      ]
    ],
    isDefault: [false]
  });

  readonly isCartEmpty = computed(
    () => (this.cart()?.items.length ?? 0) === 0
  );

  readonly totalItemQuantity = computed(() =>
    this.cart()?.items.reduce(
      (total, item) => total + item.quantity,
      0
    ) ?? 0
  );

  readonly allItemsAvailable = computed(
    () =>
      this.cart()?.items.every(
        (item) => item.isAvailable
      ) ?? true
  );

  readonly selectedAddress = computed(() => {
    const addressId = this.selectedAddressId();

    return this.addresses().find(
      (address) => address.id === addressId
    ) ?? null;
  });

  readonly isAddressMutationInProgress = computed(
    () =>
      this.isSavingAddress() ||
      this.deletingAddressId() !== null ||
      this.settingDefaultAddressId() !== null
  );

  readonly canCheckout = computed(
    () =>
      !this.isCartEmpty() &&
      this.allItemsAvailable() &&
      this.selectedAddress() !== null &&
      this.processingItemId() === null &&
      !this.isClearing() &&
      !this.isCheckingOut() &&
      !this.isAddressMutationInProgress()
  );

  ngOnInit(): void {
    this.loadCart();
    this.loadAddresses();
  }

  loadCart(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.cartService
      .getCart()
      .pipe(
        finalize(() => {
          this.isLoading.set(false);
        })
      )
      .subscribe({
        next: (cart) => {
          this.cart.set(cart);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to load your cart. Please try again.'
            )
          );
        }
      });
  }

  loadAddresses(
    preferredAddressId?: number
  ): void {
    this.isLoadingAddresses.set(true);
    this.addressErrorMessage.set('');

    this.userAddressService
      .getAddresses()
      .pipe(
        finalize(() => {
          this.isLoadingAddresses.set(false);
        })
      )
      .subscribe({
        next: (addresses) => {
          const sortedAddresses = [...addresses].sort(
            (firstAddress, secondAddress) =>
              Number(secondAddress.isDefault) -
              Number(firstAddress.isDefault)
          );

          this.addresses.set(sortedAddresses);

          const currentAddressId =
            preferredAddressId ??
            this.selectedAddressId();

          const currentAddressStillExists =
            currentAddressId !== null &&
            sortedAddresses.some(
              (address) =>
                address.id === currentAddressId
            );

          if (currentAddressStillExists) {
            this.selectedAddressId.set(
              currentAddressId
            );
            return;
          }

          const defaultAddress =
            sortedAddresses.find(
              (address) => address.isDefault
            );

          this.selectedAddressId.set(
            defaultAddress?.id ??
              sortedAddresses[0]?.id ??
              null
          );
        },
        error: (error: HttpErrorResponse) => {
          this.addresses.set([]);
          this.selectedAddressId.set(null);
          this.addressErrorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to load your saved addresses.'
            )
          );
        }
      });
  }

  increaseQuantity(item: CartItem): void {
    this.updateQuantity(item, item.quantity + 1);
  }

  decreaseQuantity(item: CartItem): void {
    if (item.quantity <= 1) {
      return;
    }

    this.updateQuantity(item, item.quantity - 1);
  }

  removeItem(item: CartItem): void {
    if (this.processingItemId() !== null) {
      return;
    }

    this.processingItemId.set(item.id);
    this.errorMessage.set('');

    this.cartService
      .removeCartItem(item.id)
      .pipe(
        finalize(() => {
          this.processingItemId.set(null);
        })
      )
      .subscribe({
        next: () => {
          this.removeItemFromLocalCart(item.id);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to remove the item from your cart.'
            )
          );
        }
      });
  }

  clearCart(): void {
    if (this.isCartEmpty() || this.isClearing()) {
      return;
    }

    this.isClearing.set(true);
    this.errorMessage.set('');

    this.cartService
      .clearCart()
      .pipe(
        finalize(() => {
          this.isClearing.set(false);
        })
      )
      .subscribe({
        next: () => {
          const currentCart = this.cart();

          if (!currentCart) {
            return;
          }

          this.cart.set({
            ...currentCart,
            items: [],
            totalAmount: 0
          });
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to clear your cart.'
            )
          );
        }
      });
  }

  selectAddress(addressId: number): void {
    if (
      this.isCheckingOut() ||
      this.isAddressMutationInProgress()
    ) {
      return;
    }

    this.selectedAddressId.set(addressId);
    this.checkoutErrorMessage.set('');
  }

  openAddAddressForm(): void {
    if (this.isAddressMutationInProgress()) {
      return;
    }

    this.clearAddressMessages();
    this.editingAddressId.set(null);
    this.addressForm.reset({
      addressLabel: '',
      recipientName: '',
      recipientPhone: '',
      addressLine1: '',
      addressLine2: '',
      landmark: '',
      city: '',
      state: '',
      postalCode: '',
      isDefault: this.addresses().length === 0
    });
    this.isAddressFormOpen.set(true);
  }

  openEditAddressForm(address: UserAddress): void {
    if (this.isAddressMutationInProgress()) {
      return;
    }

    this.clearAddressMessages();
    this.editingAddressId.set(address.id);
    this.addressForm.reset({
      addressLabel: address.addressLabel,
      recipientName: address.recipientName,
      recipientPhone: address.recipientPhone,
      addressLine1: address.addressLine1,
      addressLine2: address.addressLine2 ?? '',
      landmark: address.landmark ?? '',
      city: address.city,
      state: address.state,
      postalCode: address.postalCode,
      isDefault: address.isDefault
    });
    this.isAddressFormOpen.set(true);
  }

  cancelAddressForm(): void {
    if (this.isSavingAddress()) {
      return;
    }

    this.addressForm.reset();
    this.editingAddressId.set(null);
    this.isAddressFormOpen.set(false);
  }

  saveAddress(): void {
    if (this.isSavingAddress()) {
      return;
    }

    this.addressForm.markAllAsTouched();

    if (this.addressForm.invalid) {
      return;
    }

    const editingAddressId = this.editingAddressId();
    const request = this.createAddressRequest();

    this.clearAddressMessages();
    this.isSavingAddress.set(true);

    const saveRequest = editingAddressId === null
      ? this.userAddressService.createAddress(request)
      : this.userAddressService.updateAddress(
          editingAddressId,
          request
        );

    saveRequest
      .pipe(
        finalize(() => {
          this.isSavingAddress.set(false);
        })
      )
      .subscribe({
        next: () => {
          this.addressSuccessMessage.set(
            editingAddressId === null
              ? 'Delivery address added successfully.'
              : 'Delivery address updated successfully.'
          );

          this.editingAddressId.set(null);
          this.isAddressFormOpen.set(false);
          this.addressForm.reset();
          this.loadAddresses(
            editingAddressId ?? undefined
          );
        },
        error: (error: HttpErrorResponse) => {
          this.addressErrorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to save the delivery address.'
            )
          );
        }
      });
  }

  deleteAddress(address: UserAddress): void {
    if (this.isAddressMutationInProgress()) {
      return;
    }

    const confirmed = window.confirm(
      `Delete the ${address.addressLabel} address?`
    );

    if (!confirmed) {
      return;
    }

    this.clearAddressMessages();
    this.deletingAddressId.set(address.id);

    this.userAddressService
      .deleteAddress(address.id)
      .pipe(
        finalize(() => {
          this.deletingAddressId.set(null);
        })
      )
      .subscribe({
        next: () => {
          this.addressSuccessMessage.set(
            'Delivery address deleted successfully.'
          );
          this.loadAddresses();
        },
        error: (error: HttpErrorResponse) => {
          this.addressErrorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to delete the delivery address.'
            )
          );
        }
      });
  }

  setDefaultAddress(address: UserAddress): void {
    if (
      address.isDefault ||
      this.isAddressMutationInProgress()
    ) {
      return;
    }

    this.clearAddressMessages();
    this.settingDefaultAddressId.set(address.id);

    this.userAddressService
      .setDefaultAddress(address.id)
      .pipe(
        finalize(() => {
          this.settingDefaultAddressId.set(null);
        })
      )
      .subscribe({
        next: () => {
          this.addressSuccessMessage.set(
            `${address.addressLabel} is now your default address.`
          );
          this.loadAddresses(address.id);
        },
        error: (error: HttpErrorResponse) => {
          this.addressErrorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to change the default address.'
            )
          );
        }
      });
  }

  updateDeliveryInstructions(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    this.deliveryInstructions.set(textarea.value);
  }

  checkout(): void {
    const currentCart = this.cart();
    const selectedAddress = this.selectedAddress();

    if (
      !currentCart ||
      currentCart.items.length === 0 ||
      this.isCheckingOut()
    ) {
      return;
    }

    if (!this.allItemsAvailable()) {
      this.checkoutErrorMessage.set(
        'Remove unavailable items before checkout.'
      );
      return;
    }

    if (!selectedAddress) {
      this.checkoutErrorMessage.set(
        'Select or add a delivery address before checkout.'
      );
      return;
    }

    const normalizedInstructions =
      this.deliveryInstructions().trim();

    const request: CheckoutRequest = {
      userAddressId: selectedAddress.id,
      deliveryInstructions:
        normalizedInstructions || null
    };

    this.isCheckingOut.set(true);
    this.checkoutMessage.set('');
    this.checkoutErrorMessage.set('');
    this.errorMessage.set('');

    this.cartService
      .checkout(request)
      .pipe(
        finalize(() => {
          this.isCheckingOut.set(false);
        })
      )
      .subscribe({
        next: (response) => {
          if (!response.isSuccess || !response.order) {
            this.checkoutErrorMessage.set(
              response.message ||
                'Unable to complete your order.'
            );
            return;
          }

          this.checkoutMessage.set(
            `Order #${response.order.id} was placed successfully.`
          );

          this.cart.set({
            ...currentCart,
            items: [],
            totalAmount: 0
          });

          this.deliveryInstructions.set('');
        },
        error: (error: HttpErrorResponse) => {
          this.checkoutErrorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to complete your order. Please try again.'
            )
          );
        }
      });
  }

  private updateQuantity(
    item: CartItem,
    quantity: number
  ): void {
    if (this.processingItemId() !== null) {
      return;
    }

    this.processingItemId.set(item.id);
    this.errorMessage.set('');

    this.cartService
      .updateCartItemQuantity(item.id, { quantity })
      .pipe(
        finalize(() => {
          this.processingItemId.set(null);
        })
      )
      .subscribe({
        next: (updatedCart) => {
          this.cart.set(updatedCart);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to update the item quantity.'
            )
          );
        }
      });
  }

  private removeItemFromLocalCart(
    cartItemId: number
  ): void {
    const currentCart = this.cart();

    if (!currentCart) {
      return;
    }

    const remainingItems = currentCart.items.filter(
      (item) => item.id !== cartItemId
    );

    const updatedTotal = remainingItems.reduce(
      (total, item) => total + item.subtotal,
      0
    );

    this.cart.set({
      ...currentCart,
      items: remainingItems,
      totalAmount: updatedTotal
    });
  }

  private createAddressRequest(): SaveUserAddressRequest {
    const value = this.addressForm.getRawValue();

    return {
      addressLabel: value.addressLabel.trim(),
      recipientName: value.recipientName.trim(),
      recipientPhone: value.recipientPhone.trim(),
      addressLine1: value.addressLine1.trim(),
      addressLine2:
        value.addressLine2.trim() || null,
      landmark: value.landmark.trim() || null,
      city: value.city.trim(),
      state: value.state.trim(),
      postalCode: value.postalCode.trim(),
      isDefault: value.isDefault
    };
  }

  private clearAddressMessages(): void {
    this.addressErrorMessage.set('');
    this.addressSuccessMessage.set('');
  }

  private getErrorMessage(
    error: HttpErrorResponse,
    fallbackMessage: string
  ): string {
    const backendMessage = error.error?.message;

    if (
      typeof backendMessage === 'string' &&
      backendMessage.trim().length > 0
    ) {
      return backendMessage;
    }

    const validationErrors = error.error?.errors;

    if (
      validationErrors &&
      typeof validationErrors === 'object' &&
      !Array.isArray(validationErrors)
    ) {
      const firstValidationMessage = Object.values(
        validationErrors
      )
        .flatMap((messages) =>
          Array.isArray(messages)
            ? messages
            : [messages]
        )
        .find(
          (message): message is string =>
            typeof message === 'string'
        );

      if (firstValidationMessage) {
        return firstValidationMessage;
      }
    }

    return fallbackMessage;
  }
}
