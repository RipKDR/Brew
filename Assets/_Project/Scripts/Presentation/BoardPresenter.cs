using System;
using System.Collections;
using System.Collections.Generic;
using Brew.Core;
using Brew.Data;
using Brew.Utilities;
using UnityEngine;

namespace Brew.Presentation
{
    public class BoardPresenter : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BoardConfigSO _boardConfig;
        [SerializeField] private TokenView _tokenPrefab;
        [SerializeField] private Transform _tokenContainer;

        [Header("Input")]
        [SerializeField] private InputController _inputController;

        [Header("Juice (Optional)")]
        [SerializeField] private ScreenShakeController _screenShake;
        [SerializeField] private ParticleManager _particleManager;
        [SerializeField] private BrewAnimationController _brewAnimator;
        [SerializeField] private TutorialController _tutorialController;

        private BoardModel _board;
        private BoardStateMachine _stateMachine;
        private ClusterDetector _clusterDetector;
        private FusionEngine _fusionEngine;
        private CascadeResolver _cascadeResolver;
        private TokenSpawner _spawner;
        private ObjectPool<TokenView> _tokenPool;

        private MoveTracker _moveTracker;
        private ScoreCalculator _scoreCalculator;
        private RecipeTracker _recipeTracker;
        private WinLoseEvaluator _winLoseEvaluator;
        private BoosterManager _boosterManager;
        private LevelConfig _currentLevel;

        private readonly Dictionary<GridCoord, TokenView> _activeViews = new();
        private Vector3 _boardOrigin;
        private bool _isResolving;
        private bool _catalystArmed;

        public MoveTracker MoveTracker => _moveTracker;
        public ScoreCalculator ScoreCalculator => _scoreCalculator;
        public RecipeTracker RecipeTracker => _recipeTracker;
        public BoosterManager BoosterManager => _boosterManager;
        public BoardModel Board => _board;
        public ClusterDetector ClusterDetector => _clusterDetector;
        public TokenSpawner Spawner => _spawner;
        public int MinClusterSize => _boardConfig.MinClusterSize;

        public event Action OnResolutionComplete;
        public event Action<LevelOutcome> OnLevelOutcome;

        private void Start()
        {
            if (_currentLevel == null)
                InitializeGame();
        }

        public void InitializeGame()
        {
            var defaultConfig = new LevelConfig(
                0,
                _boardConfig.Width,
                _boardConfig.Height,
                _boardConfig.AvailableColors,
                new[] { new RecipeTarget(IngredientColor.Ember, 1) },
                99,
                new[] { 500, 1500, 3000 },
                false);

            InitializeWithLevel(defaultConfig);
        }

        public void InitializeWithLevel(LevelConfig levelConfig)
        {
            _currentLevel = levelConfig ?? throw new ArgumentNullException(nameof(levelConfig));

            CleanupPreviousGame();

            var rng = new System.Random();
            var colors = new IngredientColor[levelConfig.IngredientPool.Count];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = levelConfig.IngredientPool[i];

            _board = new BoardModel(levelConfig.GridWidth, levelConfig.GridHeight);
            _stateMachine = new BoardStateMachine();
            _clusterDetector = new ClusterDetector(_board);
            _spawner = new TokenSpawner(rng, colors);
            _fusionEngine = new FusionEngine(
                _board, _clusterDetector,
                _boardConfig.MinClusterSize, _boardConfig.BrewThreshold);
            _cascadeResolver = new CascadeResolver(_board, _spawner);

            _moveTracker = new MoveTracker(levelConfig.MoveLimit);
            _scoreCalculator = new ScoreCalculator();
            _recipeTracker = new RecipeTracker(levelConfig.RecipeTargets);
            _winLoseEvaluator = new WinLoseEvaluator(_recipeTracker, _moveTracker);
            _boosterManager = new BoosterManager();

            _tokenPool = new ObjectPool<TokenView>(_tokenPrefab, _tokenContainer, _board.CellCount);

            CalculateBoardOrigin();
            _spawner.PopulateBoard(_board);
            PlaceStoneBlockers(levelConfig);
            RebuildAllViews();

            _inputController.Initialize(
                _stateMachine,
                _board.Width, _board.Height,
                _boardConfig.CellSize, _boardConfig.CellSpacing,
                _boardOrigin);

            _inputController.OnCellTapped += HandleCellTapped;

            _stateMachine.TransitionTo(BoardPhase.PlayerInput);

            if (_tutorialController != null && levelConfig.IsTutorial)
                _tutorialController.StartTutorial(levelConfig.LevelId);
        }

