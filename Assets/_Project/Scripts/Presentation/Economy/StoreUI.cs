using System.Collections.Generic;
using Brew.Core.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// IAP store modal. Displays available gem packs, starter bundle, and no-ads pass.
    /// </summary>
    public class StoreUI : MonoBehaviour
    {
        [SerializeField] private Transform _productContainer;
        [SerializeField] private GameObject _productRowPrefab;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _restoreButton;

        private IAPManager _iapManager;
        private int _currentLevel;

        public event System.Action OnCloseRequested;
        public event System.Action OnRestoreRequested;

        public void Initialize(IAPManager iapManager, int currentLevel)
        {
            _iapManager = iapManager;
            _currentLevel = currentLevel;

            if (_closeButton != null)
                _closeButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
            if (_restoreButton != null)
                _restoreButton.onClick.AddListener(() => OnRestoreRequested?.Invoke());

            Refresh();
        }

        public void Refresh()
        {
            if (_iapManager == null || _productContainer == null || _productRowPrefab == null) return;

            foreach (Transform child in _productContainer)
                Destroy(child.gameObject);

            IReadOnlyList<IAPProduct> products = _iapManager.GetAvailableProducts(_currentLevel);
            foreach (var product in products)
            {
                var row = Instantiate(_productRowPrefab, _productContainer);

                var texts = row.GetComponentsInChildren<Text>();
                if (texts.Length >= 2)
                {
                    texts[0].text = FormatProductName(product);
                    texts[1].text = product.PriceDisplay;
                }

                var button = row.GetComponentInChildren<Button>();
                if (button != null)
                {
                    string pid = product.ProductId;
                    button.onClick.AddListener(() => _iapManager.CompletePurchase(pid));
                }
            }
        }

        private static string FormatProductName(IAPProduct product)
        {
            var parts = new List<string>();
            if (product.GemAmount > 0) parts.Add($"{product.GemAmount} Gems");
            if (product.EssenceAmount > 0) parts.Add($"{product.EssenceAmount} Essence");
            if (product.BoosterCount > 0) parts.Add($"{product.BoosterCount} Catalysts");

            string name = parts.Count > 0 ? string.Join(" + ", parts) : product.ProductId;
            if (!string.IsNullOrEmpty(product.BadgeText)) name += $" ({product.BadgeText})";
            return name;
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
