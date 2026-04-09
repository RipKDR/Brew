using System;
using System.Collections;
using System.Collections.Generic;
using Brew.Core;
using Brew.Data;
using Brew.Utilities;
using UnityEngine;

namespace Brew.Presentation
{
    /// <summary>
    /// Visual representation of the board. Reads from BoardModel and manages
    /// TokenView GameObjects via object pooling. Drives animations for fusion,
    /// gravity, and brew events. Integrates MoveTracker, ScoreCalculator,
    /// RecipeTracker, and WinLoseEvaluator for full gameplay loop.
    /// </summary>
    public class BoardPresenter : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BoardConfigSO _boardConfig;
        [SerializeField] private TokenView _tokenPrefab;
        [SerializeField] private Transform _tokenContainer;

        [Header("Input")]
        [SerializeField] private InputController _inputController;

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
        private LevelConfig _currentLevel;

        private readonly Dictionary<GridCoord, TokenView> _activeViews = new();
        private Vector3 _boardOrigin;
        private bool _isResolving;

        public MoveTracker MoveTracker => _moveTracker;
        public ScoreCalculator ScoreCalculator => _scoreCalculator;
        public RecipeTracker RecipeTracker => _recipeTracker;

        public event Action OnResolutionComplete;
        public event Action<LevelOutcome> OnLevelOutcome;

        private void Start()
        {
            if (_currentLevel == null)
                InitializeGame();
        }

        /// <summary>
        /// Initializes using defaults from BoardConfigSO (backward-compatible).
        /// </summary>
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

        /// <summary>
        /// Initializes the board from a LevelConfig loaded via LevelLoader.
        /// </summary>
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

            _tokenPool = new ObjectPool<TokenView>(_tokenPrefab, _tokenContainer, _board.CellCount);

            CalculateBoardOrigin();
            _spawner.PopulateBoard(_board);
            RebuildAllViews();

            _inputController.Initialize(
                _stateMachine,
                _board.Width, _board.Height,
                _boardConfig.CellSize, _boardConfig.CellSpacing,
                _boardOrigin);

            _inputController.OnCellTapped += HandleCellTapped;

            _stateMachine.TransitionTo(BoardPhase.PlayerInput);
        }

        private void CleanupPreviousGame()
        {
            if (_inputController != null)
                _inputController.OnCellTapped -= HandleCellTapped;

            if (_activeViews != null)
            {
                foreach (var kv in _activeViews)
                    if (_tokenPool != null) _tokenPool.Return(kv.Value);
                _activeViews.Clear();
            }
        }

        private void OnDestroy()
        {
            if (_inputController != null)
                _inputController.OnCellTapped -= HandleCellTapped;
        }

        private void HandleCellTapped(GridCoord coord)
        {
            if (_isResolving || !_stateMachine.AcceptsInput)
                return;

            var result = _fusionEngine.TryPlayerFusion(coord);
            if (result == null)
                return;

            _moveTracker.TryConsumeMove();
            _scoreCalculator.ResetForNewMove();

            StartCoroutine(ResolveSequence(result));
        }

        private IEnumerator ResolveSequence(FusionResult initialFusion)
        {
            _isResolving = true;
            _stateMachine.TransitionTo(BoardPhase.Fusing);

            _scoreCalculator.OnFusion(initialFusion.ConsumedCells.Count);
            HandleBrewIfTriggered(initialFusion);

            yield return AnimateFusion(initialFusion);

            int cascadeWave = 0;
            int maxCascadeWaves = _boardConfig.MaxCascadeWaves;

            while (cascadeWave < maxCascadeWaves)
            {
                _stateMachine.TransitionTo(BoardPhase.Cascading);

                ResolveChainFusions();

                var (drops, spawns) = _cascadeResolver.ApplyGravityAndRefill();
                yield return AnimateGravity(drops, spawns);

                _stateMachine.TransitionTo(BoardPhase.Settling);
                yield return new WaitForSeconds(0.05f);

                ResolveChainFusions();
                RebuildAllViews();

                var cascadeClusters = _clusterDetector.FindAllClusters(_boardConfig.MinClusterSize);
                if (cascadeClusters.Count == 0)
                    break;

                cascadeWave++;
                _scoreCalculator.OnCascadeWave();

                SortClustersBottomToTop(cascadeClusters);
                foreach (var cluster in cascadeClusters)
                {
                    _stateMachine.TransitionTo(BoardPhase.CheckBrew);
                    var cascadeFusion = _fusionEngine.ExecuteCascadeFusion(cluster);
                    if (cascadeFusion != null)
                    {
                        _scoreCalculator.OnFusion(cascadeFusion.ConsumedCells.Count);
                        HandleBrewIfTriggered(cascadeFusion);
                        yield return AnimateFusion(cascadeFusion);
                    }

                    ResetStateMachineForNextCascade();
                }
            }

            EnsurePhase(BoardPhase.CheckBrew);
            _stateMachine.TransitionTo(BoardPhase.CheckWin);

            var outcome = _winLoseEvaluator.Evaluate();
            if (outcome == LevelOutcome.Win)
                _scoreCalculator.CalculateEndOfLevelBonus(_moveTracker.MovesRemaining);

            RebuildAllViews();

            _stateMachine.TransitionTo(BoardPhase.Idle);

            if (outcome == LevelOutcome.InProgress)
            {
                _stateMachine.TransitionTo(BoardPhase.PlayerInput);
            }

            _isResolving = false;
            OnResolutionComplete?.Invoke();

            if (outcome != LevelOutcome.InProgress)
                OnLevelOutcome?.Invoke(outcome);
        }

        private void HandleBrewIfTriggered(FusionResult fusion)
        {
            if (!fusion.TriggeredBrew)
                return;

            var color = fusion.CreatedOrb.Color;
            bool isTarget = _recipeTracker.IsTargetColor(color);
            _scoreCalculator.OnBrew(isTarget);
            _recipeTracker.OnBrew(color);
            _board.SetCell(fusion.OrbCell, CellContent.Empty);
        }

        private void ResolveChainFusions()
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

                    if (chainResult.TriggeredBrew)
                    {
                        var color = _board.GetCell(chainResult.SurvivorCell).Color;
                        bool isTarget = _recipeTracker.IsTargetColor(color);
                        _scoreCalculator.OnBrew(isTarget);
                        _recipeTracker.OnBrew(color);
                        _board.SetCell(chainResult.SurvivorCell, CellContent.Empty);
                    }
                }
                safety++;
            }
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

            if (_activeViews.TryGetValue(coord, out var existing))
            {
                existing.UpdateVisual(cell, GetDisplayColor(cell.Color));
                existing.transform.position = GridToWorld(coord);
                return;
            }

            var view = _tokenPool.Get();
            view.Initialize(coord, cell, GetDisplayColor(cell.Color), _boardConfig.CellSize);
            view.transform.position = GridToWorld(coord);
            view.SetSortingOrder(coord.Row * _board.Width + coord.Col);
            _activeViews[coord] = view;
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
            IngredientColor.Ember => new Color(0.878f, 0.251f, 0.251f),
            IngredientColor.Frost => new Color(0.251f, 0.502f, 0.878f),
            IngredientColor.Vine => new Color(0.251f, 0.690f, 0.251f),
            IngredientColor.Sun => new Color(0.878f, 0.753f, 0.125f),
            IngredientColor.Shadow => new Color(0.502f, 0.251f, 0.753f),
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
            while (_stateMachine.CurrentPhase != target)
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
