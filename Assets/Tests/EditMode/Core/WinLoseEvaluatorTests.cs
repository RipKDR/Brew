using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    [TestFixture]
    public class WinLoseEvaluatorTests
    {
        private RecipeTracker _recipe;
        private MoveTracker _moves;
        private WinLoseEvaluator _eval;

        [SetUp]
        public void SetUp()
        {
            _recipe = new RecipeTracker(new[] { new RecipeTarget(IngredientColor.Ember, 2) });
            _moves = new MoveTracker(10);
            _eval = new WinLoseEvaluator(_recipe, _moves);
        }

        [Test]
        public void Evaluate_Initial_InProgress()
        {
            Assert.AreEqual(LevelOutcome.InProgress, _eval.Evaluate());
        }

        [Test]
        public void Evaluate_RecipeComplete_Win()
        {
            _recipe.OnBrew(IngredientColor.Ember);
            _recipe.OnBrew(IngredientColor.Ember);
            Assert.AreEqual(LevelOutcome.Win, _eval.Evaluate());
        }

        [Test]
        public void Evaluate_MovesExhausted_RecipeIncomplete_Lose()
        {
            var moves = new MoveTracker(1);
            var eval = new WinLoseEvaluator(_recipe, moves);
            moves.TryConsumeMove();
            Assert.AreEqual(LevelOutcome.Lose, eval.Evaluate());
        }

        [Test]
        public void Evaluate_MovesExhausted_RecipeComplete_Win()
        {
            var moves = new MoveTracker(1);
            var eval = new WinLoseEvaluator(_recipe, moves);

            moves.TryConsumeMove();
            _recipe.OnBrew(IngredientColor.Ember);
            _recipe.OnBrew(IngredientColor.Ember);

            Assert.AreEqual(LevelOutcome.Win, eval.Evaluate());
        }

        [Test]
        public void Evaluate_MovesRemaining_RecipeIncomplete_InProgress()
        {
            _moves.TryConsumeMove();
            _recipe.OnBrew(IngredientColor.Ember);
            Assert.AreEqual(LevelOutcome.InProgress, _eval.Evaluate());
        }

        [Test]
        public void WinPriorityOverLose_LastMoveTriggersBrew()
        {
            var recipe = new RecipeTracker(new[] { new RecipeTarget(IngredientColor.Frost, 1) });
            var moves = new MoveTracker(1);
            var eval = new WinLoseEvaluator(recipe, moves);

            moves.TryConsumeMove();
            recipe.OnBrew(IngredientColor.Frost);

            Assert.IsTrue(moves.IsExhausted);
            Assert.IsTrue(recipe.IsComplete);
            Assert.AreEqual(LevelOutcome.Win, eval.Evaluate());
        }

        [Test]
        public void MultiTarget_AllRequired()
        {
            var recipe = new RecipeTracker(new[]
            {
                new RecipeTarget(IngredientColor.Ember, 1),
                new RecipeTarget(IngredientColor.Frost, 1)
            });
            var eval = new WinLoseEvaluator(recipe, _moves);

            recipe.OnBrew(IngredientColor.Ember);
            Assert.AreEqual(LevelOutcome.InProgress, eval.Evaluate());

            recipe.OnBrew(IngredientColor.Frost);
            Assert.AreEqual(LevelOutcome.Win, eval.Evaluate());
        }
    }
}
