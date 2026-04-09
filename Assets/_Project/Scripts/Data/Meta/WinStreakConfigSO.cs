using System;
using UnityEngine;

namespace Brew.Data
{
    [CreateAssetMenu(fileName = "WinStreakConfig", menuName = "Brew/Config/Win Streak Config")]
    public class WinStreakConfigSO : ScriptableObject
    {
        [SerializeField] private StreakTier[] _tiers =
        {
            new() { Threshold = 1, Multiplier = 1.0f },
            new() { Threshold = 2, Multiplier = 1.25f },
            new() { Threshold = 3, Multiplier = 1.5f },
            new() { Threshold = 5, Multiplier = 2.0f },
            new() { Threshold = 8, Multiplier = 2.5f },
            new() { Threshold = 10, Multiplier = 3.0f }
        };

        public StreakTier[] Tiers => _tiers;

        public (int threshold, float multiplier)[] BuildTierTuples()
        {
            var tuples = new (int, float)[_tiers.Length];
            for (int i = 0; i < _tiers.Length; i++)
                tuples[i] = (_tiers[i].Threshold, _tiers[i].Multiplier);
            return tuples;
        }

        [Serializable]
        public struct StreakTier
        {
            public int Threshold;
            public float Multiplier;
        }
    }
}
