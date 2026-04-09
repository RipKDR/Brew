using System;
using System.Collections.Generic;

namespace Brew.Core.LiveOps
{
    public enum EventState { NotStarted, Active, Completed, Expired }

    public sealed class WeeklyEventManager
    {
        private readonly int _levelCount;
        private readonly int _essencePerLevel;
        private readonly MilestoneDefinition[] _milestones;
        private readonly Func<bool> _featureFlagCheck;
        private readonly bool[] _completedLevels;
        private readonly bool[] _milestonesClaimed;

        private string _eventId;
        private string _eventName;
        private string _potionId;
        private long _endTimestamp;
        private bool _started;
        private bool _allLevelsDone;

        public string EventId => _eventId;
        public string EventName => _eventName;
        public string PotionId => _potionId;
        public int CompletedLevelCount { get; private set; }

        public event Action<int> OnLevelCompleted;
        public event Action<string, int> OnMilestoneReached;
        public event Action OnEventCompleted;

        public WeeklyEventManager(
            int levelCount,
            int essencePerLevel,
            MilestoneDefinition[] milestones,
            Func<bool> featureFlagCheck = null)
        {
            _levelCount = levelCount;
            _essencePerLevel = essencePerLevel;
            _milestones = milestones ?? Array.Empty<MilestoneDefinition>();
            _featureFlagCheck = featureFlagCheck ?? (() => true);
            _completedLevels = new bool[_levelCount];
            _milestonesClaimed = new bool[_milestones.Length];
        }

        public void StartEvent(string eventId, string eventName, long endTimestamp, string potionId)
        {
            if (!_featureFlagCheck()) return;

            _eventId = eventId ?? string.Empty;
            _eventName = eventName ?? string.Empty;
            _endTimestamp = endTimestamp;
            _potionId = potionId ?? string.Empty;
            _started = true;
            _allLevelsDone = false;
        }

        public bool TryCompleteLevel(int levelIndex, long currentTimestamp)
        {
            if (!_started) return false;
            if (currentTimestamp >= _endTimestamp) return false;
            if (levelIndex < 0 || levelIndex >= _levelCount) return false;
            if (_completedLevels[levelIndex]) return false;
            if (_allLevelsDone) return false;

            _completedLevels[levelIndex] = true;
            CompletedLevelCount++;

            OnLevelCompleted?.Invoke(levelIndex);

            if (CompletedLevelCount >= _levelCount)
            {
                _allLevelsDone = true;
                OnEventCompleted?.Invoke();
            }

            return true;
        }

        public EventState GetState(long currentTimestamp)
        {
            if (!_started) return EventState.NotStarted;
            if (_allLevelsDone) return EventState.Completed;
            if (currentTimestamp >= _endTimestamp) return EventState.Expired;
            return EventState.Active;
        }

        public bool IsLevelCompleted(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= _levelCount) return false;
            return _completedLevels[levelIndex];
        }

        public int EssencePerLevel => _essencePerLevel;

        public IReadOnlyList<(string type, int amount)> ClaimMilestoneRewards()
        {
            var rewards = new List<(string type, int amount)>();

            for (int i = 0; i < _milestones.Length; i++)
            {
                if (_milestonesClaimed[i]) continue;
                if (CompletedLevelCount < _milestones[i].RequiredLevels) continue;

                _milestonesClaimed[i] = true;
                foreach (var reward in _milestones[i].Rewards)
                    rewards.Add(reward);

                OnMilestoneReached?.Invoke(_milestones[i].Name, _milestones[i].RequiredLevels);
            }

            return rewards;
        }

        public void Reset()
        {
            _eventId = null;
            _eventName = null;
            _potionId = null;
            _endTimestamp = 0;
            _started = false;
            _allLevelsDone = false;
            CompletedLevelCount = 0;

            for (int i = 0; i < _completedLevels.Length; i++)
                _completedLevels[i] = false;
            for (int i = 0; i < _milestonesClaimed.Length; i++)
                _milestonesClaimed[i] = false;
        }
    }

    public sealed class MilestoneDefinition
    {
        public string Name { get; }
        public int RequiredLevels { get; }
        public (string type, int amount)[] Rewards { get; }

        public MilestoneDefinition(string name, int requiredLevels, params (string type, int amount)[] rewards)
        {
            Name = name;
            RequiredLevels = requiredLevels;
            Rewards = rewards ?? Array.Empty<(string, int)>();
        }
    }
}
