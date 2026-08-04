using FoodOrderAPI.DTOs;
using FoodOrderAPI.Models;
using FoodOrderAPI.Validators;
using Xunit;

namespace FoodOrderAPI.UnitTests.Validators
{
    public class CheckoutDtoValidatorTests
    {
        [Fact]
        public void Validate_WhenCashOnDeliveryIsSelected_ReturnsNoPaymentError()
        {
            var validator = new CheckoutDtoValidator();

            var checkoutDto = new CheckoutDto
            {
                UserAddressId = 1,
                PaymentMethod = PaymentMethods.CashOnDelivery
            };

            var validationResult = validator.Validate(checkoutDto);

            Assert.DoesNotContain(
                validationResult.Errors,
                error => error.PropertyName ==
                    nameof(CheckoutDto.PaymentMethod));
        }

        [Fact]
        public void Validate_WhenPaymentMethodIsUnsupported_ReturnsPaymentError()
        {
            var validator = new CheckoutDtoValidator();

            var checkoutDto = new CheckoutDto
            {
                UserAddressId = 1,
                PaymentMethod = "Card"
            };

            var validationResult = validator.Validate(checkoutDto);

            Assert.Contains(
                validationResult.Errors,
                error => error.PropertyName ==
                    nameof(CheckoutDto.PaymentMethod));
        }

        [Fact]
        public void Validate_WhenPaymentMethodIsEmpty_ReturnsPaymentError()
        {
            var validator = new CheckoutDtoValidator();

            var checkoutDto = new CheckoutDto
            {
                UserAddressId = 1,
                PaymentMethod = string.Empty
            };

            var validationResult = validator.Validate(checkoutDto);

            Assert.Contains(
                validationResult.Errors,
                error => error.PropertyName ==
                    nameof(CheckoutDto.PaymentMethod));
        }
    }
}
