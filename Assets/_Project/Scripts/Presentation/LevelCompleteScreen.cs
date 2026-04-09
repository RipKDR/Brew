using System;
using Brew.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Displayed when a level is won. Shows score tally, star rating, Essence earned,
    /// streak multiplier breakdown, double-reward ad button, and new potion reveal.
    /// </summary>
    public class LevelCompleteScreen : MonoBehaviour
    {
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _bonusText;
        [SerializeField] private GameObject[] _starObjects;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _replayButton;

        [Header("Economy")]
        [SerializeField] private Text _essenceEarnedText;
        [SerializeField] private Text _streakMultiplierText;
        [SerializeField] private Text _newPotionText;

        [Header("Ads")]
        [SerializeField] private Button _doubleRewardAdButton;
        [SerializeField] private Text _doubleRewardLabel;

        public event Action OnNextLevel;
        public event Action OnReplay;
        public event Action OnDoubleRewardAd;

        private void Awake()
        {
            if (_nextLevelButton != null)
                _nextLevelButton.onClick.AddListener(() => OnNextLevel?.Invoke());
            if (_replayButton != null)
                _replayButton.onClick.AddListener(() => OnReplay?.Invoke());
            if (_doubleRewardAdButton != null)
                _doubleRewardAdButton.onClick.AddListener(() => OnDoubleRewardAd?.Invoke());
        }

        public void Show(int totalScore, int bonus, int stars)
        {
            Show(totalScore, bonus, stars, 0, 1.0f, false, false);
        }

        public void Show(int totalScore, int bonus, int stars, int essenceEarned, float streakMultiplier,
            bool isNewPotion, bool canDoubleReward)
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

            if (_essenceEarnedText != null)
                _essenceEarnedText.text = essenceEarned > 0 ? $"+{essenceEarned} Essence" : "";

            if (_streakMultiplierText != null)
                _streakMultiplierText.text = streakMultiplier > 1.01f ? $"Streak {streakMultiplier:F1}x" : "";

            if (_newPotionText != null)
            {
                _newPotionText.gameObject.SetActive(isNewPotion);
                if (isNewPotion) _newPotionText.text = "New Potion Brewed!";
            }

            if (_doubleRewardAdButton != null)
                _doubleRewardAdButton.gameObject.SetActive(canDoubleReward);
        }

        public void HideDoubleRewardButton()
        {
            if (_doubleRewardAdButton != null)
                _doubleRewardAdButton.gameObject.SetActive(false);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
