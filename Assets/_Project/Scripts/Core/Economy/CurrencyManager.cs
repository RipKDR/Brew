using System;
using System.Collections.Generic;

namespace Brew.Core.Economy
{
    /// <summary>
    /// Single source of truth for all currency operations. Balances cannot go negative.
    /// All earning and spending must flow through Add/Spend. Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class CurrencyManager
    {
        private readonly Dictionary<CurrencyType, int> _balances = new()
        {
            { CurrencyType.Essence, 0 },
            { CurrencyType.Gems, 0 }
        };

        public event Action<CurrencyType, int, string> OnCurrencyEarned;
        public event Action<CurrencyType, int, string> OnCurrencySpent;
        public event Action<CurrencyType, int> OnBalanceChanged;

        public int GetBalance(CurrencyType type) => _balances[type];

        public void Add(CurrencyType type, int amount, string source)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Add amount must be positive.");

            _balances[type] += amount;
            OnCurrencyEarned?.Invoke(type, amount, source);
            OnBalanceChanged?.Invoke(type, _balances[type]);
        }

        public bool Spend(CurrencyType type, int amount, string sink)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Spend amount must be positive.");

            if (_balances[type] < amount)
                return false;

            _balances[type] -= amount;
            OnCurrencySpent?.Invoke(type, amount, sink);
            OnBalanceChanged?.Invoke(type, _balances[type]);
            return true;
        }

        /// <summary>
        /// For save/load — sets the balance directly without firing earn/spend events.
        /// Fires OnBalanceChanged.
        /// </summary>
        public void SetBalance(CurrencyType type, int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Balance cannot be negative.");

            _balances[type] = amount;
            OnBalanceChanged?.Invoke(type, _balances[type]);
        }

        public bool CanAfford(CurrencyType type, int amount) => _balances[type] >= amount;
    }
}
