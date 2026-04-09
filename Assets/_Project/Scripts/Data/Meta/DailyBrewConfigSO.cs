using System;
using Brew.Core.Meta;
using UnityEngine;

namespace Brew.Data
{
    [CreateAssetMenu(fileName = "DailyBrewConfig", menuName = "Brew/Config/Daily Brew Config")]
    public class DailyBrewConfigSO : ScriptableObject
    {
        [SerializeField] private int _baseEssence = 100;
        [SerializeField] private int _baseGems = 5;

        [SerializeField] private DailyStreakBonusEntry[] _streakBonuses =
        {
            new() { RequiredStreak = 3, EssenceMultiplier = 1.25f, BonusGems = 5 },
            new() { RequiredStreak = 5, EssenceMultiplier = 1.5f, BonusGems = 10 },
            new() { RequiredStreak = 7, EssenceMultiplier = 2.0f, BonusGems = 15 }
        };

        public int BaseEssence => _baseEssence;
        public int BaseGems => _baseGems;
        public DailyStreakBonusEntry[] StreakBonuses => _streakBonuses;

        public DailyStreakBonusDefinition[] BuildStreakBonusDefinitions()
        {
            var defs = new DailyStreakBonusDefinition[_streakBonuses.Length];
            for (int i = 0; i < _streakBonuses.Length; i++)
            {
                var e = _streakBonuses[i];
                defs[i] = new DailyStreakBonusDefinition(e.RequiredStreak, e.EssenceMultiplier, e.BonusGems);
            }

            return defs;
        }

        [Serializable]
        public struct DailyStreakBonusEntry
        {
            public int RequiredStreak;
            public float EssenceMultiplier;
            public int BonusGems;
        }
    }
}