        private void CleanupPreviousGame()
        {
            if (_inputController != null)
                _inputController.OnCellTapped -= HandleCellTapped;

            if (_brewAnimator != null)
                _brewAnimator.StopBrewAnimation();

            StopAllCoroutines();
            _isResolving = false;

            if (_activeViews != null)
            {
                foreach (var kv in _activeViews)
                    if (_tokenPool != null) _tokenPool.Return(kv.Value);
                _activeViews.Clear();
            }

            _catalystArmed = false;
        }

        private void OnDestroy()
        {
            if (_inputController != null)
                _inputController.OnCellTapped -= HandleCellTapped;
        }

        public void ArmCatalyst() => _catalystArmed = true;
        public void DisarmCatalyst() => _catalystArmed = false;

        public void ActivateShake()
        {
            if (_isResolving || _board == null) return;

            _boosterManager.ActivateShake(_board, _spawner, _clusterDetector, _boardConfig.MinClusterSize);
            RebuildAllViews();

            if (_screenShake != null) _screenShake.ShakeFusion();
            HapticManager.MediumImpact();
        }

        public void ActivateExtraMoves(int amount)
        {
            if (_moveTracker == null || amount <= 0) return;
            _boosterManager.ActivateExtraMoves(_moveTracker, amount);
            HapticManager.LightImpact();
        }

        private void HandleCellTapped(GridCoord coord)
        {
            if (_isResolving || !_stateMachine.AcceptsInput)
                return;

            if (_tutorialController != null && _tutorialController.IsActive)
            {
                if (!_tutorialController.IsValidTutorialTap(coord))
                    return;
                _tutorialController.NotifyTapPerformed();
            }

            if (_catalystArmed)
            {
                HandleCatalystTap(coord);
                return;
            }

            var result = _fusionEngine.TryPlayerFusion(coord);
            if (result == null)
            {
                HapticManager.SelectionImpact();
                return;
            }

            HapticManager.MediumImpact();
            _moveTracker.TryConsumeMove();
            _scoreCalculator.ResetForNewMove();

            StartCoroutine(ResolveSequence(result));
        }

        private void HandleCatalystTap(GridCoord coord)
        {
            _catalystArmed = false;

            if (!_boosterManager.ActivateCatalyst(_board, coord))
            {
                HapticManager.SelectionImpact();
                return;
            }

            HapticManager.MediumImpact();

            RebuildAllViews();
        }

        private IEnumerator ResolveSequence(FusionResult initialFusion)
        {
            _isResolving = true;
            _stateMachine.TransitionTo(BoardPhase.Fusing);

            _scoreCalculator.OnFusion(initialFusion.ConsumedCells.Count);

            if (_screenShake != null) _screenShake.ShakeFusion();
            if (_particleManager != null)
            {
                var fusionPos = GridToWorld(initialFusion.OrbCell);
                var fusionColor = GetDisplayColor(initialFusion.CreatedOrb.Color);
                _particleManager.PlayFusionSparkles(fusionPos, fusionColor);
            }

            yield return AnimateFusion(initialFusion);

            if (initialFusion.TriggeredBrew)
                yield return HandleBrewSequence(initialFusion);

            int cascadeWave = 0;
            int maxCascadeWaves = _boardConfig.MaxCascadeWaves;

            while (cascadeWave < maxCascadeWaves)
            {
                _stateMachine.TransitionTo(BoardPhase.Cascading);

                yield return ResolveChainFusionsAnimated();

                var (drops, spawns) = _cascadeResolver.ApplyGravityAndRefill();
                yield return AnimateGravity(drops, spawns);

                _stateMachine.TransitionTo(BoardPhase.Settling);
                yield return new WaitForSeconds(0.05f);

                yield return ResolveChainFusionsAnimated();
                RebuildAllViews();

                var cascadeClusters = _clusterDetector.FindAllClusters(_boardConfig.MinClusterSize);
                if (cascadeClusters.Count == 0)
                    break;

                cascadeWave++;
                _scoreCalculator.OnCascadeWave();

                if (_screenShake != null) _screenShake.ShakeChain(cascadeWave);

                SortClustersBottomToTop(cascadeClusters);
                foreach (var cluster in cascadeClusters)
                {
                    _stateMachine.TransitionTo(BoardPhase.CheckBrew);
                    var cascadeFusion = _fusionEngine.ExecuteCascadeFusion(cluster);
                    if (cascadeFusion != null)
                    {
                        _scoreCalculator.OnFusion(cascadeFusion.ConsumedCells.Count);

                        if (_particleManager != null)
                        {
                            var pos = GridToWorld(cascadeFusion.OrbCell);
                            var col = GetDisplayColor(cascadeFusion.CreatedOrb.Color);
                            _particleManager.PlayFusionSparkles(pos, col);
                        }

                        if (cascadeFusion.TriggeredBrew)
                            yield return HandleBrewSequence(cascadeFusion);
                        else
                            yield return AnimateFusion(cascadeFusion);
                    }

                    ResetStateMachineForNextCascade();
                }
            }

            EnsurePhase(BoardPhase.CheckBrew);
            _stateMachine.TransitionTo(BoardPhase.CheckWin);

            var outcome = _winLoseEvaluator.Evaluate();
            if (outcome == LevelOutcome.Win)
            {
                _scoreCalculator.CalculateEndOfLevelBonus(_moveTracker.MovesRemaining);

                if (_screenShake != null) _screenShake.ShakeLevelComplete();
                if (_particleManager != null)
                {
                    int stars = ScoreCalculator.CalculateStars(
                        _scoreCalculator.TotalScore, _currentLevel.StarThresholds);
                    _particleManager.PlayLevelCompleteConfetti(stars);
                }
                HapticManager.SuccessPattern();
            }

            RebuildAllViews();

            _stateMachine.TransitionTo(BoardPhase.Idle);

            if (outcome == LevelOutcome.InProgress)
            {
                if (_spawner.IsDeadlocked(_board))
                {
                    _spawner.TryRecoverDeadlock(_board);
                    RebuildAllViews();
                }

                _stateMachine.TransitionTo(BoardPhase.PlayerInput);
            }

            _isResolving = false;
            OnResolutionComplete?.Invoke();

            if (outcome != LevelOutcome.InProgress)
                OnLevelOutcome?.Invoke(outcome);
        }

