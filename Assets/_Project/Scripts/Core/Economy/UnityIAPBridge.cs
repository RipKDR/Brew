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
        private readonly IapPendingOperations _pending = new();
        private IStoreController _storeController;
        private IExtensionProvider _extensions;
        private bool _initialized;

        public bool IsInitialized => _initialized;

        public event Action OnInitialized;
        public event Action<string> OnInitializeFailed;
        public event Action<string> OnPurchaseInitiated;
        public event Action OnRestored;

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

        /// <summary>
        /// Starts a store purchase, or queues it until initialization completes.
        /// Returns true only when the store sheet was actually opened.
        /// </summary>
        public bool PurchaseProduct(string productId)
        {
            var disposition = _pending.RequestPurchase(productId, CanPurchase);
            if (disposition == IapPurchaseDisposition.Ignored)
                return false;
            if (disposition == IapPurchaseDisposition.Queued)
            {
                Debug.Log("[UnityIAPBridge] Purchase queued until store is ready.");
                return false;
            }

            return TryInitiatePurchase(productId);
        }

        public void RestorePurchases()
        {
            if (_pending.RequestRestore(_initialized) == IapRestoreDisposition.Queued)
            {
                Debug.Log("[UnityIAPBridge] Restore queued until store is ready.");
                return;
            }

            ExecuteRestore();
        }

        void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensions = extensions;
            _initialized = true;

            RestoreOwnedFromReceipts();
            OnInitialized?.Invoke();
            FlushPending();
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

        private bool CanPurchase => _initialized && _storeController != null;

        private void FlushPending()
        {
            if (_pending.TryTakeRestore())
                ExecuteRestore();

            if (_pending.TryTakePurchase(out var productId))
                TryInitiatePurchase(productId);
        }

        private bool TryInitiatePurchase(string productId)
        {
            if (!CanPurchase)
            {
                Debug.LogWarning("[UnityIAPBridge] Store not initialized.");
                return false;
            }

            var product = _storeController.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"[UnityIAPBridge] Product {productId} not available.");
                return false;
            }

            _storeController.InitiatePurchase(product);
            OnPurchaseInitiated?.Invoke(productId);
            return true;
        }

        private void ExecuteRestore()
        {
            var apple = _extensions?.GetExtension<IAppleExtensions>();
            if (apple != null)
            {
                apple.RestoreTransactions(result =>
                {
                    Debug.Log($"[UnityIAPBridge] Apple restore result: {result}");
                    RestoreOwnedFromReceipts();
                    OnRestored?.Invoke();
                });
                return;
            }

            // Google Play and other stores restore non-consumables via receipts at init.
            RestoreOwnedFromReceipts();
            OnRestored?.Invoke();
        }

        private void RestoreOwnedFromReceipts()
        {
            if (_storeController == null)
                return;

            var ownedIds = new List<string>();
            foreach (var product in IAPManager.Catalog)
            {
                if (product.Type != IAPProductType.NonConsumable)
                    continue;

                var storeProduct = _storeController.products.WithID(product.ProductId);
                if (storeProduct != null && storeProduct.hasReceipt)
                    ownedIds.Add(product.ProductId);
            }

            if (ownedIds.Count > 0)
                _iapManager.RestorePurchases(ownedIds.ToArray());
        }
    }
#else
    public sealed class UnityIAPBridge
    {
        public bool IsInitialized => false;

#pragma warning disable CS0067
        public event Action OnInitialized;
        public event Action<string> OnInitializeFailed;
        public event Action<string> OnPurchaseInitiated;
        public event Action OnRestored;
#pragma warning restore CS0067

        public UnityIAPBridge(IAPManager iapManager)
        {
            if (iapManager == null) throw new ArgumentNullException(nameof(iapManager));
            Debug.LogWarning("[UnityIAPBridge] UNITY_IAP not defined. IAP disabled.");
        }

        public void Initialize() { }

        public bool PurchaseProduct(string productId) => false;

        public void RestorePurchases() { }
    }
#endif
}
