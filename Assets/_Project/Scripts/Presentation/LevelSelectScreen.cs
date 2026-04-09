using System;
using Brew.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Scrollable grid of level buttons. Shows lock/unlock state and star counts.
    /// </summary>
    public class LevelSelectScreen : MonoBehaviour
    {
        [SerializeField] private Transform _levelButtonContainer;
        [SerializeField] private LevelButton _levelButtonPrefab;
        [SerializeField] private int _totalLevels = 40;

        private PlayerProgress _progress;

        public event Action<int> OnLevelSelected;

        public void Initialize(PlayerProgress progress)
        {
            _progress = progress;
            BuildButtons();
        }

        public void Refresh()
        {
            if (_progress != null)
                BuildButtons();
        }

        private void BuildButtons()
        {
            foreach (Transform child in _levelButtonContainer)
                Destroy(child.gameObject);

            for (int i = 1; i <= _totalLevels; i++)
            {
                var button = Instantiate(_levelButtonPrefab, _levelButtonContainer);
                bool unlocked = _progress.IsLevelUnlocked(i);
                int stars = _progress.GetStars(i);

                int levelId = i;
                button.Setup(levelId, unlocked, stars, () =>
                {
                    if (unlocked)
                        OnLevelSelected?.Invoke(levelId);
                });
            }
        }
    }
}
