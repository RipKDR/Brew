using System;
using Brew.Core.Meta;
using UnityEngine;

namespace Brew.Data
{
    [CreateAssetMenu(fileName = "PotionShelfConfig", menuName = "Brew/Config/Potion Shelf Config")]
    public class PotionShelfConfigSO : ScriptableObject
    {
        [SerializeField] private int _totalPotions = 100;

        [SerializeField] private MilestoneEntry[] _milestones =
        {
            new() { RequiredCount = 25, EssenceReward = 500, GemReward = 25, Title = "Apprentice" },
            new() { RequiredCount = 50, EssenceReward = 1000, GemReward = 50, Title = "Brewer" },
            new() { RequiredCount = 75, EssenceReward = 2000, GemReward = 100, Title = "Alchemist" },
            new() { RequiredCount = 100, EssenceReward = 5000, GemReward = 250, Title = "Master Brewer" }
        };

        public int TotalPotions => _totalPotions;
        public MilestoneEntry[] Milestones => _milestones;

        public PotionShelfMilestoneDefinition[] BuildMilestoneDefinitions()
        {
            var defs = new PotionShelfMilestoneDefinition[_milestones.Length];
            for (int i = 0; i < _milestones.Length; i++)
            {
                var e = _milestones[i];
                defs[i] = new PotionShelfMilestoneDefinition(e.RequiredCount, e.EssenceReward, e.GemReward, e.Title);
            }

            return defs;
        }

        [Serializable]
        public struct MilestoneEntry
        {
            public int RequiredCount;
            public int EssenceReward;
            public int GemReward;
            public string Title;
        }
    }
}
