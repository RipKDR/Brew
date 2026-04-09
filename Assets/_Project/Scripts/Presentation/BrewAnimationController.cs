using System;
using System.Collections;
using Brew.Core;
using UnityEngine;

namespace Brew.Presentation
{
    public class BrewAnimationController : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float _ignitionDuration = 0.3f;
        [SerializeField] private float _eruptionDuration = 0.4f;
        [SerializeField] private float _formationDuration = 0.5f;
        [SerializeField] private float _celebrationDuration = 0.6f;

        [Header("Animation Curves")]
        [SerializeField] private AnimationCurve _contractCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.6f);
        [SerializeField] private AnimationCurve _expandCurve = AnimationCurve.EaseInOut(0, 0.6f, 1, 1.5f);
        [SerializeField] private AnimationCurve _settleCurve = AnimationCurve.EaseInOut(0, 1.5f, 1, 1f);
        [SerializeField] private AnimationCurve _floatUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("References")]
        [SerializeField] private SpriteRenderer _flashOverlay;

        private ParticleManager _particleManager;
        private ScreenShakeController _screenShake;
        private bool _isAnimating;

        public bool IsAnimating => _isAnimating;
        public float TotalDuration => _ignitionDuration + _eruptionDuration + _formationDuration + _celebrationDuration;

        public event Action OnBrewAnimationStarted;
        public event Action OnBrewAnimationCompleted;

        public void Initialize(ParticleManager particleManager, ScreenShakeController screenShake)
        {
            _particleManager = particleManager;
            _screenShake = screenShake;
        }

        public Coroutine PlayBrewAnimation(Transform orbTransform, IngredientColor color, Vector3 vialTargetPosition)
        {
            if (orbTransform == null) return null;
            return StartCoroutine(BrewSequence(orbTransform, color, vialTargetPosition));
        }

        private IEnumerator BrewSequence(Transform orbTransform, IngredientColor color, Vector3 vialTarget)
        {
            _isAnimating = true;
            OnBrewAnimationStarted?.Invoke();

            var displayColor = BoardPresenter.GetDisplayColor(color);
            var startPos = orbTransform.position;
            var startScale = orbTransform.localScale;

            yield return PhaseIgnition(orbTransform, startScale, displayColor);
            yield return PhaseEruption(orbTransform, startScale, startPos, displayColor);
            yield return PhaseFormation(orbTransform, startScale, displayColor);
            yield return PhaseCelebration(orbTransform, startPos, vialTarget, displayColor);

            _isAnimating = false;
            OnBrewAnimationCompleted?.Invoke();
        }

        private IEnumerator PhaseIgnition(Transform orbTransform, Vector3 startScale, Color color)
        {
            HapticManager.MediumImpact();

            float elapsed = 0f;
            while (elapsed < _ignitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _ignitionDuration);
                float scale = _contractCurve.Evaluate(t);
                orbTransform.localScale = startScale * scale;
                yield return null;
            }
        }

        private IEnumerator PhaseEruption(Transform orbTransform, Vector3 startScale, Vector3 position, Color color)
        {
            HapticManager.HeavyImpact();

            if (_screenShake != null)
                _screenShake.ShakeBrew();

            if (_particleManager != null)
                _particleManager.PlayBrewBurst(position, color);

            if (_flashOverlay != null)
            {
                _flashOverlay.enabled = true;
                _flashOverlay.color = new Color(1, 1, 1, 0.8f);
            }

            float elapsed = 0f;
            while (elapsed < _eruptionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _eruptionDuration);
                float scale = _expandCurve.Evaluate(t);
                orbTransform.localScale = startScale * scale;

                if (_flashOverlay != null)
                {
                    var c = _flashOverlay.color;
                    c.a = Mathf.Lerp(0.8f, 0f, t);
                    _flashOverlay.color = c;
                }

                yield return null;
            }

            if (_flashOverlay != null)
                _flashOverlay.enabled = false;
        }

        private IEnumerator PhaseFormation(Transform orbTransform, Vector3 startScale, Color color)
        {
            float elapsed = 0f;
            while (elapsed < _formationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _formationDuration);
                float scale = _settleCurve.Evaluate(t);
                orbTransform.localScale = startScale * scale;
                yield return null;
            }
        }

        private IEnumerator PhaseCelebration(Transform orbTransform, Vector3 startPos, Vector3 vialTarget, Color color)
        {
            HapticManager.SuccessPattern();

            if (_particleManager != null)
                _particleManager.PlayFusionSparkles(startPos, color);

            var sr = orbTransform.GetComponent<SpriteRenderer>();
            float startAlpha = sr != null ? sr.color.a : 1f;

            float elapsed = 0f;
            while (elapsed < _celebrationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _celebrationDuration);
                float curveT = _floatUpCurve.Evaluate(t);

                orbTransform.position = Vector3.Lerp(startPos, vialTarget, curveT);

                float scaleFactor = Mathf.Lerp(1f, 0.3f, t);
                orbTransform.localScale = orbTransform.localScale.normalized * scaleFactor;

                orbTransform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 4) * 5f);

                if (sr != null)
                {
                    var c = sr.color;
                    c.a = Mathf.Lerp(startAlpha, 0f, t * t);
                    sr.color = c;
                }

                yield return null;
            }
        }
    }
}
