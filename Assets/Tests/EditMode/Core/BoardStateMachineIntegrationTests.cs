using System;
using System.Collections.Generic;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    /// <summary>
    /// Integration tests for the full board state machine cycle:
    /// Idle -> PlayerInput -> Fusing -> Cascading -> Settling -> CheckBrew -> CheckWin -> Idle
    ///
    /// These tests exercise all six pure-C# subsystems together (BoardStateMachine,
    /// BoardModel, FusionEngine, ClusterDetector, CascadeResolver, WinLoseEvaluator)
    /// using a deterministic seeded board so results are fully reproducible.
    ///
    /// Board layout used throughout (5×5, origin top-left, col/row 0-indexed):
    ///
    ///   Col:  0      1      2      3      4
    /// Row 0: [Em]   [Em]   [Em]   [Fr]   [Fr]
    /// Row 1: [Em]   [Fr]   [Vi]   [Fr]   [Vi]
    /// Row 2: [Vi]   [Vi]   [Vi]   [Sun]  [Sun]
    /// Row 3: [Fr]   [Sun]  [Sun]  [Sun]  [Vi]
    /// Row 4: [Fr]   [Fr]   [Sh]   [Sh]   [Sh]
    ///
    /// Deterministic clusters (size >= 3):
    ///   - Ember cluster: (0,0),(1,0),(2,0),(0,1) — size 4
    ///   - Frost cluster: (3,0),(4,0),(1,1),(3,1),(0,3),(1,4),(4,0)... see per-test notes
    ///   - Vine cluster: (0,2),(1,2),(2,2),(2,1),(4,1),(3,4)... see per-test notes
    ///   - Sun cluster: (3,2),(4,2),(1,3),(2,3),(3,3) — size 5
    ///   - Shadow cluster: (2,4),(3,4),(4,4) — size 3
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    public class BoardStateMachineIntegrationTests
    {
        // ------------------------------------------------------------------ constants

        private const int Width = 5;
        private const int Height = 5;
        private const int MinClusterSize = 3;
        private const int BrewThreshold = 3;   // orb brews if token_count >= 3
        private const int DefaultMoveLimit = 10;

        // ------------------------------------------------------------------ fields

        private BoardModel _board;
        private BoardStateMachine _sm;
        private ClusterDetector _detector;
        private FusionEngine _fusion;
        private CascadeResolver _cascade;
        private RecipeTracker _recipe;
        private MoveTracker _moves;
        private WinLoseEvaluator _evaluator;

        // ------------------------------------------------------------------ setup

        [SetUp]
        public void SetUp()
        {
            _board = new BoardModel(Width, Height);
            PlaceDeterministicBoard();

            _sm = new BoardStateMachine();
            _detector = new ClusterDetector(_board);

            var rng = new Random(42); // seeded — deterministic refills
            var spawner = new TokenSpawner(rng,
                new[] { IngredientColor.Ember, IngredientColor.Frost, IngredientColor.Vine,
                        IngredientColor.Sun,   IngredientColor.Shadow });

            _fusion = new FusionEngine(_board, _detector, MinClusterSize, BrewThreshold);
            _cascade = new CascadeResolver(_board, spawner);

            _recipe = new RecipeTracker(new List<RecipeTarget>
            {
                new RecipeTarget(IngredientColor.Ember, 1),
                new RecipeTarget(IngredientColor.Sun,   1),
            });

            _moves = new MoveTracker(DefaultMoveLimit);
            _evaluator = new WinLoseEvaluator(_recipe, _moves);
        }

        // ------------------------------------------------------------------ helpers

        /// <summary>
        /// Populates the board with a fixed, deterministic layout.
        /// </summary>
        private void PlaceDeterministicBoard()
        {
            // Row 0
            Set(0, 0, IngredientColor.Ember);
            Set(1, 0, IngredientColor.Ember);
            Set(2, 0, IngredientColor.Ember);
            Set(3, 0, IngredientColor.Frost);
            Set(4, 0, IngredientColor.Frost);

            // Row 1
            Set(0, 1, IngredientColor.Ember);
            Set(1, 1, IngredientColor.Frost);
            Set(2, 1, IngredientColor.Vine);
            Set(3, 1, IngredientColor.Frost);
            Set(4, 1, IngredientColor.Vine);

            // Row 2
            Set(0, 2, IngredientColor.Vine);
            Set(1, 2, IngredientColor.Vine);
            Set(2, 2, IngredientColor.Vine);
            Set(3, 2, IngredientColor.Sun);
            Set(4, 2, IngredientColor.Sun);

            // Row 3
            Set(0, 3, IngredientColor.Frost);
            Set(1, 3, IngredientColor.Sun);
            Set(2, 3, IngredientColor.Sun);
            Set(3, 3, IngredientColor.Sun);
            Set(4, 3, IngredientColor.Vine);

            // Row 4
            Set(0, 4, IngredientColor.Frost);
            Set(1, 4, IngredientColor.Frost);
            Set(2, 4, IngredientColor.Shadow);
            Set(3, 4, IngredientColor.Shadow);
            Set(4, 4, IngredientColor.Shadow);
        }

        private void Set(int col, int row, IngredientColor color) =>
            _board.SetCell(new GridCoord(col, row), CellContent.Token(color));

        /// <summary>
        /// Drives the state machine through one full resolution loop:
        /// PlayerInput -> Fusing -> Cascading -> Settling -> CheckBrew -> CheckWin -> Idle.
        /// Returns true if the evaluator reports Win or Lose, false if InProgress.
        /// </summary>
        private LevelOutcome RunFullCycle(GridCoord tapCell, bool consumeMove = true)
        {
            // Idle -> PlayerInput
            _sm.TransitionTo(BoardPhase.PlayerInput);
            Assert.IsTrue(_sm.AcceptsInput, "Should accept input during PlayerInput phase.");

            // PlayerInput: attempt fusion
            var result = _fusion.TryPlayerFusion(tapCell);
            Assert.IsNotNull(result, $"Expected a valid fusion at {tapCell}.");

            if (consumeMove)
                _moves.TryConsumeMove();

            // PlayerInput -> Fusing
            _sm.TransitionTo(BoardPhase.Fusing);

            // Fusing -> Cascading
            _sm.TransitionTo(BoardPhase.Cascading);

            // Cascading -> Settling
            _cascade.ApplyGravityAndRefill();
            _sm.TransitionTo(BoardPhase.Settling);

            // Settling -> CheckBrew
            _sm.TransitionTo(BoardPhase.CheckBrew);
            if (result.TriggeredBrew)
                _recipe.OnBrew(result.CreatedOrb.Color);

            // CheckBrew -> CheckWin
            _sm.TransitionTo(BoardPhase.CheckWin);
            var outcome = _evaluator.Evaluate();

            // CheckWin -> Idle
            _sm.TransitionTo(BoardPhase.Idle);

            return outcome;
        }

        // ------------------------------------------------------------------ tests

        [Test]
        public void TapValidCluster_TransitionsFromIdleToFusing()
        {
            // Arrange: board starts in Idle; Ember cluster at (0,0)-(2,0)-(0,1) is size 4.
            Assert.AreEqual(BoardPhase.Idle, _sm.CurrentPhase);

            // Act: advance to PlayerInput, issue tap on a known valid cluster
            _sm.TransitionTo(BoardPhase.PlayerInput);
            var result = _fusion.TryPlayerFusion(new GridCoord(0, 0));

            _sm.TransitionTo(BoardPhase.Fusing);

            // Assert
            Assert.IsNotNull(result, "Tap on 4-cell Ember cluster must yield a FusionResult.");
            Assert.AreEqual(BoardPhase.Fusing, _sm.CurrentPhase);
            Assert.IsFalse(_sm.AcceptsInput, "Input must be locked during Fusing phase.");
        }

        [Test]
        public void FusionComplete_TransitionsToSettling()
        {
            // Arrange
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _fusion.TryPlayerFusion(new GridCoord(0, 0)); // Ember cluster

            // Act: drive through Fusing -> Cascading -> Settling
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.TransitionTo(BoardPhase.Cascading);
            _cascade.ApplyGravityAndRefill();
            _sm.TransitionTo(BoardPhase.Settling);

            // Assert
            Assert.AreEqual(BoardPhase.Settling, _sm.CurrentPhase,
                "After gravity + refill the machine must be in Settling.");
        }

        [Test]
        public void AfterSettle_ChecksForBrew()
        {
            // Arrange: full pass up to Settling
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _fusion.TryPlayerFusion(new GridCoord(0, 0));
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.TransitionTo(BoardPhase.Cascading);
            _cascade.ApplyGravityAndRefill();
            _sm.TransitionTo(BoardPhase.Settling);

            // Act
            _sm.TransitionTo(BoardPhase.CheckBrew);

            // Assert
            Assert.AreEqual(BoardPhase.CheckBrew, _sm.CurrentPhase,
                "Settling must transition into CheckBrew, not skip it.");
        }

        [Test]
        public void RecipeComplete_TransitionsToWin()
        {
            // Arrange: recipe requires 1 Ember brew and 1 Sun brew.
            // Fuse the Ember cluster (size 4 >= BrewThreshold=3 -> TriggeredBrew=true).
            var emberResult = _fusion.TryPlayerFusion(new GridCoord(0, 0));
            Assert.IsNotNull(emberResult, "Setup: Ember cluster must fuse.");
            Assert.IsTrue(emberResult.TriggeredBrew, "Ember cluster of 4 must trigger a brew.");
            _recipe.OnBrew(emberResult.CreatedOrb.Color);

            // Re-place Sun cluster to ensure it is present after gravity shifted cells.
            // Sun tokens at (3,2),(4,2),(1,3),(2,3),(3,3) may have shifted; set them explicitly.
            _board.SetCell(new GridCoord(0, 3), CellContent.Token(IngredientColor.Sun));
            _board.SetCell(new GridCoord(1, 3), CellContent.Token(IngredientColor.Sun));
            _board.SetCell(new GridCoord(2, 3), CellContent.Token(IngredientColor.Sun));

            var sunResult = _fusion.TryPlayerFusion(new GridCoord(1, 3));
            Assert.IsNotNull(sunResult, "Setup: Sun cluster must fuse.");
            Assert.IsTrue(sunResult.TriggeredBrew, "Sun cluster of 3 must trigger a brew.");
            _recipe.OnBrew(sunResult.CreatedOrb.Color);

            // Drive the machine to CheckWin with both brews registered
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.TransitionTo(BoardPhase.Cascading);
            _sm.TransitionTo(BoardPhase.Settling);
            _sm.TransitionTo(BoardPhase.CheckBrew);
            _sm.TransitionTo(BoardPhase.CheckWin);

            // Assert
            var outcome = _evaluator.Evaluate();
            Assert.AreEqual(LevelOutcome.Win, outcome,
                "When all recipe targets are filled the evaluator must return Win.");
        }

        [Test]
        public void MovesExhausted_TriggersLose()
        {
            // Arrange: exhaust all moves without completing the recipe.
            for (int i = 0; i < DefaultMoveLimit; i++)
                _moves.TryConsumeMove();

            // Drive to CheckWin
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.TransitionTo(BoardPhase.Cascading);
            _sm.TransitionTo(BoardPhase.Settling);
            _sm.TransitionTo(BoardPhase.CheckBrew);
            _sm.TransitionTo(BoardPhase.CheckWin);

            // Assert
            Assert.IsTrue(_moves.IsExhausted, "MoveTracker must report exhausted.");
            Assert.IsFalse(_recipe.IsComplete, "Recipe must still be incomplete.");
            Assert.AreEqual(LevelOutcome.Lose, _evaluator.Evaluate());
        }

        [Test]
        public void InvalidTap_NoStateChange()
        {
            // Arrange: (2,4) is a Shadow token but Shadow cluster has exactly 3 cells.
            // Place only 2 Shadow tokens so the cluster is too small to fuse.
            _board.SetCell(new GridCoord(3, 4), CellContent.Empty); // break 3rd Shadow

            _sm.TransitionTo(BoardPhase.PlayerInput);

            // Act: tap an under-size Shadow cluster (now only 2 cells)
            var result = _fusion.TryPlayerFusion(new GridCoord(2, 4));

            // Assert: fusion rejected, machine stays in PlayerInput
            Assert.IsNull(result, "A cluster of size 2 must not yield a FusionResult.");
            Assert.AreEqual(BoardPhase.PlayerInput, _sm.CurrentPhase,
                "State must remain PlayerInput after a rejected tap.");
            Assert.IsTrue(_sm.AcceptsInput, "Input should still be accepted after an invalid tap.");
        }

        [Test]
        public void CascadeChain_IncrementsWaveCount()
        {
            // Arrange: record phase transitions to count cascade waves.
            int cascadeCount = 0;
            _sm.OnPhaseChanged += (from, to) =>
            {
                if (to == BoardPhase.Cascading)
                    cascadeCount++;
            };

            // Act: perform one full cycle which includes exactly one Cascading phase.
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _fusion.TryPlayerFusion(new GridCoord(0, 0)); // valid Ember fusion
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.TransitionTo(BoardPhase.Cascading);
            _cascade.ApplyGravityAndRefill();
            _sm.TransitionTo(BoardPhase.Settling);
            _sm.TransitionTo(BoardPhase.CheckBrew);
            _sm.TransitionTo(BoardPhase.CheckWin);
            _sm.TransitionTo(BoardPhase.Idle);

            // Assert
            Assert.AreEqual(1, cascadeCount,
                "One player move must produce exactly one Cascading phase transition.");
        }

        [Test]
        public void PhaseOrder_IsAlwaysEnforced_AcrossFullCycle()
        {
            // Records every phase in order to verify the strict sequence.
            var phases = new List<BoardPhase> { BoardPhase.Idle }; // start state
            _sm.OnPhaseChanged += (_, to) => phases.Add(to);

            // Run one full cycle
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _fusion.TryPlayerFusion(new GridCoord(0, 0));
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.TransitionTo(BoardPhase.Cascading);
            _cascade.ApplyGravityAndRefill();
            _sm.TransitionTo(BoardPhase.Settling);
            _sm.TransitionTo(BoardPhase.CheckBrew);
            _sm.TransitionTo(BoardPhase.CheckWin);
            _sm.TransitionTo(BoardPhase.Idle);

            var expected = new[]
            {
                BoardPhase.Idle,
                BoardPhase.PlayerInput,
                BoardPhase.Fusing,
                BoardPhase.Cascading,
                BoardPhase.Settling,
                BoardPhase.CheckBrew,
                BoardPhase.CheckWin,
                BoardPhase.Idle,
            };

            Assert.AreEqual(expected.Length, phases.Count,
                "Exactly 8 phases (including initial Idle) must be observed in one cycle.");

            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], phases[i],
                    $"Phase at index {i} must be {expected[i]} but was {phases[i]}.");
        }

        [Test]
        public void WinTakesPriorityOverLose_WhenFinalMoveCompletesRecipe()
        {
            // Per core-mechanic.md §9.3: if the last move triggers a brew that completes
            // the recipe, the outcome is Win, not Lose, even with 0 moves remaining.

            // Consume all moves but one
            for (int i = 0; i < DefaultMoveLimit - 1; i++)
                _moves.TryConsumeMove();

            // Consume the final move
            _moves.TryConsumeMove();
            Assert.IsTrue(_moves.IsExhausted, "All moves must be consumed.");

            // Simulate both recipe brews completing (Win condition)
            _recipe.OnBrew(IngredientColor.Ember);
            _recipe.OnBrew(IngredientColor.Sun);
            Assert.IsTrue(_recipe.IsComplete, "Recipe must be complete.");

            // Drive to CheckWin
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.TransitionTo(BoardPhase.Cascading);
            _sm.TransitionTo(BoardPhase.Settling);
            _sm.TransitionTo(BoardPhase.CheckBrew);
            _sm.TransitionTo(BoardPhase.CheckWin);

            // Win takes priority over Lose
            Assert.AreEqual(LevelOutcome.Win, _evaluator.Evaluate(),
                "Win must take priority over Lose when recipe is complete on the final move.");
        }

        [Test]
        public void MultipleFullCycles_ReturnToIdleBetweenEachCycle()
        {
            // Verifies that the state machine resets cleanly and the board stays valid
            // across three consecutive resolution loops.

            var tapCells = new[]
            {
                new GridCoord(0, 0),  // Ember cluster (size 4)
                new GridCoord(2, 4),  // Shadow cluster (size 3, after refill)
            };

            foreach (var tap in tapCells)
            {
                // Force a valid cluster at the tap cell in case refill changed it
                PlaceMinimalClusterAt(tap, IngredientColor.Vine);

                _sm.TransitionTo(BoardPhase.PlayerInput);
                var result = _fusion.TryPlayerFusion(tap);
                // Skip if refill did not produce a cluster — board is live
                if (result == null)
                {
                    _sm.Reset();
                    continue;
                }

                _sm.TransitionTo(BoardPhase.Fusing);
                _sm.TransitionTo(BoardPhase.Cascading);
                _cascade.ApplyGravityAndRefill();
                _sm.TransitionTo(BoardPhase.Settling);
                _sm.TransitionTo(BoardPhase.CheckBrew);
                _sm.TransitionTo(BoardPhase.CheckWin);
                _sm.TransitionTo(BoardPhase.Idle);

                Assert.AreEqual(BoardPhase.Idle, _sm.CurrentPhase,
                    "Machine must return to Idle after each full cycle.");
            }
        }

        [Test]
        public void OnPhaseChanged_EventFires_ForEveryTransitionInCycle()
        {
            // Subscribes before the cycle begins and counts events.
            int eventCount = 0;
            _sm.OnPhaseChanged += (_, _) => eventCount++;

            // One complete cycle = 7 transitions (PlayerInput..CheckWin..Idle)
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _fusion.TryPlayerFusion(new GridCoord(0, 0));
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.TransitionTo(BoardPhase.Cascading);
            _cascade.ApplyGravityAndRefill();
            _sm.TransitionTo(BoardPhase.Settling);
            _sm.TransitionTo(BoardPhase.CheckBrew);
            _sm.TransitionTo(BoardPhase.CheckWin);
            _sm.TransitionTo(BoardPhase.Idle);

            Assert.AreEqual(7, eventCount,
                "OnPhaseChanged must fire exactly 7 times per full cycle (one per transition).");
        }

        [Test]
        public void InputBlocked_DuringAllNonPlayerInputPhases()
        {
            // AcceptsInput must be false in every phase except PlayerInput.
            var nonInputPhases = new[]
            {
                BoardPhase.Fusing,
                BoardPhase.Cascading,
                BoardPhase.Settling,
                BoardPhase.CheckBrew,
                BoardPhase.CheckWin,
            };

            // Walk through the cycle and check each phase
            _sm.TransitionTo(BoardPhase.PlayerInput);
            Assert.IsTrue(_sm.AcceptsInput, "AcceptsInput must be true during PlayerInput.");

            _fusion.TryPlayerFusion(new GridCoord(0, 0));
            _sm.TransitionTo(BoardPhase.Fusing);
            Assert.IsFalse(_sm.AcceptsInput, "AcceptsInput must be false during Fusing.");

            _sm.TransitionTo(BoardPhase.Cascading);
            Assert.IsFalse(_sm.AcceptsInput, "AcceptsInput must be false during Cascading.");

            _cascade.ApplyGravityAndRefill();
            _sm.TransitionTo(BoardPhase.Settling);
            Assert.IsFalse(_sm.AcceptsInput, "AcceptsInput must be false during Settling.");

            _sm.TransitionTo(BoardPhase.CheckBrew);
            Assert.IsFalse(_sm.AcceptsInput, "AcceptsInput must be false during CheckBrew.");

            _sm.TransitionTo(BoardPhase.CheckWin);
            Assert.IsFalse(_sm.AcceptsInput, "AcceptsInput must be false during CheckWin.");

            _sm.TransitionTo(BoardPhase.Idle);
            Assert.IsFalse(_sm.AcceptsInput, "AcceptsInput must be false during Idle.");
        }

        // ------------------------------------------------------------------ private helpers

        /// <summary>
        /// Places a 3-cell horizontal cluster of <paramref name="color"/> starting at
        /// <paramref name="origin"/> so that TryPlayerFusion will succeed there.
        /// Cells at (col, row), (col+1, row), (col+2, row) are written if they are in bounds;
        /// otherwise the test is silently skipped via Assert.Ignore.
        /// </summary>
        private void PlaceMinimalClusterAt(GridCoord origin, IngredientColor color)
        {
            var coords = new[]
            {
                origin,
                new GridCoord(origin.Col + 1, origin.Row),
                new GridCoord(origin.Col + 2, origin.Row),
            };

            foreach (var c in coords)
            {
                if (!_board.InBounds(c))
                {
                    Assert.Ignore($"PlaceMinimalClusterAt: {c} is out of bounds for {Width}×{Height} board.");
                    return;
                }
            }

            foreach (var c in coords)
                _board.SetCell(c, CellContent.Token(color));
        }
    }
}
