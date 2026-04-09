using System.Collections.Generic;

namespace Brew.Data
{
    /// <summary>
    /// Default values for all Firebase Remote Config keys. Mirrors config/firebase/remote-config-defaults.json.
    /// Used as local fallback when server values are unavailable.
    /// </summary>
    public static class RemoteConfigDefaults
    {
        public const string EssencePerStar1 = "essence_per_star_1";
        public const string EssencePerStar2 = "essence_per_star_2";
        public const string EssencePerStar3 = "essence_per_star_3";

        public const string StreakMultiplier2 = "streak_multiplier_2";
        public const string StreakMultiplier3 = "streak_multiplier_3";
        public const string StreakMultiplier5 = "streak_multiplier_5";
        public const string StreakMultiplier8 = "streak_multiplier_8";
        public const string StreakMultiplier10 = "streak_multiplier_10";

        public const string ShakeEssenceCost = "shake_essence_cost";
        public const string CatalystEssenceCost = "catalyst_essence_cost";
        public const string ShakeGemCost = "shake_gem_cost";
        public const string CatalystGemCost = "catalyst_gem_cost";
        public const string ExtraMovesGemCost = "extra_moves_gem_cost";
        public const string StreakProtectionGemCost = "streak_protection_gem_cost";

        public const string DailyBrewEssence = "daily_brew_essence";
        public const string DailyBrewGems = "daily_brew_gems";

        public const string RewardedAdMaxPerSession = "rewarded_ad_max_per_session";
        public const string InterstitialFrequency = "interstitial_frequency";

        public const string WeeklyEventsEnabled = "weekly_events_enabled";
        public const string DailyBrewEnabled = "daily_brew_enabled";

        public static Dictionary<string, object> GetAll() => new()
        {
            { EssencePerStar1, 30 },
            { EssencePerStar2, 50 },
            { EssencePerStar3, 80 },
            { StreakMultiplier2, 1.25f },
            { StreakMultiplier3, 1.5f },
            { StreakMultiplier5, 2.0f },
            { StreakMultiplier8, 2.5f },
            { StreakMultiplier10, 3.0f },
            { ShakeEssenceCost, 50 },
            { CatalystEssenceCost, 100 },
            { ShakeGemCost, 5 },
            { CatalystGemCost, 10 },
            { ExtraMovesGemCost, 10 },
            { StreakProtectionGemCost, 5 },
            { DailyBrewEssence, 100 },
            { DailyBrewGems, 5 },
            { RewardedAdMaxPerSession, 5 },
            { InterstitialFrequency, 3 },
            { WeeklyEventsEnabled, true },
            { DailyBrewEnabled, true }
        };
    }
}
