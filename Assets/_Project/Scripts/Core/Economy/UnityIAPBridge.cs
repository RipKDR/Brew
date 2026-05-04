using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brew.Core.Economy
{
#if UNITY_IAP
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;

    public sealed class UnityIAPBridge : IDetailedStoreListener
    {
        private readonly IAPManager _iapManager;
        private IStoreController _storeController;
        private IExtensionProvider _extensions;
        private bool _initialized;

        public bool IsInitialized => _initialized;

        public event Action OnInitialized;
        public event Action<string> OnInitializeFailed;

        public UnityIAPBridge(IAPManager iapManager)
        {
            _iapManager = iapManager ?? throw new ArgumentNullException(nameof(iapManager));
        }

        public void Initialize()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (var product in IAPManager.Catalog)
            {
                var productType = product.Type == IAPProductType.Consumable
                    ? ProductType.Consumable
                    : ProductType.NonConsumable;
                builder.AddProduct(product.ProductId, productType);
            }

            UnityPurchasing.Initialize(this, builder);
            Debug.Log("[UnityIAPBridge] Initializing Unity IAP...");
        }

        public void PurchaseProduct(string productId)
        {
            if (!_initialized || _storeController == null)
            {
                Debug.LogWarning("[UnityIAPBridge] Store not initialized.");
                return;
            }

            var product = _storeController.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"[UnityIAPBridge] Product {productId} not available.");
                return;
            }

            _storeController.InitiatePurchase(product);
        }

        public void RestorePurchases()
        {
            if (!_initialized) return;

            var apple = _extensions?.GetExtension<IAppleExtensions>();
            apple?.RestoreTransactions(result =>
            {
                Debug.Log($"[UnityIAPBridge] Restore result: {result}");
            });
        }

        void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensions = extensions;
            _initialized = true;

            var ownedIds = new List<string>();
            foreach (var product in IAPManager.Catalog)
            {
                if (product.Type == IAPProductType.NonConsumable)
                {
                    var storeProduct = controller.products.WithID(product.ProductId);
                    if (storeProduct != null && storeProduct.hasReceipt)
                        ownedIds.Add(product.ProductId);
                }
            }

            if (ownedIds.Count > 0)
                _iapManager.RestorePurchases(ownedIds.ToArray());

            OnInitialized?.Invoke();
            Debug.Log("[UnityIAPBridge] Unity IAP initialized.");
        }

        void IStoreListener.OnInitializeFailed(InitializationFailureReason error)
        {
            _initialized = false;
            OnInitializeFailed?.Invoke(error.ToString());
            Debug.LogError($"[UnityIAPBridge] Init failed: {error}");
        }

        void IStoreListener.OnInitializeFailed(InitializationFailureReason error, string message)
        {
            _initialized = false;
            OnInitializeFailed?.Invoke($"{error}: {message}");
            Debug.LogError($"[UnityIAPBridge] Init failed: {error} — {message}");
        }

        PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs args)
        {
            var productId = args.purchasedProduct.definition.id;
            _iapManager.CompletePurchase(productId);
            Debug.Log($"[UnityIAPBridge] Purchase completed: {productId}");
            return PurchaseProcessingResult.Complete;
        }

        void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogWarning($"[UnityIAPBridge] Purchase failed: {product.definition.id} — {reason}");
        }

        void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogWarning($"[UnityIAPBridge] Purchase failed: {product.definition.id} — {failureDescription.message}");
        }
    }
#else
    public sealed class UnityIAPBridge
    {
        public bool IsInitialized => false;

#pragma warning disable CS0067
        public event Action OnInitialized;
        public event Action<string> OnInitializeFailed;
#pragma warning restore CS0067

        public UnityIAPBridge(IAPManager iapManager)
        {
            if (iapManager == null) throw new ArgumentNullException(nameof(iapManager));
            Debug.LogWarning("[UnityIAPBridge] UNITY_IAP not defined. IAP disabled.");
        }

        public void Initialize() { }

        public void PurchaseProduct(string productId) { }

        public void RestorePurchases() { }
    }
#endif
}
