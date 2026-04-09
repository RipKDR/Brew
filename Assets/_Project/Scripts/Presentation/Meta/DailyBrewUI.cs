using Brew.Core.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Main menu indicator for Daily Brew availability. Shows streak count and "Available" / "Completed" state.
    /// </summary>
    public class DailyBrewUI : MonoBehaviour
    {
        [SerializeField] private Button _dailyBrewButton;
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _streakText;
        [SerializeField] private GameObject _availableGlow;
        [SerializeField] private GameObject _completedBadge;

        private DailyBrewManager _manager;

        public event System.Action OnDailyBrewRequested;

        public void Initialize(DailyBrewManager manager)
        {
            _manager = manager;
            _manager.OnDailyBrewCompleted += OnCompleted;
            _manager.OnDailyStreakChanged += OnStreakChanged;

            if (_dailyBrewButton != null)
                _dailyBrewButton.onClick.AddListener(() => OnDailyBrewRequested?.Invoke());

            Refresh();
        }

        private void OnDestroy()
        {
            if (_manager != null)
            {
                _manager.OnDailyBrewCompleted -= OnCompleted;
                _manager.OnDailyStreakChanged -= OnStreakChanged;
            }
        }

        public void Refresh()
        {
            if (_manager == null) return;

            bool available = !_manager.IsCompletedToday;

            if (_availableGlow != null) _availableGlow.SetActive(available);
            if (_completedBadge != null) _completedBadge.SetActive(!available);
            if (_dailyBrewButton != null) _dailyBrewButton.interactable = available;

            if (_statusText != null)
                _statusText.text = available ? "Daily Brew Available!" : "Completed \u2713";

            if (_streakText != null)
                _streakText.text = _manager.CurrentDailyStreak > 0
                    ? $"Streak: {_manager.CurrentDailyStreak} days"
                    : "";
        }

        private void OnCompleted(int essence, int gems) => Refresh();
        private void OnStreakChanged(int streak) => Refresh();

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
