using System;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    [TestFixture]
    public class RecipeTrackerTests
    {
        [Test]
        public void Constructor_NullTargets_Throws()
        {
            Assert.Throws<ArgumentException>(() => new RecipeTracker(null));
        }

        [Test]
        public void Constructor_EmptyTargets_Throws()
        {
            Assert.Throws<ArgumentException>(() => new RecipeTracker(Array.Empty<RecipeTarget>()));
        }

        [Test]
        public void IsComplete_InitiallyFalse()
        {
            var tracker = new RecipeTracker(new[] { new RecipeTarget(IngredientColor.Ember, 2) });
            Assert.IsFalse(tracker.IsComplete);
        }

        [Test]
        public void SingleTarget_CompletesAfterEnoughBrews()
        {
            var tracker = new RecipeTracker(new[] { new RecipeTarget(IngredientColor.Ember, 2) });
            tracker.OnBrew(IngredientColor.Ember);
            Assert.IsFalse(tracker.IsComplete);
            tracker.OnBrew(IngredientColor.Ember);
            Assert.IsTrue(tracker.IsComplete);
        }

        [Test]
        public void MultipleTargets_AllMustBeComplete()
        {
            var tracker = new RecipeTracker(new[]
            {
                new RecipeTarget(IngredientColor.Ember, 1),
                new RecipeTarget(IngredientColor.Frost, 2)
            });

            tracker.OnBrew(IngredientColor.Ember);
            Assert.IsFalse(tracker.IsComplete);

            tracker.OnBrew(IngredientColor.Frost);
            Assert.IsFalse(tracker.IsComplete);

            tracker.OnBrew(IngredientColor.Frost);
            Assert.IsTrue(tracker.IsComplete);
        }

        [Test]
        public void NonTargetBrew_DoesNotAffectProgress()
        {
            var tracker = new RecipeTracker(new[] { new RecipeTarget(IngredientColor.Ember, 1) });
            tracker.OnBrew(IngredientColor.Vine);
            tracker.OnBrew(IngredientColor.Frost);
            Assert.IsFalse(tracker.IsComplete);

            tracker.OnBrew(IngredientColor.Ember);
            Assert.IsTrue(tracker.IsComplete);
        }

        [Test]
        public void ExtraBrew_BeyondTarget_DoesNotOverflow()
        {
            var tracker = new RecipeTracker(new[] { new RecipeTarget(IngredientColor.Ember, 1) });
            tracker.OnBrew(IngredientColor.Ember);
            tracker.OnBrew(IngredientColor.Ember); // extra, should not break
            Assert.IsTrue(tracker.IsComplete);
        }

        [Test]
        public void IsTargetColor_TrueForUnfilledTarget()
        {
            var tracker = new RecipeTracker(new[]
            {
                new RecipeTarget(IngredientColor.Ember, 2),
                new RecipeTarget(IngredientColor.Frost, 1)
            });

            Assert.IsTrue(tracker.IsTargetColor(IngredientColor.Ember));
            Assert.IsTrue(tracker.IsTargetColor(IngredientColor.Frost));
            Assert.IsFalse(tracker.IsTargetColor(IngredientColor.Vine));
        }

        [Test]
        public void IsTargetColor_FalseAfterTargetFilled()
        {
            var tracker = new RecipeTracker(new[] { new RecipeTarget(IngredientColor.Frost, 1) });
            Assert.IsTrue(tracker.IsTargetColor(IngredientColor.Frost));

            tracker.OnBrew(IngredientColor.Frost);
            Assert.IsFalse(tracker.IsTargetColor(IngredientColor.Frost));
        }

        [Test]
        public void CurrentProgress_ReflectsRemainingCounts()
        {
            var tracker = new RecipeTracker(new[]
            {
                new RecipeTarget(IngredientColor.Ember, 3),
                new RecipeTarget(IngredientColor.Frost, 1)
            });

            tracker.OnBrew(IngredientColor.Ember);

            var progress = tracker.CurrentProgress;
            Assert.AreEqual(2, progress.Count);

            Assert.AreEqual(IngredientColor.Ember, progress[0].Color);
            Assert.AreEqual(3, progress[0].Required);
            Assert.AreEqual(2, progress[0].Remaining);
            Assert.IsFalse(progress[0].IsFilled);

            Assert.AreEqual(IngredientColor.Frost, progress[1].Color);
            Assert.AreEqual(1, progress[1].Required);
            Assert.AreEqual(1, progress[1].Remaining);
        }

        [Test]
        public void CurrentProgress_IsFilled_TrueWhenZeroRemaining()
        {
            var tracker = new RecipeTracker(new[] { new RecipeTarget(IngredientColor.Sun, 1) });
            tracker.OnBrew(IngredientColor.Sun);
            Assert.IsTrue(tracker.CurrentProgress[0].IsFilled);
        }

        [Test]
        public void OnRecipeProgressChanged_FiresOnBrew()
        {
            var tracker = new RecipeTracker(new[] { new RecipeTarget(IngredientColor.Ember, 3) });
            IngredientColor receivedColor = IngredientColor.Shadow;
            int receivedRemaining = -1;
            tracker.OnRecipeProgressChanged += (color, remaining) =>
            {
                receivedColor = color;
                receivedRemaining = remaining;
            };

            tracker.OnBrew(IngredientColor.Ember);
            Assert.AreEqual(IngredientColor.Ember, receivedColor);
            Assert.AreEqual(2, receivedRemaining);
        }

        [Test]
        public void OnRecipeProgressChanged_DoesNotFireForNonMatchingColor()
        {
            var tracker = new RecipeTracker(new[] { new RecipeTarget(IngredientColor.Ember, 1) });
            bool fired = false;
            tracker.OnRecipeProgressChanged += (_, _) => fired = true;

            tracker.OnBrew(IngredientColor.Frost);
            Assert.IsFalse(fired);
        }

        [Test]
        public void OnRecipeComplete_FiresWhenAllTargetsMet()
        {
            var tracker = new RecipeTracker(new[]
            {
                new RecipeTarget(IngredientColor.Ember, 1),
                new RecipeTarget(IngredientColor.Frost, 1)
            });
            bool completeFired = false;
            tracker.OnRecipeComplete += () => completeFired = true;

            tracker.OnBrew(IngredientColor.Ember);
            Assert.IsFalse(completeFired);

            tracker.OnBrew(IngredientColor.Frost);
            Assert.IsTrue(completeFired);
        }

        [Test]
        public void DuplicateColorTargets_HandledIndependently()
        {
            var tracker = new RecipeTracker(new[]
            {
                new RecipeTarget(IngredientColor.Ember, 2),
                new RecipeTarget(IngredientColor.Ember, 1)
            });

            // First two brews fill the first target
            tracker.OnBrew(IngredientColor.Ember);
            tracker.OnBrew(IngredientColor.Ember);
            Assert.IsFalse(tracker.IsComplete);

            // Third brew fills the second target
            tracker.OnBrew(IngredientColor.Ember);
            Assert.IsTrue(tracker.IsComplete);
        }
    }
}
