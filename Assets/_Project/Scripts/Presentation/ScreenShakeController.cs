using System.Collections.Generic;
using Brew.Data;
using UnityEngine;

namespace Brew.Presentation
{
    /// <summary>
    /// Perlin-noise camera shake with stacking (max envelope of active shakes). Uses unscaled time.
    /// </summary>
    public class ScreenShakeController : MonoBehaviour
    {
        private const float NoiseFrequency = 24f;
        private const float PositionScale = 0.01f;

        [SerializeField] private ScreenShakeConfigSO _config;
        [SerializeField] private Transform _cameraTransform;

        [SerializeField] private bool _enabled = true;

        private readonly List<ShakeInstance> _shakes = new();
        private Vector3 _originalLocalPosition;
        private float _noiseSeedX;
        private float _noiseSeedY;

        /// <summary>Local enable flag; also respects <see cref="SettingsManager.ScreenShakeEnabled"/>.</summary>
        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        private void Awake()
        {
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;

            if (_cameraTransform != null)
            {
                _originalLocalPosition = _cameraTransform.localPosition;
                _noiseSeedX = Random.Range(0f, 1000f);
                _noiseSeedY = Random.Range(0f, 1000f);
            }
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null) return;

            float now = Time.unscaledTime;
            for (int i = _shakes.Count - 1; i >= 0; i--)
            {
                if (now >= _shakes[i].EndUnscaled)
                    _shakes.RemoveAt(i);
            }

            if (!IsShakeAllowed() || _shakes.Count == 0)
            {
                _cameraTransform.localPosition = _originalLocalPosition;
                return;
            }

            float envelope = ComputeStackedEnvelope(now);
            float nx = now * NoiseFrequency;
            float ny = _noiseSeedX + now * NoiseFrequency;
            float ox = (Mathf.PerlinNoise(nx, _noiseSeedY) - 0.5f) * 2f * envelope * PositionScale;
            float oy = (Mathf.PerlinNoise(_noiseSeedY, ny) - 0.5f) * 2f * envelope * PositionScale;

            _cameraTransform.localPosition = _originalLocalPosition + new Vector3(ox, oy, 0f);
        }

        public void Shake(float intensity, float duration)
        {
            if (_cameraTransform == null) return;
            if (!IsShakeAllowed()) return;

            if (_shakes.Count == 0)
                _originalLocalPosition = _cameraTransform.localPosition;

            _shakes.Add(new ShakeInstance(Time.unscaledTime, duration, intensity));
        }

        public void ShakeFusion()
        {
            if (_config == null) return;
            Shake(_config.Fusion.Intensity, _config.Fusion.Duration);
        }

        public void ShakeChain(int chainLength)
        {
            if (_config == null) return;
            int steps = Mathf.Max(0, chainLength);
            float intensity = _config.Chain.Intensity + 0.2f * steps;
            Shake(intensity, _config.Chain.Duration);
        }

        public void ShakeBrew()
        {
            if (_config == null) return;
            Shake(_config.Brew.Intensity, _config.Brew.Duration);
        }

        public void ShakeLevelComplete()
        {
            if (_config == null) return;
            Shake(_config.LevelComplete.Intensity, _config.LevelComplete.Duration);
        }

        private bool IsShakeAllowed() => _enabled && SettingsManager.ScreenShakeEnabled;

        private float ComputeStackedEnvelope(float now)
        {
            float max = 0f;
            for (int i = 0; i < _shakes.Count; i++)
            {
                ShakeInstance s = _shakes[i];
                float remaining = s.EndUnscaled - now;
                if (remaining <= 0f) continue;
                float linear = Mathf.Clamp01(remaining / Mathf.Max(0.0001f, s.Duration));
                float contribution = s.Intensity * linear;
                if (contribution > max)
                    max = contribution;
            }

            return max;
        }

        private struct ShakeInstance
        {
            public readonly float StartUnscaled;
            public readonly float Duration;
            public readonly float Intensity;
            public readonly float EndUnscaled;

            public ShakeInstance(float startUnscaled, float duration, float intensity)
            {
                StartUnscaled = startUnscaled;
                Duration = Mathf.Max(0.0001f, duration);
                Intensity = intensity;
                EndUnscaled = startUnscaled + Duration;
            }
        }
    }
}
