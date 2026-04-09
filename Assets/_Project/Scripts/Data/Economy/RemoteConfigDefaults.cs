using System.Collections.Generic;

namespace Brew.Data
{
    /// <summary>
    /// Default values for all Firebase Remote Config keys. Mirrors config/firebase/remote-config-defaults.json.
    /// Used as local fallback when server values are unavailable.
    /// </summary>
    public static class RemoteConfigDefaults
    {
        // Economy — Star rewards
        public const string EssencePerStar1 = "essence_per_star_1";
        public const string EssencePerStar2 = "essence_per_star_2";
        public const string EssencePerStar3 = "essence_per_star_3";

        // Economy — Streak multipliers
        public const string StreakMultiplier2 = "streak_multiplier_2";
        public const string StreakMultiplier3 = "streak_multiplier_3";
        public const string StreakMultiplier5 = "streak_multiplier_5";
        public const string StreakMultiplier8 = "streak_multiplier_8";
        public const string StreakMultiplier10 = "streak_multiplier_10";

        // Economy — Booster costs
        public const string ShakeEssenceCost = "shake_essence_cost";
        public const string CatalystEssenceCost = "catalyst_essence_cost";
        public const string ShakeGemCost = "shake_gem_cost";
        public const string CatalystGemCost = "catalyst_gem_cost";
        public const string ExtraMovesGemCost = "extra_moves_gem_cost";
        public const string StreakProtectionGemCost = "streak_protection_gem_cost";

        // Economy — Daily Brew
        public const string DailyBrewEssence = "daily_brew_essence";
        public const string DailyBrewGems = "daily_brew_gems";

        // Ads
        public const string RewardedAdMaxPerSession = "rewarded_ad_max_per_session";
        public const string InterstitialFrequency = "interstitial_frequency";

        // Feature flags
        public const string WeeklyEventsEnabled = "weekly_events_enabled";
        public const string DailyBrewEnabled = "daily_brew_enabled";

        // Workshop costs (12 tiers)
        public const string WorkshopCost1 = "workshop_cost_1";
        public const string WorkshopCost2 = "workshop_cost_2";
        public const string WorkshopCost3 = "workshop_cost_3";
        public const string WorkshopCost4 = "workshop_cost_4";
        public const string WorkshopCost5 = "workshop_cost_5";
        public const string WorkshopCost6 = "workshop_cost_6";
        public const string WorkshopCost7 = "workshop_cost_7";
        public const string WorkshopCost8 = "workshop_cost_8";
        public const string WorkshopCost9 = "workshop_cost_9";
        public const string WorkshopCost10 = "workshop_cost_10";
        public const string WorkshopCost11 = "workshop_cost_11";
        public const string WorkshopCost12 = "workshop_cost_12";

        // Potion Shelf milestones
        public const string ShelfMilestone25Essence = "shelf_milestone_25_essence";
        public const string ShelfMilestone25Gems = "shelf_milestone_25_gems";
        public const string ShelfMilestone50Essence = "shelf_milestone_50_essence";
        public const string ShelfMilestone50Gems = "shelf_milestone_50_gems";
        public const string ShelfMilestone75Essence = "shelf_milestone_75_essence";
        public const string ShelfMilestone75Gems = "shelf_milestone_75_gems";
        public const string ShelfMilestone100Essence = "shelf_milestone_100_essence";
        public const string ShelfMilestone100Gems = "shelf_milestone_100_gems";

        // Daily Brew streak bonuses
        public const string DailyBrewStreak3Multiplier = "daily_brew_streak_3_multiplier";
        public const string DailyBrewStreak5Multiplier = "daily_brew_streak_5_multiplier";
        public const string DailyBrewStreak7Multiplier = "daily_brew_streak_7_multiplier";
        public const string DailyBrewStreak3BonusGems = "daily_brew_streak_3_bonus_gems";
        public const string DailyBrewStreak5BonusGems = "daily_brew_streak_5_bonus_gems";
        public const string DailyBrewStreak7BonusGems = "daily_brew_streak_7_bonus_gems";

