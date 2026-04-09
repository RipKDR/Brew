using System;
using Brew.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Displayed when a level is won. Shows score tally, star rating, and next level button.
    /// </summary>
    public class LevelCompleteScreen : MonoBehaviour
    {
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _bonusText;
        [SerializeField] private GameObject[] _starObjects;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _replayButton;

        public event Action OnNextLevel;
        public event Action OnReplay;

        private void Awake()
        {
            if (_nextLevelButton != null)
                _nextLevelButton.onClick.AddListener(() => OnNextLevel?.Invoke());
            if (_replayButton != null)
                _replayButton.onClick.AddListener(() => OnReplay?.Invoke());

            gameObject.SetActive(false);
        }

        public void Show(int totalScore, int bonus, int stars)
        {
            gameObject.SetActive(true);

            if (_scoreText != null)
                _scoreText.text = totalScore.ToString("N0");

            if (_bonusText != null)
                _bonusText.text = bonus > 0 ? $"+{bonus} Move Bonus" : "";

            if (_starObjects != null)
            {
                for (int i = 0; i < _starObjects.Length; i++)
                {
                    if (_starObjects[i] != null)
                        _starObjects[i].SetActive(i < stars);
                }
            }
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