        private IEnumerator HandleBrewSequence(FusionResult fusion)
        {
            var color = fusion.CreatedOrb.Color;
            bool isTarget = _recipeTracker.IsTargetColor(color);
            _scoreCalculator.OnBrew(isTarget);
            _recipeTracker.OnBrew(color);

            if (_brewAnimator != null && _activeViews.TryGetValue(fusion.OrbCell, out var orbView))
            {
                yield return _brewAnimator.PlayBrewAnimation(
                    orbView.transform, color, Vector3.up * 5f);
            }
            else
            {
                yield return AnimateFusion(fusion);
            }

            StoneClearer.ClearAdjacentStones(_board, fusion.OrbCell);
            _board.SetCell(fusion.OrbCell, CellContent.Empty);
            RebuildAllViews();
        }

        private IEnumerator ResolveChainFusionsAnimated()
        {
            int safety = 0;
            while (safety < 100)
            {
                var orbGroups = _clusterDetector.FindAllOrbChainGroups();
                if (orbGroups.Count == 0)
                    break;

                foreach (var group in orbGroups)
                {
                    var chainResult = _fusionEngine.ExecuteChainFusion(group);
                    if (chainResult == null) continue;

                    _scoreCalculator.OnChainStep();
                    HapticManager.MediumImpact();

                    if (_particleManager != null)
                    {
                        var pos = GridToWorld(chainResult.SurvivorCell);
                        var col = GetDisplayColor(_board.GetCell(chainResult.SurvivorCell).Color);
                        _particleManager.PlayChainIndicator(pos, pos + Vector3.up * 0.5f, col);
                    }

                    if (chainResult.TriggeredBrew)
                    {
                        var brewCell = chainResult.SurvivorCell;
                        var color = _board.GetCell(brewCell).Color;
                        bool isTarget = _recipeTracker.IsTargetColor(color);
                        _scoreCalculator.OnBrew(isTarget);
                        _recipeTracker.OnBrew(color);
                        StoneClearer.ClearAdjacentStones(_board, brewCell);
                        _board.SetCell(brewCell, CellContent.Empty);
                    }
                }
                safety++;
            }

            yield return null;
        }

        private IEnumerator AnimateFusion(FusionResult fusion)
        {
            foreach (var consumed in fusion.ConsumedCells)
            {
                if (_activeViews.TryGetValue(consumed, out var view))
                {
                    _tokenPool.Return(view);
                    _activeViews.Remove(consumed);
                }
            }

            CreateOrUpdateView(fusion.OrbCell);
            yield return new WaitForSeconds(0.2f);
        }

        private IEnumerator AnimateGravity(List<GravityStep> drops, List<GravityStep> spawns)
        {
            RebuildAllViews();

            float maxDuration = 0f;
            foreach (var drop in drops)
            {
                float duration = drop.Distance * 0.05f + 0.03f;
                if (duration > maxDuration) maxDuration = duration;
            }
            foreach (var spawn in spawns)
            {
                float duration = spawn.Distance * 0.05f + 0.03f;
                if (duration > maxDuration) maxDuration = duration;
            }

            if (maxDuration > 0)
                yield return new WaitForSeconds(maxDuration);
        }

