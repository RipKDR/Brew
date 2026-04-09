using System;
using Brew.Core.LiveOps;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation.LiveOps
{
    public class WeeklyEventView : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _eventBannerPanel;
        [SerializeField] private GameObject _eventLevelSelectPanel;
        [SerializeField] private GameObject _eventResultsPanel;

        [Header("Timer")]
        [SerializeField] private Text _timerText;

        [Header("Levels")]
        [SerializeField] private Button[] _levelButtons;
        [SerializeField] private Text[] _rewardTexts;

        public event Action<int> OnEventLevelSelected;
        public event Action OnClaimRewards;

        private WeeklyEventManager _manager;
        private long _endTimestamp;
        private bool _initialized;

        public void Initialize(WeeklyEventManager manager)
        {
            _manager = manager;
            _initialized = true;

            for (int i = 0; i < _levelButtons.Length; i++)
            {
                int index = i;
                if (_levelButtons[i] != null)
                    _levelButtons[i].onClick.AddListener(() => HandleLevelButton(index));
            }

            Refresh();
        }

        public void SetEndTimestamp(long endTimestamp)
        {
            _endTimestamp = endTimestamp;
        }

        private void Update()
        {
            if (!_initialized || _timerText == null) return;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long remaining = _endTimestamp - now;

            if (remaining <= 0)
            {
                _timerText.text = "Event Ended";
                return;
            }

            int hours = (int)(remaining / 3600);
            int minutes = (int)((remaining % 3600) / 60);
            int seconds = (int)(remaining % 60);
            _timerText.text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        public void Refresh()
        {
            if (_manager == null) return;

            for (int i = 0; i < _levelButtons.Length; i++)
            {
                if (_levelButtons[i] == null) continue;

                if (_manager.IsLevelCompleted(i))
                {
                    _levelButtons[i].interactable = false;
                    SetButtonColor(_levelButtons[i], new Color(0.4f, 0.8f, 0.4f));
                }
                else
                {
                    long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    bool expired = now >= _endTimestamp;
                    _levelButtons[i].interactable = !expired;
                    SetButtonColor(_levelButtons[i], expired
                        ? new Color(0.5f, 0.5f, 0.5f)
                        : new Color(0.91f, 0.64f, 0.29f));
                }
            }
        }

        public void RefreshState(EventState state)
        {
            HideBanner();
            HideLevelSelect();
            HideResults();

            switch (state)
            {
                case EventState.Active:
                    ShowBanner();
                    Refresh();
                    break;
                case EventState.Completed:
                    ShowBanner();
                    ShowResults();
                    break;
            }
        }

        public void ShowBanner()
        {
            if (_eventBannerPanel != null) _eventBannerPanel.SetActive(true);
        }

        public void HideBanner()
        {
            if (_eventBannerPanel != null) _eventBannerPanel.SetActive(false);
        }

        public void ShowLevelSelect()
        {
            if (_eventLevelSelectPanel != null) _eventLevelSelectPanel.SetActive(true);
            Refresh();
        }

        public void HideLevelSelect()
        {
            if (_eventLevelSelectPanel != null) _eventLevelSelectPanel.SetActive(false);
        }

        public void ShowResults()
        {
            if (_eventResultsPanel != null) _eventResultsPanel.SetActive(true);
        }

        public void HideResults()
        {
            if (_eventResultsPanel != null) _eventResultsPanel.SetActive(false);
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void HandleLevelButton(int index)
        {
            OnEventLevelSelected?.Invoke(index);
        }

        public void HandleClaimRewards()
        {
            OnClaimRewards?.Invoke();
        }

        private static void SetButtonColor(Button button, Color color)
        {
            var image = button.GetComponent<Image>();
            if (image != null) image.color = color;
        }

        private void OnDestroy()
        {
            if (_levelButtons == null) return;
            foreach (var btn in _levelButtons)
            {
                if (btn != null) btn.onClick.RemoveAllListeners();
            }
        }
    }
}
