using Brew.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// UI element for a single recipe vial. Shows the ingredient color,
    /// required count, and fill progress.
    /// </summary>
    public class RecipeVialUI : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private Text _countText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _completeStamp;

        private int _required;

        public void Setup(RecipeProgress progress)
        {
            _required = progress.Required;

            if (_iconImage != null)
                _iconImage.color = BoardPresenter.GetDisplayColor(progress.Color);

            if (_completeStamp != null)
                _completeStamp.SetActive(false);

            UpdateProgress(progress);
        }

        public void UpdateProgress(RecipeProgress progress)
        {
            if (_countText != null)
                _countText.text = progress.IsFilled ? "0" : progress.Remaining.ToString();

            if (_fillImage != null)
            {
                float fill = _required > 0 ? 1f - ((float)progress.Remaining / _required) : 1f;
                _fillImage.fillAmount = fill;
                _fillImage.color = BoardPresenter.GetDisplayColor(progress.Color);
            }

            if (_completeStamp != null)
                _completeStamp.SetActive(progress.IsFilled);
        }
    }
}
