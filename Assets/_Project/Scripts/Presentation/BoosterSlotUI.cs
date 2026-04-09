using System;
using Brew.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Single booster button: charge label, optional icon, armed highlight, tap callback.
    /// </summary>
    public class BoosterSlotUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _chargeText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _armedHighlight;

        private BoosterType _type;
        private Action<BoosterType> _onClicked;

        public void Setup(BoosterType type, int charges, Action<BoosterType> onClicked)
        {
            _type = type;
            _onClicked = onClicked;

            UpdateCharges(charges);
            SetArmed(false);

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => _onClicked?.Invoke(_type));
            }
        }

        public void UpdateCharges(int charges)
        {
            if (_chargeText != null)
                _chargeText.text = charges == int.MaxValue ? "\u221E" : charges.ToString();

            if (_button != null)
                _button.interactable = charges > 0;
        }

        public void SetArmed(bool armed)
        {
            if (_armedHighlight != null)
                _armedHighlight.SetActive(armed);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}
