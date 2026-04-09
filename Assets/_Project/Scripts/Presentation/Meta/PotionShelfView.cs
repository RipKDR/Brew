using Brew.Core.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Scrollable grid displaying all potions. Locked slots show silhouettes; brewed slots glow.
    /// Milestone progress bar at the top.
    /// </summary>
    public class PotionShelfView : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private GameObject _potionSlotPrefab;
        [SerializeField] private ScrollRect _scrollRect;

        [Header("Milestone")]
        [SerializeField] private Slider _milestoneProgressBar;
        [SerializeField] private Text _milestoneText;

        [Header("Visual")]
        [SerializeField] private Color _lockedColor = new(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color _unlockedColor = Color.white;

        private PotionShelfManager _manager;

        public void Initialize(PotionShelfManager manager)
        {
            _manager = manager;
            _manager.OnPotionUnlocked += OnPotionUnlocked;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_manager != null) _manager.OnPotionUnlocked -= OnPotionUnlocked;
        }

        public void Refresh()
        {
            if (_manager == null) return;

            UpdateMilestoneBar();
            RebuildGrid();
        }

        private void RebuildGrid()
        {
            if (_gridContainer == null || _potionSlotPrefab == null) return;

            foreach (Transform child in _gridContainer)
                Destroy(child.gameObject);

            for (int i = 1; i <= _manager.TotalPotions; i++)
            {
                var slot = Instantiate(_potionSlotPrefab, _gridContainer);
                bool unlocked = _manager.HasPotion(i);

                var image = slot.GetComponent<Image>();
                if (image != null)
                    image.color = unlocked ? _unlockedColor : _lockedColor;

                var label = slot.GetComponentInChildren<Text>();
                if (label != null)
                    label.text = unlocked ? $"L{i}" : $"{i}";
            }
        }

        private void UpdateMilestoneBar()
        {
            if (_milestoneProgressBar != null)
                _milestoneProgressBar.value = _manager.CompletionPercent;

            if (_milestoneText != null)
                _milestoneText.text = $"{_manager.BrewedCount} / {_manager.TotalPotions} Potions";
        }

        private void OnPotionUnlocked(int levelId)
        {
            UpdateMilestoneBar();
            RebuildGrid();
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
