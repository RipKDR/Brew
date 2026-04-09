using System;
using Brew.Core.Economy;

namespace Brew.Core.Meta
{
    /// <summary>
    /// Sequential workshop upgrades purchased with Essence. Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class WorkshopManager
    {
        private readonly (string name, int essenceCost)[] _upgrades;
        private readonly CurrencyManager _currency;
        private int _currentLevel;

        public event Action<int, string> OnUpgradePurchased;

        public WorkshopManager((string name, int essenceCost)[] upgrades, CurrencyManager currency)
        {
            _upgrades = upgrades ?? throw new ArgumentNullException(nameof(upgrades));
            _currency = currency ?? throw new ArgumentNullException(nameof(currency));

            if (_upgrades.Length != 12)
                throw new ArgumentException("Workshop requires exactly 12 upgrade entries.", nameof(upgrades));

            foreach (var u in _upgrades)
            {
                if (string.IsNullOrEmpty(u.name))
                    throw new ArgumentException("Upgrade name cannot be null or empty.", nameof(upgrades));

                if (u.essenceCost <= 0)
                    throw new ArgumentOutOfRangeException(nameof(upgrades), u.essenceCost, "Each upgrade cost must be positive.");
            }
        }

        public int CurrentLevel => _currentLevel;

        public int MaxLevel => _upgrades.Length;

        public float CompletionPercent => MaxLevel == 0 ? 0f : _currentLevel * 100f / MaxLevel;

        public bool TryPurchaseNext()
        {
            if (_currentLevel >= MaxLevel)
                return false;

            var (name, cost) = _upgrades[_currentLevel];
            if (!_currency.Spend(CurrencyType.Essence, cost, "workshop_upgrade"))
                return false;

            _currentLevel++;
            OnUpgradePurchased?.Invoke(_currentLevel, name);
            return true;
        }

        public string GetNextUpgradeName() =>
            _currentLevel >= MaxLevel ? null : _upgrades[_currentLevel].name;

        public int GetNextUpgradeCost() =>
            _currentLevel >= MaxLevel ? -1 : _upgrades[_currentLevel].essenceCost;

        public void LoadLevel(int level)
        {
            if (level < 0 || level > MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(level), level, $"Level must be between 0 and {MaxLevel}.");

            _currentLevel = level;
        }
    }
}
