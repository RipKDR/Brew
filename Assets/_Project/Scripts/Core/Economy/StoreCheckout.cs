using System;
using System.Globalization;

namespace Brew.Core.Economy
{
    /// <summary>
    /// Routes store product taps to a purchase initiator. Never fulfills locally.
    /// Fulfillment stays on <see cref="IAPManager.CompletePurchase"/> after a store callback (ADR 0005).
    /// </summary>
    public static class StoreCheckout
    {
        /// <summary>
        /// Requests a store purchase for <paramref name="productId"/>.
        /// Does nothing when the id is empty or <paramref name="initiatePurchase"/> is null.
        /// </summary>
        public static void RequestPurchase(string productId, Action<string> initiatePurchase)
        {
            if (string.IsNullOrEmpty(productId) || initiatePurchase == null)
                return;

            initiatePurchase(productId);
        }

        /// <summary>
        /// Parses catalog display strings such as "$0.99" into a USD float for analytics.
        /// Does not invent prices; returns false when the display cannot be parsed.
        /// </summary>
        public static bool TryParseUsdPrice(string priceDisplay, out float usd)
        {
            usd = 0f;
            if (string.IsNullOrWhiteSpace(priceDisplay))
                return false;

            var trimmed = priceDisplay.Trim();
            if (trimmed.StartsWith("$", StringComparison.Ordinal))
                trimmed = trimmed.Substring(1);

            return float.TryParse(
                trimmed,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out usd);
        }
    }
}