        // IAP — Starter Bundle
        public const string StarterBundleGems = "starter_bundle_gems";
        public const string StarterBundleEssence = "starter_bundle_essence";
        public const string StarterBundleCatalysts = "starter_bundle_catalysts";
        public const string StarterBundleMaxLevel = "starter_bundle_max_level";

        // IAP — Gem Packs
        public const string GemPack1Amount = "gem_pack_1_amount";
        public const string GemPack2Amount = "gem_pack_2_amount";
        public const string GemPack3Amount = "gem_pack_3_amount";
        public const string GemPack4Amount = "gem_pack_4_amount";

        // IAP — No-Ads Pass
        public const string NoAdsPassSku = "no_ads_pass_sku";

        // Weekly Event
        public const string EventActive = "event_active";
        public const string EventId = "event_id";
        public const string EventName = "event_name";
        public const string EventEndTimestamp = "event_end_timestamp";
        public const string EventPotionId = "event_potion_id";
        public const string EventThemePrimaryColor = "event_theme_primary_color";
        public const string EventThemeSecondaryColor = "event_theme_secondary_color";
        public const string EventThemedIngredientId = "event_themed_ingredient_id";
        public const string EventEssencePerLevel = "event_essence_per_level";
        public const string EventMilestone3Essence = "event_milestone_3_essence";
        public const string EventMilestone5Gems = "event_milestone_5_gems";
        public const string EventAllCompleteEssence = "event_all_complete_essence";
        public const string EventAllCompleteGems = "event_all_complete_gems";

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
            { DailyBrewEnabled, true },
            { WorkshopCost1, 50 },
            { WorkshopCost2, 100 },
            { WorkshopCost3, 200 },
            { WorkshopCost4, 400 },
            { WorkshopCost5, 600 },
            { WorkshopCost6, 1000 },
            { WorkshopCost7, 1500 },
            { WorkshopCost8, 2500 },
            { WorkshopCost9, 4000 },
            { WorkshopCost10, 6000 },
            { WorkshopCost11, 8000 },
            { WorkshopCost12, 12000 },
            { ShelfMilestone25Essence, 500 },
            { ShelfMilestone25Gems, 25 },
            { ShelfMilestone50Essence, 1000 },
            { ShelfMilestone50Gems, 50 },
            { ShelfMilestone75Essence, 2000 },
            { ShelfMilestone75Gems, 100 },
            { ShelfMilestone100Essence, 5000 },
            { ShelfMilestone100Gems, 250 },
            { DailyBrewStreak3Multiplier, 1.25f },
            { DailyBrewStreak5Multiplier, 1.5f },
            { DailyBrewStreak7Multiplier, 2.0f },
            { DailyBrewStreak3BonusGems, 5 },
            { DailyBrewStreak5BonusGems, 10 },
            { DailyBrewStreak7BonusGems, 15 },
            { StarterBundleGems, 50 },
            { StarterBundleEssence, 500 },
            { StarterBundleCatalysts, 3 },
            { StarterBundleMaxLevel, 15 },
            { GemPack1Amount, 50 },
            { GemPack2Amount, 300 },
            { GemPack3Amount, 700 },
            { GemPack4Amount, 1500 },
            { NoAdsPassSku, "no_ads" },
            { EventActive, false },
            { EventId, "" },
            { EventName, "" },
            { EventEndTimestamp, 0 },
            { EventPotionId, "" },
            { EventThemePrimaryColor, "#FFFFFF" },
            { EventThemeSecondaryColor, "#CCCCCC" },
            { EventThemedIngredientId, "" },
            { EventEssencePerLevel, 75 },
            { EventMilestone3Essence, 100 },
            { EventMilestone5Gems, 10 },
            { EventAllCompleteEssence, 250 },
            { EventAllCompleteGems, 25 }
        };
    }
}
