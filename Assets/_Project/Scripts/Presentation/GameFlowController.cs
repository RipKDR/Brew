using Brew.Core;
using Brew.Data;
using UnityEngine;

namespace Brew.Presentation
{
    /// <summary>
    /// Top-level controller that orchestrates the game flow:
    /// Level Select -> Gameplay -> Complete/Fail -> Level Select.
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private GameObject _levelSelectPanel;
        [SerializeField] private GameObject _gameplayPanel;
        [SerializeField] private LevelCompleteScreen _levelCompleteScreen;
        [SerializeField] private LevelFailScreen _levelFailScreen;

        [Header("Gameplay")]
        [SerializeField] private BoardPresenter _boardPresenter;
        [SerializeField] private HudController _hudController;
        [SerializeField] private LevelSelectScreen _levelSelectScreen;

        private PlayerProgress _progress;
        private LevelConfig _currentLevelConfig;

        private void Start()
        {
            _progress = LocalSaveManager.Load();

            _levelSelectScreen.Initialize(_progress);
            _levelSelectScreen.OnLevelSelected += StartLevel;

            _levelCompleteScreen.OnNextLevel += AdvanceToNextLevel;
            _levelCompleteScreen.OnReplay += ReplayCurrentLevel;
            _levelFailScreen.OnRetry += ReplayCurrentLevel;

            ShowLevelSelect();
        }

        private void OnDestroy()
        {
            if (_levelSelectScreen != null) _levelSelectScreen.OnLevelSelected -= StartLevel;
            if (_levelCompleteScreen != null)
            {
                _levelCompleteScreen.OnNextLevel -= AdvanceToNextLevel;
                _levelCompleteScreen.OnReplay -= ReplayCurrentLevel;
            }
            if (_levelFailScreen != null) _levelFailScreen.OnRetry -= ReplayCurrentLevel;
            if (_boardPresenter != null) _boardPresenter.OnLevelOutcome -= HandleLevelOutcome;
        }

        private void ShowLevelSelect()
        {
            _levelSelectPanel.SetActive(true);
            _gameplayPanel.SetActive(false);
            _levelCompleteScreen.Hide();
            _levelFailScreen.Hide();
            _levelSelectScreen.Refresh();
        }

        private void StartLevel(int levelId)
        {
            _currentLevelConfig = LevelLoader.LoadFromResources(levelId);

            _levelSelectPanel.SetActive(false);
            _gameplayPanel.SetActive(true);
            _levelCompleteScreen.Hide();
            _levelFailScreen.Hide();

            _boardPresenter.OnLevelOutcome -= HandleLevelOutcome;
            _boardPresenter.InitializeWithLevel(_currentLevelConfig);
            _boardPresenter.OnLevelOutcome += HandleLevelOutcome;

            _hudController.Initialize(
                _boardPresenter.MoveTracker,
                _boardPresenter.ScoreCalculator,
                _boardPresenter.RecipeTracker);
        }

        private void HandleLevelOutcome(LevelOutcome outcome)
        {
            _boardPresenter.OnLevelOutcome -= HandleLevelOutcome;

            switch (outcome)
            {
                case LevelOutcome.Win:
                    int score = _boardPresenter.ScoreCalculator.TotalScore;
                    int bonus = _boardPresenter.MoveTracker.MovesRemaining * 50;
                    int stars = ScoreCalculator.CalculateStars(score, _currentLevelConfig.StarThresholds);

                    _progress.RecordLevelComplete(_currentLevelConfig.LevelId, stars);
                    LocalSaveManager.Save(_progress);

                    _levelCompleteScreen.Show(score, bonus, stars);
                    break;

                case LevelOutcome.Lose:
                    _levelFailScreen.Show();
                    break;
            }
        }

        private void AdvanceToNextLevel()
        {
            _levelCompleteScreen.Hide();
            int nextLevelId = _currentLevelConfig.LevelId + 1;

            if (nextLevelId <= 40)
                StartLevel(nextLevelId);
            else
                ShowLevelSelect();
        }

        private void ReplayCurrentLevel()
        {
            _levelCompleteScreen.Hide();
            _levelFailScreen.Hide();
            StartLevel(_currentLevelConfig.LevelId);
        }
    }
}