        private void RebuildAllViews()
        {
            var staleCoords = new List<GridCoord>(_activeViews.Keys);
            foreach (var coord in staleCoords)
            {
                _tokenPool.Return(_activeViews[coord]);
            }
            _activeViews.Clear();

            for (int c = 0; c < _board.Width; c++)
            {
                for (int r = 0; r < _board.Height; r++)
                {
                    var coord = new GridCoord(c, r);
                    var cell = _board.GetCell(coord);
                    if (!cell.IsEmpty)
                        CreateOrUpdateView(coord);
                }
            }
        }

        private void CreateOrUpdateView(GridCoord coord)
        {
            var cell = _board.GetCell(coord);
            if (cell.IsEmpty) return;

            var displayColor = GetCellDisplayColor(cell);

            if (_activeViews.TryGetValue(coord, out var existing))
            {
                existing.UpdateVisual(cell, displayColor);
                existing.transform.position = GridToWorld(coord);
                return;
            }

            var view = _tokenPool.Get();
            view.Initialize(coord, cell, displayColor, _boardConfig.CellSize);
            view.transform.position = GridToWorld(coord);
            view.SetSortingOrder(coord.Row * _board.Width + coord.Col);
            _activeViews[coord] = view;
        }

        private void PlaceStoneBlockers(LevelConfig levelConfig)
        {
            foreach (var placement in levelConfig.BlockerPlacements)
            {
                if (!string.Equals(placement.Type, "stone", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                var coord = new GridCoord(placement.Col, placement.Row);
                if (!_board.InBounds(coord))
                    continue;

                _board.SetCell(coord, CellContent.Stone);
            }
        }

        private static Color GetCellDisplayColor(CellContent cell)
        {
            if (cell.IsStone)
                return new Color(0.45f, 0.45f, 0.48f); // grey stone placeholder
            return GetDisplayColor(cell.Color);
        }

        private void CalculateBoardOrigin()
        {
            float stride = _boardConfig.CellSize + _boardConfig.CellSpacing;
            float totalWidth = (_board.Width - 1) * stride;
            float totalHeight = (_board.Height - 1) * stride;
            _boardOrigin = new Vector3(-totalWidth / 2f, totalHeight / 2f, 0f);
        }

        private Vector3 GridToWorld(GridCoord coord)
        {
            float stride = _boardConfig.CellSize + _boardConfig.CellSpacing;
            return new Vector3(
                _boardOrigin.x + coord.Col * stride,
                _boardOrigin.y - coord.Row * stride,
                0f);
        }

        internal static Color GetDisplayColor(IngredientColor ingredient) => ingredient switch
        {
            IngredientColor.Ember => new Color(0.878f, 0.353f, 0.227f),   // #E05A3A
            IngredientColor.Frost => new Color(0.435f, 0.722f, 0.851f),   // #6FB8D9
            IngredientColor.Vine => new Color(0.427f, 0.686f, 0.369f),    // #6DAF5E
            IngredientColor.Sun => new Color(0.949f, 0.780f, 0.271f),     // #F2C745
            IngredientColor.Shadow => new Color(0.420f, 0.306f, 0.608f),  // #6B4E9B
            _ => Color.white
        };

        private void SortClustersBottomToTop(List<List<GridCoord>> clusters) => clusters.Sort((a, b) =>
        {
            var aBottom = FindBottomLeft(a);
            var bBottom = FindBottomLeft(b);
            int rowCmp = bBottom.Row.CompareTo(aBottom.Row);
            return rowCmp != 0 ? rowCmp : aBottom.Col.CompareTo(bBottom.Col);
        });

        private static GridCoord FindBottomLeft(List<GridCoord> cells)
        {
            var best = cells[0];
            for (int i = 1; i < cells.Count; i++)
            {
                var c = cells[i];
                if (c.Row > best.Row || (c.Row == best.Row && c.Col < best.Col))
                    best = c;
            }
            return best;
        }

        private void EnsurePhase(BoardPhase target)
        {
            int guard = 20;
            while (guard-- > 0 && _stateMachine.CurrentPhase != target)
            {
                _stateMachine.Advance();
            }
        }

        private void ResetStateMachineForNextCascade()
        {
            while (_stateMachine.CurrentPhase != BoardPhase.Cascading)
            {
                if (_stateMachine.CurrentPhase == BoardPhase.Idle)
                {
                    _stateMachine.TransitionTo(BoardPhase.PlayerInput);
                    _stateMachine.TransitionTo(BoardPhase.Fusing);
                    _stateMachine.TransitionTo(BoardPhase.Cascading);
                    break;
                }
                _stateMachine.Advance();
            }
        }
    }
}
