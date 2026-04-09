using System.Collections;
using Brew.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    public class HudController : MonoBehaviour
    {
        [Header("Move Counter")]
        [SerializeField] private Text _moveCountText;
        [SerializeField] private Color _normalMoveColor = Color.white;
        [SerializeField] private Color _lowMoveColor = new(0.85f, 0.22f, 0.27f);
        [SerializeField] private Color _criticalMoveColor = new(0.85f, 0.07f, 0.07f);

        [Header("Score")]
        [SerializeField] private Text _scoreText;

        [Header("Recipe Vials")]
        [SerializeField] private Transform _vialContainer;
        [SerializeField] private RecipeVialUI _vialPrefab;

        [Header("Economy")]
        [SerializeField] private Text _essenceText;
        [SerializeField] private Text _gemsText;
        [SerializeField] private Text _streakText;
        [SerializeField] private GameObject _streakFlameIcon;

        private MoveTracker _moveTracker;
        private ScoreCalculator _scoreCalculator;
        private RecipeTracker _recipeTracker;
        private RecipeVialUI[] _vialInstances;

        private int _displayedScore;
        private Coroutine _scoreTweenCoroutine;
        private Coroutine _movePulseCoroutine;

        public void Initialize(MoveTracker moveTracker, ScoreCalculator scoreCalculator, RecipeTracker recipeTracker)
        {
            Cleanup();

            _moveTracker = moveTracker;
            _scoreCalculator = scoreCalculator;
            _recipeTracker = recipeTracker;

            _moveTracker.OnMovesChanged += UpdateMoveDisplay;
            _scoreCalculator.OnScoreChanged += UpdateScoreDisplay;
            _recipeTracker.OnRecipeProgressChanged += UpdateVialDisplay;

            _displayedScore = 0;
            UpdateMoveDisplay(_moveTracker.MovesRemaining);
            UpdateScoreDisplay(_scoreCalculator.TotalScore);
            BuildVials();
        }

        public void SetMoveCounterVisible(bool visible)
        {
            if (_moveCountText != null)
                _moveCountText.gameObject.SetActive(visible);
        }

        private void OnDestroy() => Cleanup();

        private void Cleanup()
        {
            if (_moveTracker != null) _moveTracker.OnMovesChanged -= UpdateMoveDisplay;
            if (_scoreCalculator != null) _scoreCalculator.OnScoreChanged -= UpdateScoreDisplay;
            if (_recipeTracker != null) _recipeTracker.OnRecipeProgressChanged -= UpdateVialDisplay;
        }

        private void UpdateMoveDisplay(int movesRemaining)
        {
            if (_moveCountText == null) return;

            _moveCountText.text = movesRemaining.ToString();

            if (movesRemaining <= 1)
            {
                _moveCountText.color = _criticalMoveColor;
                TriggerMovePulse(1.4f);
            }
            else if (movesRemaining <= 5)
            {
                _moveCountText.color = _lowMoveColor;
                TriggerMovePulse(1.2f);
            }
            else
            {
                _moveCountText.color = _normalMoveColor;
            }
        }

        private void TriggerMovePulse(float targetScale)
        {
            if (_movePulseCoroutine != null)
                StopCoroutine(_movePulseCoroutine);
            _movePulseCoroutine = StartCoroutine(PulseTransform(_moveCountText.rectTransform, targetScale, 0.15f));
        }

        private void UpdateScoreDisplay(int totalScore)
        {
            if (_scoreText == null) return;

            if (_scoreTweenCoroutine != null)
                StopCoroutine(_scoreTweenCoroutine);

            _scoreTweenCoroutine = StartCoroutine(TweenScore(_displayedScore, totalScore, 0.4f));
        }

        private IEnumerator TweenScore(int from, int to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t); // smoothstep
                int current = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
                _scoreText.text = current.ToString("N0");
                _displayedScore = current;
                yield return null;
            }

            _displayedScore = to;
            _scoreText.text = to.ToString("N0");
        }

        private IEnumerator PulseTransform(RectTransform rt, float targetScale, float duration)
        {
            if (rt == null) yield break;

            float half = duration * 0.5f;
            float elapsed = 0f;

            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / half;
                float scale = Mathf.Lerp(1f, targetScale, t);
                rt.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / half;
                float scale = Mathf.Lerp(targetScale, 1f, t);
                rt.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            rt.localScale = Vector3.one;
        }

        public void UpdateWalletDisplay(int essence, int gems)
        {
            if (_essenceText != null) _essenceText.text = essence.ToString("N0");
            if (_gemsText != null) _gemsText.text = gems.ToString("N0");
        }

        public void UpdateStreakDisplay(int streak, float multiplier)
        {
            if (_streakFlameIcon != null) _streakFlameIcon.SetActive(streak > 0);
            if (_streakText != null)
                _streakText.text = streak > 0 ? $"x{multiplier:F1}" : "";
        }

        private void UpdateVialDisplay(IngredientColor color, int remaining)
        {
            if (_vialInstances == null) return;

            var progress = _recipeTracker.CurrentProgress;
            for (int i = 0; i < _vialInstances.Length && i < progress.Count; i++)
            {
                _vialInstances[i].UpdateProgress(progress[i]);
            }
        }

        private void BuildVials()
        {
            if (_vialContainer == null || _vialPrefab == null) return;

            foreach (Transform child in _vialContainer)
                Destroy(child.gameObject);

            var progress = _recipeTracker.CurrentProgress;
            _vialInstances = new RecipeVialUI[progress.Count];

            for (int i = 0; i < progress.Count; i++)
            {
                var vial = Instantiate(_vialPrefab, _vialContainer);
                vial.Setup(progress[i]);
                _vialInstances[i] = vial;
            }
        }
    }
}
