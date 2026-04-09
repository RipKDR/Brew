using System.Collections.Generic;
using Brew.Core.LiveOps;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core.LiveOps
{
    [TestFixture]
    public class WeeklyEventManagerTests
    {
        private const int LevelCount = 7;
        private const int EssencePerLevel = 75;
        private const int Milestone3Essence = 100;
        private const int Milestone5Gems = 10;
        private const int AllCompleteEssence = 250;
        private const int AllCompleteGems = 25;

        private const long ActiveTimestamp = 1000L;
        private const long EndTimestamp = 9999L;
        private const long ExpiredTimestamp = 10000L;

        private WeeklyEventManager _manager;

        private static MilestoneDefinition[] DefaultMilestones() => new[]
        {
            new MilestoneDefinition("3 Levels", 3, ("Essence", Milestone3Essence)),
            new MilestoneDefinition("5 Levels", 5, ("Gems", Milestone5Gems)),
            new MilestoneDefinition("All Complete", LevelCount,
                ("Essence", AllCompleteEssence), ("Gems", AllCompleteGems))
        };

        [SetUp]
        public void SetUp()
        {
            _manager = new WeeklyEventManager(LevelCount, EssencePerLevel, DefaultMilestones());
        }

        [Test]
        public void GetState_BeforeStartEvent_ReturnsNotStarted()
        {
            Assert.AreEqual(EventState.NotStarted, _manager.GetState(ActiveTimestamp));
        }

        [Test]
        public void StartEvent_SetsStateToActive()
        {
            _manager.StartEvent("evt1", "Test Event", EndTimestamp, "potion_x");
            Assert.AreEqual(EventState.Active, _manager.GetState(ActiveTimestamp));
        }

        [Test]
        public void StartEvent_SetsProperties()
        {
            _manager.StartEvent("evt1", "Test Event", EndTimestamp, "potion_x");
            Assert.AreEqual("evt1", _manager.EventId);
            Assert.AreEqual("Test Event", _manager.EventName);
            Assert.AreEqual("potion_x", _manager.PotionId);
        }

        [Test]
        public void TryCompleteLevel_ValidIndex_ReturnsTrue()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            Assert.IsTrue(_manager.TryCompleteLevel(0, ActiveTimestamp));
        }

        [Test]
        public void TryCompleteLevel_AlreadyCompleted_ReturnsFalse()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            _manager.TryCompleteLevel(0, ActiveTimestamp);
            Assert.IsFalse(_manager.TryCompleteLevel(0, ActiveTimestamp));
        }

        [Test]
        public void TryCompleteLevel_Expired_ReturnsFalse()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            Assert.IsFalse(_manager.TryCompleteLevel(0, ExpiredTimestamp));
        }

        [Test]
        public void TryCompleteLevel_InvalidIndex_ReturnsFalse()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            Assert.IsFalse(_manager.TryCompleteLevel(-1, ActiveTimestamp));
            Assert.IsFalse(_manager.TryCompleteLevel(LevelCount, ActiveTimestamp));
        }

        [Test]
        public void TryCompleteLevel_NotStarted_ReturnsFalse()
        {
            Assert.IsFalse(_manager.TryCompleteLevel(0, ActiveTimestamp));
        }

        [Test]
        public void CompletedLevelCount_IncrementsCorrectly()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            Assert.AreEqual(0, _manager.CompletedLevelCount);

            _manager.TryCompleteLevel(0, ActiveTimestamp);
            Assert.AreEqual(1, _manager.CompletedLevelCount);

            _manager.TryCompleteLevel(3, ActiveTimestamp);
            Assert.AreEqual(2, _manager.CompletedLevelCount);
        }

        [Test]
        public void IsLevelCompleted_ReturnsCorrectValues()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            _manager.TryCompleteLevel(2, ActiveTimestamp);

            Assert.IsTrue(_manager.IsLevelCompleted(2));
            Assert.IsFalse(_manager.IsLevelCompleted(0));
            Assert.IsFalse(_manager.IsLevelCompleted(6));
        }

        [Test]
        public void ClaimMilestoneRewards_At3Levels_Awards100Essence()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            CompleteLevels(3);

            var rewards = _manager.ClaimMilestoneRewards();
            Assert.IsTrue(ContainsReward(rewards, "Essence", Milestone3Essence));
        }

        [Test]
        public void ClaimMilestoneRewards_At5Levels_Awards10Gems()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            CompleteLevels(5);

            var rewards = _manager.ClaimMilestoneRewards();
            Assert.IsTrue(ContainsReward(rewards, "Gems", Milestone5Gems));
        }

        [Test]
        public void ClaimMilestoneRewards_AllLevels_AwardsAllCompleteRewards()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            CompleteLevels(LevelCount);

            var rewards = _manager.ClaimMilestoneRewards();
            Assert.IsTrue(ContainsReward(rewards, "Essence", AllCompleteEssence));
            Assert.IsTrue(ContainsReward(rewards, "Gems", AllCompleteGems));
        }

        [Test]
        public void TotalRewards_MatchSpec_875Essence_35Gems()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            CompleteLevels(LevelCount);

            var milestoneRewards = _manager.ClaimMilestoneRewards();
            int milestoneEssence = 0;
            int milestoneGems = 0;
            foreach (var (type, amount) in milestoneRewards)
            {
                if (type == "Essence") milestoneEssence += amount;
                if (type == "Gems") milestoneGems += amount;
            }

            int perLevelEssence = LevelCount * EssencePerLevel;
            int totalEssence = perLevelEssence + milestoneEssence;

            Assert.AreEqual(525, perLevelEssence, "Per-level: 7 * 75 = 525");
            Assert.AreEqual(350, milestoneEssence, "Milestones: 100 + 250 = 350");
            Assert.AreEqual(875, totalEssence, "Total Essence: 525 + 350 = 875");
            Assert.AreEqual(35, milestoneGems, "Total Gems: 10 + 25 = 35");
        }

        [Test]
        public void ClaimMilestoneRewards_NoLevelsCompleted_ReturnsEmpty()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            var rewards = _manager.ClaimMilestoneRewards();
            Assert.AreEqual(0, rewards.Count);
        }

        [Test]
        public void ClaimMilestoneRewards_DoubleClaim_DoesNotDuplicate()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            CompleteLevels(3);

            var firstClaim = _manager.ClaimMilestoneRewards();
            Assert.IsTrue(firstClaim.Count > 0, "First claim should have rewards");

            var secondClaim = _manager.ClaimMilestoneRewards();
            Assert.AreEqual(0, secondClaim.Count, "Second claim should be empty");
        }

        [Test]
        public void Reset_ClearsAllState()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            CompleteLevels(3);
            _manager.ClaimMilestoneRewards();

            _manager.Reset();

            Assert.AreEqual(EventState.NotStarted, _manager.GetState(ActiveTimestamp));
            Assert.AreEqual(0, _manager.CompletedLevelCount);
            Assert.IsFalse(_manager.IsLevelCompleted(0));
            Assert.IsNull(_manager.EventId);
        }

        [Test]
        public void GetState_AfterEndTimestamp_ReturnsExpired()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            Assert.AreEqual(EventState.Expired, _manager.GetState(ExpiredTimestamp));
        }

        [Test]
        public void GetState_AllLevelsDone_ReturnsCompleted()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            CompleteLevels(LevelCount);
            Assert.AreEqual(EventState.Completed, _manager.GetState(ActiveTimestamp));
        }

        [Test]
        public void OnLevelCompleted_Fires()
        {
            int firedIndex = -1;
            _manager.OnLevelCompleted += idx => firedIndex = idx;

            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            _manager.TryCompleteLevel(4, ActiveTimestamp);

            Assert.AreEqual(4, firedIndex);
        }

        [Test]
        public void OnEventCompleted_FiresWhenAllDone()
        {
            bool fired = false;
            _manager.OnEventCompleted += () => fired = true;

            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            CompleteLevels(LevelCount);

            Assert.IsTrue(fired);
        }

        [Test]
        public void FeatureFlag_Disabled_StartEventDoesNothing()
        {
            var mgr = new WeeklyEventManager(LevelCount, EssencePerLevel, DefaultMilestones(), () => false);
            mgr.StartEvent("evt1", "Test", EndTimestamp, "p");
            Assert.AreEqual(EventState.NotStarted, mgr.GetState(ActiveTimestamp));
        }

        [Test]
        public void OnMilestoneReached_FiresOnClaim()
        {
            var milestoneNames = new List<string>();
            _manager.OnMilestoneReached += (name, _) => milestoneNames.Add(name);

            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            CompleteLevels(3);
            _manager.ClaimMilestoneRewards();

            Assert.Contains("3 Levels", milestoneNames);
        }

        [Test]
        public void Reset_AllowsRestartAfterCompletion()
        {
            _manager.StartEvent("evt1", "Test", EndTimestamp, "p");
            CompleteLevels(LevelCount);
            Assert.AreEqual(EventState.Completed, _manager.GetState(ActiveTimestamp));

            _manager.Reset();
            _manager.StartEvent("evt2", "New Event", EndTimestamp + 5000, "potion_y");

            Assert.AreEqual(EventState.Active, _manager.GetState(ActiveTimestamp));
            Assert.AreEqual("evt2", _manager.EventId);
            Assert.IsTrue(_manager.TryCompleteLevel(0, ActiveTimestamp));
        }

        private void CompleteLevels(int count)
        {
            for (int i = 0; i < count && i < LevelCount; i++)
                _manager.TryCompleteLevel(i, ActiveTimestamp);
        }

        private static bool ContainsReward(IReadOnlyList<(string type, int amount)> rewards, string type, int amount)
        {
            foreach (var (t, a) in rewards)
            {
                if (t == type && a == amount) return true;
            }
            return false;
        }
    }
}
