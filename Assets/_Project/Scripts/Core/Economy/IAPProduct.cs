namespace Brew.Core.Economy
{
    public enum IAPProductType
    {
        Consumable,
        NonConsumable
    }

    /// <summary>
    /// Store metadata for an IAP SKU. Pure data — no Unity dependencies.
    /// </summary>
    public sealed class IAPProduct
    {
        public string ProductId { get; }
        public IAPProductType Type { get; }
        public string PriceDisplay { get; }
        public int GemAmount { get; }
        public int EssenceAmount { get; }
        public int BoosterCount { get; }
        public string BadgeText { get; }
        /// <summary>-1 means no level gate (always eligible if otherwise available).</summary>
        public int MaxLevelAvailable { get; }

        public IAPProduct(
            string productId,
            IAPProductType type,
            string priceDisplay,
            int gemAmount,
            int essenceAmount,
            int boosterCount,
            string badgeText,
            int maxLevelAvailable)
        {
            ProductId = productId;
            Type = type;
            PriceDisplay = priceDisplay;
            GemAmount = gemAmount;
            EssenceAmount = essenceAmount;
            BoosterCount = boosterCount;
            BadgeText = badgeText ?? string.Empty;
            MaxLevelAvailable = maxLevelAvailable;
        }
    }
}
