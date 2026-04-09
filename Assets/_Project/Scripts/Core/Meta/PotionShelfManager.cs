using System;
using System.Collections.Generic;
using System.Linq;

namespace Brew.Core.Meta
{
    /// <summary>
    /// Tracks brewed potions (level IDs) and shelf completion milestones. Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class PotionShelfManager
    {
        private readonly HashSet<int> _brewedLevelIds = new();
        private readonly HashSet<int> _claimedMilestoneThresholds = new();
        private readonly PotionShelfMilestoneDefinition[] _milestones;
        private readonly int _totalPotions;

        public event Action<int> OnPotionUnlocked;
        public event Action<int, int, int, string> OnMilestoneReached;

        public PotionShelfManager(int totalPotions, PotionShelfMilestoneDefinition[] milestones)
        {
            if (totalPotions <= 0)
                throw new ArgumentOutOfRangeException(nameof(totalPotions), totalPotions, "Total potions must be positive.");

            _totalPotions = totalPotions;
            _milestones = milestones ?? throw new ArgumentNullException(nameof(milestones));

            if (_milestones.Length == 0)
                throw new ArgumentException("At least one milestone is required.", nameof(milestones));

            var seen = new HashSet<int>();
            foreach (var m in _milestones)
            {
                if (!seen.Add(m.RequiredCount))
                    throw new ArgumentException($"Duplicate milestone threshold: {m.RequiredCount}.", nameof(milestones));
            }
        }

        public int TotalPotions => _totalPotions;

        public int BrewedCount => _brewedLevelIds.Count;

        public float CompletionPercent => _totalPotions == 0 ? 0f : Math.Min(100f, BrewedCount * 100f / _totalPotions);

        public bool TryUnlockPotion(int levelId)
        {
            if (_brewedLevelIds.Add(levelId))
            {
                OnPotionUnlocked?.Invoke(levelId);
                return true;
            }

            return false;
        }

        public bool HasPotion(int levelId) => _brewedLevelIds.Contains(levelId);

        /// <summary>
        /// Returns milestones that are newly satisfied and claimed (not previously claimed). Fires <see cref="OnMilestoneReached"/> for each.
        /// </summary>
        public IReadOnlyList<PotionShelfMilestoneDefinition> CheckMilestones()
        {
            var newlyClaimed = new List<PotionShelfMilestoneDefinition>();

            foreach (var m in _milestones.OrderBy(x => x.RequiredCount))
            {
                if (BrewedCount < m.RequiredCount)
                    continue;

                if (!_claimedMilestoneThresholds.Add(m.RequiredCount))
                    continue;

                newlyClaimed.Add(m);
                OnMilestoneReached?.Invoke(m.RequiredCount, m.EssenceReward, m.GemReward, m.Title);
            }

            return newlyClaimed;
        }

        public int[] GetUnlockedPotionIds() => _brewedLevelIds.OrderBy(id => id).ToArray();

        public int[] GetClaimedMilestones() => _claimedMilestoneThresholds.OrderBy(x => x).ToArray();

        public void LoadState(int[] potionIds, int[] claimedMilestones)
        {
            _brewedLevelIds.Clear();
            _claimedMilestoneThresholds.Clear();

            if (potionIds != null)
            {
                foreach (var id in potionIds)
                    _brewedLevelIds.Add(id);
            }

            if (claimedMilestones != null)
            {
                foreach (var t in claimedMilestones)
                    _claimedMilestoneThresholds.Add(t);
            }
        }
    }

    public readonly struct PotionShelfMilestoneDefinition : IEquatable<PotionShelfMilestoneDefinition>
    {
        public int RequiredCount { get; }
        public int EssenceReward { get; }
        public int GemReward { get; }
        public string Title { get; }

        public PotionShelfMilestoneDefinition(int requiredCount, int essenceReward, int gemReward, string title)
        {
            RequiredCount = requiredCount;
            EssenceReward = essenceReward;
            GemReward = gemReward;
            Title = title ?? string.Empty;
        }

        public bool Equals(PotionShelfMilestoneDefinition other) =>
            RequiredCount == other.RequiredCount &&
            EssenceReward == other.EssenceReward &&
            GemReward == other.GemReward &&
            Title == other.Title;

        public override bool Equals(object obj) => obj is PotionShelfMilestoneDefinition other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(RequiredCount, EssenceReward, GemReward, Title);
    }
}
