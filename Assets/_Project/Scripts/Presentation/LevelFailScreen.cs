using System;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Displayed when moves are exhausted and recipe is not complete.
    /// Shows retry button. Ad-for-extra-moves placeholder for Week 8.
    /// </summary>
    public class LevelFailScreen : MonoBehaviour
    {
        [SerializeField] private Text _messageText;
        [SerializeField] private Button _retryButton;

        public event Action OnRetry;

        private void Awake()
        {
            if (_retryButton != null)
                _retryButton.onClick.AddListener(() => OnRetry?.Invoke());

            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);

            if (_messageText != null)
                _messageText.text = "Out of Moves!";
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
