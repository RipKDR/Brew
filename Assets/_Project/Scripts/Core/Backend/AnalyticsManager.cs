using System;
using System.Collections.Generic;

namespace Brew.Core.Backend
{
    /// <summary>
    /// Analytics facade; forwards to Firebase Analytics when integrated. Pure C#.
    /// </summary>
    public sealed class AnalyticsManager
    {
        private readonly List<(string name, Dictionary<string, object> parameters)> _eventLog = new();

        public event Action<string, Dictionary<string, object>> OnEventLogged;

        public void LogLevelStart(int levelId) =>
            LogEvent("level_start", new Dictionary<string, object> { { "level_id", levelId } });

        public void LogLevelComplete(int levelId, int starRating, int score, int movesRemaining, int durationSeconds) =>
            LogEvent("level_complete", new Dictionary<string, object>
            {
                { "level_id", levelId },
                { "star_rating", starRating },
                { "score", score },
                { "moves_remaining", movesRemaining },
                { "duration_seconds", durationSeconds }
            });

        public void LogLevelFail(int levelId, int movesUsed, int recipeRemaining, bool nearMiss) =>
            LogEvent("level_fail", new Dictionary<string, object>
            {
                { "level_id", levelId },
                { "moves_used", movesUsed },
                { "recipe_remaining", recipeRemaining },
                { "near_miss", nearMiss }
            });

        public void LogCurrencyEarned(string currencyType, int amount, string source, int balanceAfter) =>
            LogEvent("currency_earned", new Dictionary<string, object>
            {
                { "currency_type", currencyType ?? string.Empty },
                { "amount", amount },
                { "source", source ?? string.Empty },
                { "balance_after", balanceAfter }
            });

        public void LogCurrencySpent(string currencyType, int amount, string sink, int balanceAfter) =>
            LogEvent("currency_spent", new Dictionary<string, object>
            {
                { "currency_type", currencyType ?? string.Empty },
                { "amount", amount },
                { "sink", sink ?? string.Empty },
                { "balance_after", balanceAfter }
            });

        public void LogIAPPurchase(string productId, float priceUsd, bool receiptValid) =>
            LogEvent("iap_purchase", new Dictionary<string, object>
            {
                { "product_id", productId ?? string.Empty },
                { "price_usd", priceUsd },
                { "receipt_valid", receiptValid }
            });

        public void LogAdRewarded(string placement, string rewardType) =>
            LogEvent("ad_rewarded", new Dictionary<string, object>
            {
                { "placement", placement ?? string.Empty },
                { "reward_type", rewardType ?? string.Empty }
            });

        public void LogAdInterstitial() =>
            LogEvent("ad_interstitial", new Dictionary<string, object>());

        public void LogWorkshopUpgrade(int level, string upgradeName, int cost) =>
            LogEvent("workshop_upgrade", new Dictionary<string, object>
            {
                { "level", level },
                { "upgrade_name", upgradeName ?? string.Empty },
                { "cost", cost }
            });

        public void LogPotionShelfUnlock(int levelId, int totalBrewed, float completionPercent) =>
            LogEvent("potion_shelf_unlock", new Dictionary<string, object>
            {
                { "level_id", levelId },
                { "total_brewed", totalBrewed },
                { "completion_percent", completionPercent }
            });

        public void LogStreakUpdate(int streak, float multiplier, bool isReset) =>
            LogEvent("streak_update", new Dictionary<string, object>
            {
                { "streak", streak },
                { "multiplier", multiplier },
                { "is_reset", isReset }
            });

        public void LogDailyBrewStart() =>
            LogEvent("daily_brew_start", new Dictionary<string, object>());

        public void LogDailyBrewComplete(int essenceEarned, int gemsEarned, int dailyStreak) =>
            LogEvent("daily_brew_complete", new Dictionary<string, object>
            {
                { "essence_earned", essenceEarned },
                { "gems_earned", gemsEarned },
                { "daily_streak", dailyStreak }
            });

        public void LogSessionStart(string sessionId, int sessionNumber, int daysSinceInstall) =>
            LogEvent("session_start", new Dictionary<string, object>
            {
                { "session_id", sessionId ?? string.Empty },
                { "session_number", sessionNumber },
                { "days_since_install", daysSinceInstall }
            });

        public void LogSessionEnd(string sessionId, int durationSec, int levelsPlayed, int levelsCompleted) =>
            LogEvent("session_end", new Dictionary<string, object>
            {
                { "session_id", sessionId ?? string.Empty },
                { "duration_sec", durationSec },
                { "levels_played", levelsPlayed },
                { "levels_completed", levelsCompleted }
            });

        public void LogBoosterUsed(string boosterType, int chargesRemaining) =>
            LogEvent("booster_used", new Dictionary<string, object>
            {
                { "booster_type", boosterType ?? string.Empty },
                { "charges_remaining", chargesRemaining }
            });

        public void LogTutorialStart(string version) =>
            LogEvent("tutorial_start", new Dictionary<string, object>
            {
                { "version", version ?? string.Empty }
            });

        public void LogTutorialStepComplete(int step, string stepName) =>
            LogEvent("tutorial_step_complete", new Dictionary<string, object>
            {
                { "step", step },
                { "step_name", stepName ?? string.Empty }
            });

        public void LogTutorialComplete(int durationSeconds) =>
            LogEvent("tutorial_complete", new Dictionary<string, object>
            {
                { "duration_seconds", durationSeconds }
            });

        public void LogNotificationScheduled(string notificationType) =>
            LogEvent("notification_scheduled", new Dictionary<string, object>
            {
                { "notification_type", notificationType ?? string.Empty }
            });

        public void LogNotificationOpened(string notificationType) =>
            LogEvent("notification_opened", new Dictionary<string, object>
            {
                { "notification_type", notificationType ?? string.Empty }
            });

        public void LogEventStart(string eventId) =>
            LogEvent("event_start", new Dictionary<string, object> { { "event_id", eventId ?? string.Empty } });

        public void LogEventLevelComplete(string eventId, int levelIndex, int stars) =>
            LogEvent("event_level_complete", new Dictionary<string, object>
            {
                { "event_id", eventId ?? string.Empty },
                { "level_index", levelIndex },
                { "stars", stars }
            });

        public void LogEventComplete(string eventId) =>
            LogEvent("event_complete", new Dictionary<string, object> { { "event_id", eventId ?? string.Empty } });

        public void LogEventRewardClaimed(string eventId, string rewardType, int amount) =>
            LogEvent("event_reward_claimed", new Dictionary<string, object>
            {
                { "event_id", eventId ?? string.Empty },
                { "reward_type", rewardType ?? string.Empty },
                { "amount", amount }
            });

        public List<(string name, Dictionary<string, object> parameters)> GetEventLog() => _eventLog;

        private void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            var snapshot = new Dictionary<string, object>(parameters);
            _eventLog.Add((eventName, snapshot));
            OnEventLogged?.Invoke(eventName, snapshot);
        }
    }
}
