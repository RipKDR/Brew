using System.Collections;
using UnityEngine;

namespace Brew.Presentation
{
    /// <summary>
    /// Token motion: idle shimmer, selection pulse, fusion rush. Uses <see cref="SpriteRenderer"/> on the same GameObject (or falls back to sibling lookup).
    /// </summary>
    public class TokenAnimator : MonoBehaviour
    {
        private const float IdlePeriod = 3f;
        private const float IdleScaleMin = 0.98f;
        private const float IdleScaleMax = 1.02f;
        private const float SelectedPulseDuration = 0.15f;
        private const float FusingDuration = 0.15f;

        [SerializeField] private AnimationCurve _easeIn = CreateDefaultEaseIn();
        [SerializeField] private AnimationCurve _selectedPulseCurve = CreateDefaultSelectedPulseCurve();

        private SpriteRenderer _spriteRenderer;
        private Vector3 _baseScale = Vector3.one;
        private float _idleStagger;
        private bool _idleActive;
        private Coroutine _pulseRoutine;
        private Coroutine _rushRoutine;
        private Color _spriteBaseColor = Color.white;

        private static AnimationCurve CreateDefaultEaseIn()
        {
            // Ease-in (approx. quadratic): slow start, full speed at end
            return new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(1f, 1f, 2f, 0f));
        }

        private static AnimationCurve CreateDefaultSelectedPulseCurve()
        {
            // Ease-out-back style: overshoot to ~1.08, settle at 1.05 (normalized scale factor keys)
            return new AnimationCurve(
                new Keyframe(0f, 1f, 0f, 2.4f),
                new Keyframe(0.45f, 1.08f, 0f, 0f),
                new Keyframe(1f, 1.05f, -1.8f, 0f));
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInParent<SpriteRenderer>();

            if (_spriteRenderer != null)
                _spriteBaseColor = _spriteRenderer.color;

            _baseScale = transform.localScale;

            if (_easeIn == null || _easeIn.length == 0)
                _easeIn = CreateDefaultEaseIn();
            if (_selectedPulseCurve == null || _selectedPulseCurve.length == 0)
                _selectedPulseCurve = CreateDefaultSelectedPulseCurve();
        }

        private void Update()
        {
            if (_rushRoutine != null || _pulseRoutine != null)
                return;

            if (!_idleActive)
                return;

            float t = Time.time + _idleStagger;
            float phase = (Mathf.PI * 2f / IdlePeriod) * t;
            float s = Mathf.Lerp(IdleScaleMin, IdleScaleMax, (Mathf.Sin(phase) + 1f) * 0.5f);
            transform.localScale = _baseScale * s;
        }

        /// <summary>Begin idle scale shimmer with phase offset (radians-friendly input).</summary>
        public void PlayIdleShimmer(float staggerOffset)
        {
            _idleStagger = staggerOffset;
            _baseScale = transform.localScale;
            _idleActive = true;
        }

        public void PlaySelectedPulse()
        {
            if (_pulseRoutine != null)
                StopCoroutine(_pulseRoutine);
            _pulseRoutine = StartCoroutine(SelectedPulseRoutine());
        }

        public Coroutine PlayFusingRush(Vector3 targetPosition, System.Action onComplete = null)
        {
            if (_rushRoutine != null)
                StopCoroutine(_rushRoutine);
            _rushRoutine = StartCoroutine(FusingRushRoutine(targetPosition, onComplete));
            return _rushRoutine;
        }

        public void StopAllAnimations()
        {
            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
                _pulseRoutine = null;
            }

            if (_rushRoutine != null)
            {
                StopCoroutine(_rushRoutine);
                _rushRoutine = null;
            }

            _idleActive = false;
            transform.localScale = _baseScale;

            if (_spriteRenderer != null)
                _spriteRenderer.color = _spriteBaseColor;
        }

        /// <summary>Sync logical base scale after board / pool init.</summary>
        public void SetBaseScale(Vector3 scale)
        {
            _baseScale = scale;
            if (_pulseRoutine == null && _rushRoutine == null)
                transform.localScale = scale;
        }

        private IEnumerator SelectedPulseRoutine()
        {
            _idleActive = false;
            Vector3 logicalBase = _baseScale;
            float elapsed = 0f;

            while (elapsed < SelectedPulseDuration)
            {
                float u = elapsed / SelectedPulseDuration;
                float factor = _selectedPulseCurve.Evaluate(u);
                transform.localScale = logicalBase * factor;
                elapsed += Time.deltaTime;
                yield return null;
            }

            float endFactor = _selectedPulseCurve.Evaluate(1f);
            transform.localScale = logicalBase * endFactor;
            _baseScale = logicalBase * endFactor;
            _pulseRoutine = null;
            _idleActive = true;
        }

        private IEnumerator FusingRushRoutine(Vector3 targetPosition, System.Action onComplete)
        {
            _idleActive = false;
            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
                _pulseRoutine = null;
            }

            Vector3 startPos = transform.position;
            Vector3 travel = targetPosition - startPos;
            Vector3 dir = travel.sqrMagnitude > 0.0001f ? travel.normalized : Vector3.right;
            Vector3 startScale = _baseScale;
            Color colorBefore = _spriteRenderer != null ? _spriteRenderer.color : Color.white;

            float elapsed = 0f;
            while (elapsed < FusingDuration)
            {
                float u = _easeIn.Evaluate(elapsed / FusingDuration);
                transform.position = Vector3.Lerp(startPos, targetPosition, u);

                float stretch = 1f + 0.1f * u;
                float squash = 1f / stretch;
                float ax = Mathf.Abs(dir.x);
                float ay = Mathf.Abs(dir.y);
                Vector3 stretchScale = ax >= ay
                    ? new Vector3(startScale.x * stretch, startScale.y * squash, startScale.z)
                    : new Vector3(startScale.x * squash, startScale.y * stretch, startScale.z);
                transform.localScale = stretchScale;

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition;
            transform.localScale = startScale;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.white;
                yield return null;
                _spriteRenderer.color = colorBefore;
            }

            onComplete?.Invoke();
            _rushRoutine = null;
            transform.localScale = startScale;
            _baseScale = startScale;
            _idleActive = true;
        }
    }
}
