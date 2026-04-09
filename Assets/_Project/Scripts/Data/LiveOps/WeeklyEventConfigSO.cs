using Brew.Core.LiveOps;
using UnityEngine;

namespace Brew.Data.LiveOps
{
    [CreateAssetMenu(fileName = "WeeklyEventConfig", menuName = "Brew/Config/Weekly Event Config")]
    public class WeeklyEventConfigSO : ScriptableObject
    {
        [Header("Level Settings")]
        [SerializeField] private int _levelCount = 7;
        [SerializeField] private int _essencePerLevel = 75;
        [SerializeField] private int[] _moveBudgets = { 20, 22, 25, 25, 28, 28, 32 };
        [SerializeField] private int _boardWidth = 7;
        [SerializeField] private int _boardHeight = 9;

        [Header("Milestone: 3 Levels")]
        [SerializeField] private int _milestoneThreeEssence = 100;

        [Header("Milestone: 5 Levels")]
        [SerializeField] private int _milestoneFiveGems = 10;

        [Header("Milestone: All Complete")]
        [SerializeField] private int _allCompleteEssence = 250;
        [SerializeField] private int _allCompleteGems = 25;

        [Header("Star Bonus Per Level")]
        [Tooltip("Extra essence per level for achieving 3 stars")]
        [SerializeField] private int _threeStarBonusEssence = 25;

        [Header("Gem Rewards")]
        [Tooltip("Gems for completing all event levels")]
        [SerializeField] private int _completionGems = 30;
        [Tooltip("Extra gems for all levels at 3 stars")]
        [SerializeField] private int _allThreeStarGems = 20;

        public int LevelCount => _levelCount;
        public int EssencePerLevel => _essencePerLevel;
        public int[] MoveBudgets => _moveBudgets;
        public int BoardWidth => _boardWidth;
        public int BoardHeight => _boardHeight;
        public int MilestoneThreeEssence => _milestoneThreeEssence;
        public int MilestoneFiveGems => _milestoneFiveGems;
        public int AllCompleteEssence => _allCompleteEssence;
        public int AllCompleteGems => _allCompleteGems;
        public int ThreeStarBonusEssence => _threeStarBonusEssence;
        public int CompletionGems => _completionGems;
        public int AllThreeStarGems => _allThreeStarGems;

        public MilestoneDefinition[] BuildMilestoneDefinitions()
        {
            return new[]
            {
                new MilestoneDefinition("3 Levels", 3,
                    ("Essence", _milestoneThreeEssence)),

                new MilestoneDefinition("5 Levels", 5,
                    ("Gems", _milestoneFiveGems)),

                new MilestoneDefinition("All Complete", _levelCount,
                    ("Essence", _allCompleteEssence),
                    ("Gems", _allCompleteGems))
            };
        }
    }
}
