using System;
using System.Collections.Generic;
using System.Globalization;

namespace Brew.Core.Backend
{
    /// <summary>
    /// Remote config abstraction with local defaults and server overrides. Pure C#.
    /// </summary>
    public sealed class RemoteConfigManager
    {
        private static readonly TimeSpan StaleAfter = TimeSpan.FromHours(12);

        private readonly Dictionary<string, object> _values = new();

        public RemoteConfigManager(Dictionary<string, object> defaults)
        {
            if (defaults == null)
                throw new ArgumentNullException(nameof(defaults));

            foreach (var kv in defaults)
                _values[kv.Key] = kv.Value;
        }

        public DateTime? LastFetchUtc { get; private set; }

        public bool IsStale =>
            !LastFetchUtc.HasValue || DateTime.UtcNow - LastFetchUtc.Value > StaleAfter;

        public event Action OnConfigUpdated;

        public int GetInt(string key, int fallback = 0) =>
            TryConvertToInt(_values.TryGetValue(key, out var v) ? v : null, fallback);

        public float GetFloat(string key, float fallback = 0f) =>
            TryConvertToFloat(_values.TryGetValue(key, out var v) ? v : null, fallback);

        public bool GetBool(string key, bool fallback = false) =>
            TryConvertToBool(_values.TryGetValue(key, out var v) ? v : null, fallback);

        public string GetString(string key, string fallback = "") =>
            TryConvertToString(_values.TryGetValue(key, out var v) ? v : null, fallback);

        public void ApplyServerValues(Dictionary<string, object> serverValues)
        {
            if (serverValues == null)
                throw new ArgumentNullException(nameof(serverValues));

            foreach (var kv in serverValues)
                _values[kv.Key] = kv.Value;

            OnConfigUpdated?.Invoke();
        }

        public void MarkFetched(DateTime utcNow) => LastFetchUtc = utcNow;

        private static int TryConvertToInt(object value, int fallback)
        {
            if (value == null)
                return fallback;

            return value switch
            {
                int i => i,
                long l => l >= int.MinValue && l <= int.MaxValue ? (int)l : fallback,
                float f => (int)f,
                double d => (int)d,
                decimal m => (int)m,
                bool b => b ? 1 : 0,
                string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pi) => pi,
                _ => fallback
            };
        }

        private static float TryConvertToFloat(object value, float fallback)
        {
            if (value == null)
                return fallback;

            return value switch
            {
                float f => f,
                double d => (float)d,
                decimal m => (float)m,
                int i => i,
                long l => l,
                bool b => b ? 1f : 0f,
                string s when float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var pf) => pf,
                _ => fallback
            };
        }

        private static bool TryConvertToBool(object value, bool fallback)
        {
            if (value == null)
                return fallback;

            return value switch
            {
                bool b => b,
                int i => i != 0,
                long l => l != 0,
                float f => Math.Abs(f) > float.Epsilon,
                double d => Math.Abs(d) > double.Epsilon,
                string s when bool.TryParse(s, out var pb) => pb,
                string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pi) => pi != 0,
                _ => fallback
            };
        }

        private static string TryConvertToString(object value, string fallback)
        {
            if (value == null)
                return fallback;

            return value switch
            {
                string s => s,
                _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? fallback
            };
        }
    }
}
