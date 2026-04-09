using System;
using UnityEngine;

namespace Brew.Data
{
    [CreateAssetMenu(fileName = "WorkshopConfig", menuName = "Brew/Config/Workshop Config")]
    public class WorkshopConfigSO : ScriptableObject
    {
        [SerializeField] private WorkshopUpgradeEntry[] _upgrades =
        {
            new() { Name = "Sweep the Floor", EssenceCost = 50, Description = string.Empty },
            new() { Name = "Light the Hearth", EssenceCost = 100, Description = string.Empty },
            new() { Name = "Repair the Workbench", EssenceCost = 200, Description = string.Empty },
            new() { Name = "Hang the Shelves", EssenceCost = 400, Description = string.Empty },
            new() { Name = "Install the Cauldron", EssenceCost = 600, Description = string.Empty },
            new() { Name = "Stock the Herb Rack", EssenceCost = 1000, Description = string.Empty },
            new() { Name = "Place the Star Map", EssenceCost = 1500, Description = string.Empty },
            new() { Name = "Add the Crystal Array", EssenceCost = 2500, Description = string.Empty },
            new() { Name = "Build the Distillery", EssenceCost = 4000, Description = string.Empty },
            new() { Name = "Enchant the Windows", EssenceCost = 6000, Description = string.Empty },
            new() { Name = "Summon the Familiar", EssenceCost = 8000, Description = string.Empty },
            new() { Name = "Master's Flourish", EssenceCost = 12000, Description = string.Empty }
        };

        public WorkshopUpgradeEntry[] Upgrades => _upgrades;

        public (string name, int essenceCost)[] BuildUpgradeTuples()
        {
            var tuples = new (string, int)[_upgrades.Length];
            for (int i = 0; i < _upgrades.Length; i++)
                tuples[i] = (_upgrades[i].Name, _upgrades[i].EssenceCost);
            return tuples;
        }

        [Serializable]
        public struct WorkshopUpgradeEntry
        {
            public string Name;
            public int EssenceCost;
            [TextArea(1, 4)]
            public string Description;
        }
    }
}
