using System.Collections;
using System.Collections.Generic;
using Brew.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    public class TutorialOverlay : MonoBehaviour
    {
        [SerializeField] private Image _dimPanel;
        [SerializeField] private RectTransform _glowRingPrefab;
        [SerializeField] private RectTransform _fingerIndicator;
        [SerializeField] private Text _textBubble;
        [SerializeField] private RectTransform _textBubbleContainer;
        [SerializeField] private Canvas _parentCanvas;

        private readonly List<RectTransform> _activeGlowRings = new();
        private Coroutine _fingerAnimCoroutine;
        private Coroutine _glowPulseCoroutine;

        private Camera _worldCamera;
        private float _cellSize;
        private float _cellSpacing;
        private Vector3 _boardOrigin;

        public void Initialize(Camera worldCamera, float cellSize, float cellSpacing, Vector3 boardOrigin)
        {
            _worldCamera = worldCamera;
            _cellSize = cellSize;
            _cellSpacing = cellSpacing;
            _boardOrigin = boardOrigin;
        }

        public void ShowDim()
        {
            if (_dimPanel != null)
            {
                _dimPanel.gameObject.SetActive(true);
                _dimPanel.color = new Color(0, 0, 0, 0.7f);
            }
        }

        public void HideDim()
        {
            if (_dimPanel != null)
                _dimPanel.gameObject.SetActive(false);
        }

        public void HighlightCells(GridCoord[] cells)
        {
            ClearHighlights();
            if (cells == null || cells.Length == 0) return;

            foreach (var coord in cells)
            {
                var worldPos = GridToWorld(coord);
                var ring = CreateGlowRing(worldPos);
                _activeGlowRings.Add(ring);
            }

            _glowPulseCoroutine = StartCoroutine(PulseGlowRings());
        }

        public void ClearHighlights()
        {
            if (_glowPulseCoroutine != null)
            {
                StopCoroutine(_glowPulseCoroutine);
                _glowPulseCoroutine = null;
            }

            foreach (var ring in _activeGlowRings)
            {
                if (ring != null)
                    Destroy(ring.gameObject);
            }
            _activeGlowRings.Clear();
        }

        public void ShowFingerIndicator(GridCoord cell)
        {
            if (_fingerIndicator == null) return;

            var worldPos = GridToWorld(cell);
            _fingerIndicator.gameObject.SetActive(true);
            PositionUIAtWorld(_fingerIndicator, worldPos);

            _fingerAnimCoroutine = StartCoroutine(AnimateFingerTap());
        }

        public void HideFingerIndicator()
        {
            if (_fingerAnimCoroutine != null)
            {
                StopCoroutine(_fingerAnimCoroutine);
                _fingerAnimCoroutine = null;
            }

            if (_fingerIndicator != null)
                _fingerIndicator.gameObject.SetActive(false);
        }

        public void ShowText(string text)
        {
            if (_textBubble == null) return;
            _textBubbleContainer.gameObject.SetActive(true);
            _textBubble.text = text ?? "";
        }

        public void HideText()
        {
            if (_textBubbleContainer != null)
                _textBubbleContainer.gameObject.SetActive(false);
        }

        public void ShowCelebrationText(string text)
        {
            if (_textBubble == null) return;
            _textBubbleContainer.gameObject.SetActive(true);
            _textBubble.text = text ?? "";
            _textBubble.fontSize = 36;
            _textBubble.fontStyle = FontStyle.Bold;
            StartCoroutine(HideCelebrationAfterDelay(1.5f));
        }

        public void HideAll()
        {
            HideDim();
            ClearHighlights();
            HideFingerIndicator();
            HideText();
        }

        private IEnumerator HideCelebrationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideText();
            if (_textBubble != null)
            {
                _textBubble.fontSize = 22;
                _textBubble.fontStyle = FontStyle.Normal;
            }
        }

        private Vector3 GridToWorld(GridCoord coord)
        {
            float stride = _cellSize + _cellSpacing;
            return new Vector3(
                _boardOrigin.x + coord.Col * stride,
                _boardOrigin.y - coord.Row * stride,
                0f);
        }

        private void PositionUIAtWorld(RectTransform uiElement, Vector3 worldPos)
        {
            if (_worldCamera == null || _parentCanvas == null) return;

            var screenPos = _worldCamera.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvas.GetComponent<RectTransform>(),
                screenPos,
                _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _worldCamera,
                out var localPoint);
            uiElement.anchoredPosition = localPoint;
        }

        private RectTransform CreateGlowRing(Vector3 worldPos)
        {
            RectTransform ring;
            if (_glowRingPrefab != null)
            {
                ring = Instantiate(_glowRingPrefab, transform);
            }
            else
            {
                var go = new GameObject("GlowRing", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                ring = go.GetComponent<RectTransform>();
                ring.sizeDelta = new Vector2(80, 80);
                var img = go.GetComponent<Image>();
                img.color = new Color(1f, 0.85f, 0.2f, 0.5f);
                img.raycastTarget = false;
            }

            PositionUIAtWorld(ring, worldPos);
            return ring;
        }

        private IEnumerator PulseGlowRings()
        {
            float time = 0f;
            while (true)
            {
                time += Time.deltaTime;
                float alpha = 0.3f + 0.3f * Mathf.Sin(time * 3f);
                float scale = 1f + 0.08f * Mathf.Sin(time * 3f);

                foreach (var ring in _activeGlowRings)
                {
                    if (ring == null) continue;
                    var img = ring.GetComponent<Image>();
                    if (img != null)
                    {
                        var c = img.color;
                        c.a = alpha;
                        img.color = c;
                    }
                    ring.localScale = new Vector3(scale, scale, 1f);
                }
                yield return null;
            }
        }

        private IEnumerator AnimateFingerTap()
        {
            while (true)
            {
                _fingerIndicator.localScale = Vector3.one;
                float elapsed = 0f;
                float tapDuration = 0.15f;
                while (elapsed < tapDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / tapDuration;
                    float scale = Mathf.Lerp(1f, 0.85f, t);
                    _fingerIndicator.localScale = new Vector3(scale, scale, 1f);
                    yield return null;
                }

                elapsed = 0f;
                while (elapsed < tapDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / tapDuration;
                    float scale = Mathf.Lerp(0.85f, 1f, t);
                    _fingerIndicator.localScale = new Vector3(scale, scale, 1f);
                    yield return null;
                }

                yield return new WaitForSeconds(1.2f);
            }
        }
    }
}
