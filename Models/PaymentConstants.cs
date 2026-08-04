namespace FoodOrderAPI.Models
{
    // Contains every payment method currently supported at checkout.
    public static class PaymentMethods
    {
        public const string CashOnDelivery = "CashOnDelivery";

        public static bool IsSupported(string? paymentMethod)
        {
            return string.Equals(
                paymentMethod?.Trim(),
                CashOnDelivery,
                StringComparison.OrdinalIgnoreCase);
        }

        public static string Normalize(string paymentMethod)
        {
            if (!IsSupported(paymentMethod))
            {
                throw new ArgumentException(
                    "Unsupported payment method.",
                    nameof(paymentMethod));
            }

            return CashOnDelivery;
        }
    }

    // Contains every payment status that can be stored on an order.
    public static class PaymentStatuses
    {
        public const string Pending = "Pending";
        public const string Paid = "Paid";
        public const string Failed = "Failed";
        public const string Refunded = "Refunded";
    }
}
