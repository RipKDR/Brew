using System;
using UnityEngine;

namespace Brew.Data
{
    [CreateAssetMenu(fileName = "ScreenShakeConfig", menuName = "Brew/Config/Screen Shake Config")]
    public class ScreenShakeConfigSO : ScriptableObject
    {
        [Serializable]
        public struct ShakePreset
        {
            public float Intensity;
            public float Duration;
        }

        [SerializeField] private ShakePreset _fusionShake = new ShakePreset { Intensity = 0.5f, Duration = 0.1f };
        [SerializeField] private ShakePreset _chainShake = new ShakePreset { Intensity = 1.0f, Duration = 0.15f };
        [SerializeField] private ShakePreset _brewShake = new ShakePreset { Intensity = 2.0f, Duration = 0.3f };
        [SerializeField] private ShakePreset _levelCompleteShake = new ShakePreset { Intensity = 1.5f, Duration = 0.5f };

        public ShakePreset Fusion => _fusionShake;
        public ShakePreset Chain => _chainShake;
        public ShakePreset Brew => _brewShake;
        public ShakePreset LevelComplete => _levelCompleteShake;
    }
}
