using System;

namespace Brew.Utilities
{
    /// <summary>
    /// Formats currency amounts for compact UI display.
    /// Sub-1K: exact. 1K-9.9K: one decimal + K. 10K-999K: integer + K.
    /// 1M-9.9M: one decimal + M. 10M+: integer + M.
    /// </summary>
    public static class CurrencyFormatter
    {
        public static string Format(int amount)
        {
            if (amount < 0) return "0";
            if (amount < 1_000) return amount.ToString();
            if (amount < 10_000) return $"{amount / 1000f:0.0}K";
            if (amount < 1_000_000) return $"{amount / 1000}K";
            if (amount < 10_000_000) return $"{amount / 1_000_000f:0.0}M";
            return $"{amount / 1_000_000}M";
        }
    }
}
