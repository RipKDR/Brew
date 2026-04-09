using System;
using System.Collections.Generic;
using System.Linq;

namespace Brew.Core.Economy
{
    /// <summary>
    /// In-app purchase catalog and fulfillment against <see cref="CurrencyManager"/>.
    /// Pure C# — no Unity dependencies. Booster grants use <see cref="IAPProduct.BoosterCount"/> as catalog metadata; wire <see cref="Brew.Core.BoosterManager"/> in the presentation layer if needed.
    /// </summary>
    public sealed class IAPManager
    {
        public static readonly IReadOnlyList<IAPProduct> Catalog = new List<IAPProduct>
        {
            new IAPProduct("gem_50", IAPProductType.Consumable, "$0.99", 50, 0, 0, string.Empty, -1),
            new IAPProduct("gem_300", IAPProductType.Consumable, "$4.99", 300, 0, 0, "Best Value", -1),
            new IAPProduct("gem_700", IAPProductType.Consumable, "$9.99", 700, 0, 0, "Most Popular", -1),
            new IAPProduct("gem_1500", IAPProductType.Consumable, "$19.99", 1500, 0, 0, "Best Deal", -1),
            new IAPProduct("starter_bundle", IAPProductType.NonConsumable, "$1.99", 50, 500, 3, "Starter Bundle", 15),
            new IAPProduct("no_ads", IAPProductType.NonConsumable, "$4.99", 0, 0, 0, string.Empty, -1)
        };

        private readonly CurrencyManager _currency;
        private readonly HashSet<string> _ownedNonConsumables = new();

        public event Action<string> OnPurchaseCompleted;
        public event Action<string, string> OnPurchaseFailed;

        public IAPManager(CurrencyManager currency)
        {
            _currency = currency ?? throw new ArgumentNullException(nameof(currency));
        }

        public bool HasNoAdsPass => HasPurchased("no_ads");

        public IReadOnlyList<IAPProduct> GetAvailableProducts(int currentLevel)
        {
            return Catalog.Where(p => IsProductAvailable(p, currentLevel)).ToList();
        }

        private bool IsProductAvailable(IAPProduct product, int currentLevel)
        {
            if (product.Type == IAPProductType.NonConsumable && _ownedNonConsumables.Contains(product.ProductId))
                return false;

            if (product.MaxLevelAvailable >= 0 && currentLevel > product.MaxLevelAvailable)
                return false;

            return true;
        }

        public bool HasPurchased(string productId)
        {
            return _ownedNonConsumables.Contains(productId);
        }

        public void CompletePurchase(string productId)
        {
            var product = Catalog.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                OnPurchaseFailed?.Invoke(productId, "unknown_product");
                return;
            }

            if (product.Type == IAPProductType.NonConsumable && _ownedNonConsumables.Contains(productId))
            {
                OnPurchaseFailed?.Invoke(productId, "already_owned");
                return;
            }

            var source = "iap_" + productId;

            if (product.GemAmount > 0)
                _currency.Add(CurrencyType.Gems, product.GemAmount, source);

            if (product.EssenceAmount > 0)
                _currency.Add(CurrencyType.Essence, product.EssenceAmount, source);

            if (product.Type == IAPProductType.NonConsumable)
                _ownedNonConsumables.Add(productId);

            OnPurchaseCompleted?.Invoke(productId);
        }

        public void RestorePurchases(string[] productIds)
        {
            if (productIds == null)
                return;

            foreach (var id in productIds)
            {
                var product = Catalog.FirstOrDefault(p => p.ProductId == id);
                if (product?.Type == IAPProductType.NonConsumable)
                    _ownedNonConsumables.Add(id);
            }
        }
    }
}
