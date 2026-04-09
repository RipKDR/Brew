using Brew.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Displays move counter (top-left), recipe vials (top-right), and score (top-center).
    /// Subscribes to events from MoveTracker, ScoreCalculator, and RecipeTracker.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        [Header("Move Counter")]
        [SerializeField] private Text _moveCountText;
        [SerializeField] private Color _normalMoveColor = Color.white;
        [SerializeField] private Color _lowMoveColor = Color.red;

        [Header("Score")]
        [SerializeField] private Text _scoreText;

        [Header("Recipe Vials")]
        [SerializeField] private Transform _vialContainer;
        [SerializeField] private RecipeVialUI _vialPrefab;

        private MoveTracker _moveTracker;
        private ScoreCalculator _scoreCalculator;
        private RecipeTracker _recipeTracker;
        private RecipeVialUI[] _vialInstances;

        public void Initialize(MoveTracker moveTracker, ScoreCalculator scoreCalculator, RecipeTracker recipeTracker)
        {
            Cleanup();

            _moveTracker = moveTracker;
            _scoreCalculator = scoreCalculator;
            _recipeTracker = recipeTracker;

            _moveTracker.OnMovesChanged += UpdateMoveDisplay;
            _scoreCalculator.OnScoreChanged += UpdateScoreDisplay;
            _recipeTracker.OnRecipeProgressChanged += UpdateVialDisplay;

            UpdateMoveDisplay(_moveTracker.MovesRemaining);
            UpdateScoreDisplay(_scoreCalculator.TotalScore);
            BuildVials();
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
            _moveCountText.color = movesRemaining <= 3 ? _lowMoveColor : _normalMoveColor;
        }

        private void UpdateScoreDisplay(int totalScore)
        {
            if (_scoreText == null) return;
            _scoreText.text = totalScore.ToString("N0");
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
