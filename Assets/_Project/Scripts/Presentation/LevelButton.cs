using System;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Individual level button in the level select screen.
    /// </summary>
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private Text _levelNumberText;
        [SerializeField] private GameObject[] _starObjects;
        [SerializeField] private GameObject _lockIcon;
        [SerializeField] private Button _button;

        public void Setup(int levelId, bool unlocked, int stars, Action onClick)
        {
            if (_levelNumberText != null)
                _levelNumberText.text = levelId.ToString();

            if (_lockIcon != null)
                _lockIcon.SetActive(!unlocked);

            if (_button != null)
            {
                _button.interactable = unlocked;
                _button.onClick.RemoveAllListeners();
                if (onClick != null)
                    _button.onClick.AddListener(() => onClick());
            }

            if (_starObjects != null)
            {
                for (int i = 0; i < _starObjects.Length; i++)
                {
                    if (_starObjects[i] != null)
                        _starObjects[i].SetActive(unlocked && i < stars);
                }
            }
        }
    }
}
