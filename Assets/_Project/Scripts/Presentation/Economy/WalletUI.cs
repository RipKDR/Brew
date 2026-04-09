using Brew.Core.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Header display showing Essence and Gem balances. Updates in real-time via CurrencyManager events.
    /// </summary>
    public class WalletUI : MonoBehaviour
    {
        [SerializeField] private Text _essenceText;
        [SerializeField] private Text _gemsText;
        [SerializeField] private Button _addGemsButton;

        private CurrencyManager _currencyManager;

        public event System.Action OnAddGemsClicked;

        public void Initialize(CurrencyManager currencyManager)
        {
            _currencyManager = currencyManager;
            _currencyManager.OnBalanceChanged += OnBalanceChanged;

            if (_addGemsButton != null)
                _addGemsButton.onClick.AddListener(() => OnAddGemsClicked?.Invoke());

            Refresh();
        }

        private void OnDestroy()
        {
            if (_currencyManager != null)
                _currencyManager.OnBalanceChanged -= OnBalanceChanged;
        }

        public void Refresh()
        {
            if (_currencyManager == null) return;

            if (_essenceText != null)
                _essenceText.text = _currencyManager.GetBalance(CurrencyType.Essence).ToString("N0");
            if (_gemsText != null)
                _gemsText.text = _currencyManager.GetBalance(CurrencyType.Gems).ToString("N0");
        }

        private void OnBalanceChanged(CurrencyType type, int newBalance) => Refresh();
    }
}
