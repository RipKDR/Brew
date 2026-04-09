using System;
using UnityEngine;

namespace Brew.Data
{
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "Brew/Config/Economy Config")]
    public class EconomyConfigSO : ScriptableObject
    {
        [Header("Level Completion — Essence Per Star Rating")]
        [Tooltip("Index 0 = fail (0), 1 = one-star, 2 = two-star, 3 = three-star")]
        [SerializeField] private int[] _essencePerStar = { 0, 30, 50, 80 };

        [Header("Win Streak Multiplier Tiers")]
        [SerializeField] private StreakTier[] _streakTiers =
        {
            new() { MinStreak = 1, Multiplier = 1.0f },
            new() { MinStreak = 2, Multiplier = 1.25f },
            new() { MinStreak = 3, Multiplier = 1.5f },
            new() { MinStreak = 5, Multiplier = 2.0f },
            new() { MinStreak = 8, Multiplier = 2.5f },
            new() { MinStreak = 10, Multiplier = 3.0f }
        };

        [Header("Booster Costs — Essence")]
        [SerializeField] private int _shakeEssenceCost = 50;
        [SerializeField] private int _catalystEssenceCost = 100;

        [Header("Booster Costs — Gems")]
        [SerializeField] private int _shakeGemCost = 5;
        [SerializeField] private int _catalystGemCost = 10;
        [SerializeField] private int _extraMovesGemCost = 10;

        [Header("Streak Protection")]
        [SerializeField] private int _streakProtectionGemCost = 5;

        [Header("Daily Brew")]
        [SerializeField] private int _dailyBrewEssence = 100;
        [SerializeField] private int _dailyBrewGems = 5;

        public int[] EssencePerStar => _essencePerStar;
        public StreakTier[] StreakTiers => _streakTiers;
        public int ShakeEssenceCost => _shakeEssenceCost;
        public int CatalystEssenceCost => _catalystEssenceCost;
        public int ShakeGemCost => _shakeGemCost;
        public int CatalystGemCost => _catalystGemCost;
        public int ExtraMovesGemCost => _extraMovesGemCost;
        public int StreakProtectionGemCost => _streakProtectionGemCost;
        public int DailyBrewEssence => _dailyBrewEssence;
        public int DailyBrewGems => _dailyBrewGems;

        public (int threshold, float multiplier)[] GetStreakTierTuples()
        {
            var tuples = new (int, float)[_streakTiers.Length];
            for (int i = 0; i < _streakTiers.Length; i++)
                tuples[i] = (_streakTiers[i].MinStreak, _streakTiers[i].Multiplier);
            return tuples;
        }

        [Serializable]
        public struct StreakTier
        {
            public int MinStreak;
            public float Multiplier;
        }
    }
}
