using Brew.Core.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Renders the workshop room. Shows current upgrade level, next upgrade button with cost,
    /// and a completion progress indicator.
    /// </summary>
    public class WorkshopView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Text _workshopTitleText;
        [SerializeField] private Text _nextUpgradeNameText;
        [SerializeField] private Text _nextUpgradeCostText;
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Text _progressText;
        [SerializeField] private GameObject _completedBadge;

        [Header("Visual Upgrades")]
        [SerializeField] private GameObject[] _upgradeDecorations;

        private WorkshopManager _manager;

        public void Initialize(WorkshopManager manager)
        {
            _manager = manager;
            _manager.OnUpgradePurchased += OnUpgradePurchased;

            if (_purchaseButton != null)
                _purchaseButton.onClick.AddListener(OnPurchaseClicked);

            Refresh();
        }

        private void OnDestroy()
        {
            if (_manager != null) _manager.OnUpgradePurchased -= OnUpgradePurchased;
        }

        public void Refresh()
        {
            if (_manager == null) return;

            bool maxed = _manager.CurrentLevel >= _manager.MaxLevel;

            if (_progressBar != null)
                _progressBar.value = _manager.CompletionPercent;

            if (_progressText != null)
                _progressText.text = $"{_manager.CurrentLevel} / {_manager.MaxLevel}";

            if (_completedBadge != null)
                _completedBadge.SetActive(maxed);

            if (maxed)
            {
                if (_nextUpgradeNameText != null) _nextUpgradeNameText.text = "Workshop Complete!";
                if (_nextUpgradeCostText != null) _nextUpgradeCostText.text = "";
                if (_purchaseButton != null) _purchaseButton.interactable = false;
            }
            else
            {
                if (_nextUpgradeNameText != null) _nextUpgradeNameText.text = _manager.GetNextUpgradeName();
                if (_nextUpgradeCostText != null) _nextUpgradeCostText.text = $"{_manager.GetNextUpgradeCost()} Essence";
                if (_purchaseButton != null) _purchaseButton.interactable = true;
            }

            UpdateDecorations();
        }

        private void UpdateDecorations()
        {
            if (_upgradeDecorations == null) return;
            for (int i = 0; i < _upgradeDecorations.Length; i++)
            {
                if (_upgradeDecorations[i] != null)
                    _upgradeDecorations[i].SetActive(i < _manager.CurrentLevel);
            }
        }

        private void OnPurchaseClicked()
        {
            if (_manager == null) return;
            _manager.TryPurchaseNext();
        }

        private void OnUpgradePurchased(int level, string name) => Refresh();

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
