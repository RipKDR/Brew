using Brew.Core;
using Brew.Core.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Booster purchase panel allowing purchase with Essence or Gems.
    /// Connects CurrencyManager spending to BoosterManager charge grants.
    /// </summary>
    public class BoosterShopUI : MonoBehaviour
    {
        [Header("Shake")]
        [SerializeField] private Button _buyShakeEssenceButton;
        [SerializeField] private Button _buyShakeGemButton;
        [SerializeField] private Text _shakeEssenceCostText;
        [SerializeField] private Text _shakeGemCostText;

        [Header("Catalyst")]
        [SerializeField] private Button _buyCatalystEssenceButton;
        [SerializeField] private Button _buyCatalystGemButton;
        [SerializeField] private Text _catalystEssenceCostText;
        [SerializeField] private Text _catalystGemCostText;

        [Header("Extra Moves")]
        [SerializeField] private Button _buyExtraMovesGemButton;
        [SerializeField] private Text _extraMovesGemCostText;

        private CurrencyManager _currency;
        private BoosterManager _boosters;
        private int _shakeEssence, _catalystEssence;
        private int _shakeGem, _catalystGem, _extraMovesGem;

        public void Initialize(CurrencyManager currency, BoosterManager boosters,
            int shakeEssenceCost, int catalystEssenceCost,
            int shakeGemCost, int catalystGemCost, int extraMovesGemCost)
        {
            _currency = currency;
            _boosters = boosters;
            _shakeEssence = shakeEssenceCost;
            _catalystEssence = catalystEssenceCost;
            _shakeGem = shakeGemCost;
            _catalystGem = catalystGemCost;
            _extraMovesGem = extraMovesGemCost;

            WireButton(_buyShakeEssenceButton, () => TryBuy(CurrencyType.Essence, _shakeEssence, BoosterType.Shake, "booster_shake_essence"));
            WireButton(_buyShakeGemButton, () => TryBuy(CurrencyType.Gems, _shakeGem, BoosterType.Shake, "booster_shake_gem"));
            WireButton(_buyCatalystEssenceButton, () => TryBuy(CurrencyType.Essence, _catalystEssence, BoosterType.Catalyst, "booster_catalyst_essence"));
            WireButton(_buyCatalystGemButton, () => TryBuy(CurrencyType.Gems, _catalystGem, BoosterType.Catalyst, "booster_catalyst_gem"));
            WireButton(_buyExtraMovesGemButton, () => TryBuy(CurrencyType.Gems, _extraMovesGem, BoosterType.ExtraMoves, "booster_extra_moves_gem"));

            RefreshCostLabels();
        }

        private void RefreshCostLabels()
        {
            SetText(_shakeEssenceCostText, $"{_shakeEssence} Essence");
            SetText(_shakeGemCostText, $"{_shakeGem} Gems");
            SetText(_catalystEssenceCostText, $"{_catalystEssence} Essence");
            SetText(_catalystGemCostText, $"{_catalystGem} Gems");
            SetText(_extraMovesGemCostText, $"{_extraMovesGem} Gems");
        }

        private void TryBuy(CurrencyType type, int cost, BoosterType booster, string sink)
        {
            if (_currency.Spend(type, cost, sink))
                _boosters.AddCharges(booster, 1);
        }

        private static void WireButton(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn != null) btn.onClick.AddListener(action);
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
