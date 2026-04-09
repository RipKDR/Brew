using Brew.Core;
using Brew.Core.Ads;
using Brew.Core.Backend;
using Brew.Core.Economy;
using Brew.Core.LiveOps;
using Brew.Core.Meta;
using Brew.Core.Services;
using Brew.Data;
using Brew.Data.LiveOps;
using Brew.Presentation.LiveOps;
using UnityEngine;

namespace Brew.Presentation
{
    /// <summary>
    /// Top-level controller that orchestrates the game flow:
    /// Level Select -> Gameplay -> Complete/Fail -> Level Select.
    /// Integrates economy rewards, meta-progression, ads, and analytics.
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private GameObject _levelSelectPanel;
        [SerializeField] private GameObject _gameplayPanel;
        [SerializeField] private LevelCompleteScreen _levelCompleteScreen;
        [SerializeField] private LevelFailScreen _levelFailScreen;

        [Header("Meta Screens")]
        [SerializeField] private PotionShelfView _potionShelfView;
        [SerializeField] private WorkshopView _workshopView;
        [SerializeField] private DailyBrewUI _dailyBrewUI;
        [SerializeField] private StoreUI _storeUI;
        [SerializeField] private BoosterShopUI _boosterShopUI;
        [SerializeField] private WeeklyEventView _weeklyEventView;

        [Header("Gameplay")]
        [SerializeField] private BoardPresenter _boardPresenter;
        [SerializeField] private HudController _hudController;
        [SerializeField] private LevelSelectScreen _levelSelectScreen;
        [SerializeField] private WalletUI _walletUI;

        [Header("Config")]
        [SerializeField] private EconomyConfigSO _economyConfig;
        [SerializeField] private WeeklyEventConfigSO _weeklyEventConfig;

        private PlayerProgress _progress;
        private LevelConfig _currentLevelConfig;
        private bool _isPlayingEventLevel;
        private int _currentEventLevelIndex;

        private CurrencyManager _currencyManager;
        private RewardCalculator _rewardCalculator;
        private WinStreakTracker _winStreak;
        private PotionShelfManager _potionShelf;
        private WorkshopManager _workshop;
        private DailyBrewManager _dailyBrew;
        private IAPManager _iapManager;
        private AdManager _adManager;
        private AnalyticsManager _analytics;
        private CloudSaveManager _cloudSave;
        private WeeklyEventManager _weeklyEvent;
        private RemoteConfigManager _remoteConfig;
        private NotificationManager _notificationManager;

        private void Start()
        {
            _progress = LocalSaveManager.Load();
            InitializeSystems();
            WireEvents();
            ShowLevelSelect();
        }

        private void InitializeSystems()
        {
            _currencyManager = new CurrencyManager();
            _analytics = new AnalyticsManager();
            _cloudSave = new CloudSaveManager();

            var essencePerStar = _economyConfig != null ? _economyConfig.EssencePerStar : new[] { 0, 30, 50, 80 };
            var streakTiers = _economyConfig != null
                ? _economyConfig.GetStreakTierTuples()
                : new (int, float)[] { (1, 1f), (2, 1.25f), (3, 1.5f), (5, 2f), (8, 2.5f), (10, 3f) };

            _rewardCalculator = new RewardCalculator(essencePerStar, streakTiers);
            _winStreak = new WinStreakTracker(streakTiers);

            var milestones = new[]
            {
                new PotionShelfMilestoneDefinition(25, 500, 25, "Apprentice"),
                new PotionShelfMilestoneDefinition(50, 1000, 50, "Brewer"),
                new PotionShelfMilestoneDefinition(75, 2000, 100, "Alchemist"),
                new PotionShelfMilestoneDefinition(100, 5000, 250, "Master Brewer")
            };
            _potionShelf = new PotionShelfManager(100, milestones);

            var upgrades = new[]
            {
                ("Sweep the Floor", 50), ("Light the Hearth", 100), ("Repair the Workbench", 200),
                ("Hang the Shelves", 400), ("Install the Cauldron", 600), ("Stock the Herb Rack", 1000),
                ("Place the Star Map", 1500), ("Add the Crystal Array", 2500), ("Build the Distillery", 4000),
                ("Enchant the Windows", 6000), ("Summon the Familiar", 8000), ("Master's Flourish", 12000)
            };
            _workshop = new WorkshopManager(upgrades, _currencyManager);

            _dailyBrew = new DailyBrewManager(100, 5, new[]
            {
                new DailyStreakBonusDefinition(3, 1.25f, 5),
                new DailyStreakBonusDefinition(5, 1.5f, 10),
                new DailyStreakBonusDefinition(7, 2.0f, 15)
            });

            _iapManager = new IAPManager(_currencyManager);
            _adManager = new AdManager(5, 3, () => _iapManager.HasNoAdsPass);

            _remoteConfig = new RemoteConfigManager(RemoteConfigDefaults.GetAll());
            InitializeWeeklyEvent();
            InitializeNotifications();

            InitializeViews();
        }

        private void InitializeViews()
        {
            _levelSelectScreen.Initialize(_progress);

            if (_walletUI != null) _walletUI.Initialize(_currencyManager);
            if (_potionShelfView != null) _potionShelfView.Initialize(_potionShelf);
            if (_workshopView != null) _workshopView.Initialize(_workshop);
            if (_dailyBrewUI != null) _dailyBrewUI.Initialize(_dailyBrew);
            if (_storeUI != null) _storeUI.Initialize(_iapManager, _progress.CurrentLevel);

            if (_weeklyEventView != null && _weeklyEvent != null)
                _weeklyEventView.Initialize(_weeklyEvent);

            if (_boosterShopUI != null && _boardPresenter != null)
            {
                int shakeE = _economyConfig != null ? _economyConfig.ShakeEssenceCost : 50;
                int catE = _economyConfig != null ? _economyConfig.CatalystEssenceCost : 100;
                int shakeG = _economyConfig != null ? _economyConfig.ShakeGemCost : 5;
                int catG = _economyConfig != null ? _economyConfig.CatalystGemCost : 10;
                int emG = _economyConfig != null ? _economyConfig.ExtraMovesGemCost : 10;
                _boosterShopUI.Initialize(_currencyManager, _boardPresenter.BoosterManager, shakeE, catE, shakeG, catG, emG);
            }
        }

        private void InitializeWeeklyEvent()
        {
            int levelCount = _weeklyEventConfig != null ? _weeklyEventConfig.LevelCount : 7;
            int essencePerLevel = _weeklyEventConfig != null ? _weeklyEventConfig.EssencePerLevel : 75;

            var milestones = _weeklyEventConfig != null
                ? _weeklyEventConfig.BuildMilestoneDefinitions()
                : new[]
                {
                    new MilestoneDefinition("3 Levels", 3, ("Essence", 100)),
                    new MilestoneDefinition("5 Levels", 5, ("Gems", 10)),
                    new MilestoneDefinition("All Complete", 7, ("Essence", 250), ("Gems", 25))
                };

            _weeklyEvent = new WeeklyEventManager(
                levelCount,
                essencePerLevel,
                milestones,
                () => _remoteConfig.GetBool(RemoteConfigDefaults.WeeklyEventsEnabled, true));

            TryStartWeeklyEvent();
        }

        private void TryStartWeeklyEvent()
        {
            bool eventActive = _remoteConfig.GetBool(RemoteConfigDefaults.EventActive, false);
            if (!eventActive) return;

            string eventId = _remoteConfig.GetString(RemoteConfigDefaults.EventId);
            string eventName = _remoteConfig.GetString(RemoteConfigDefaults.EventName);
            string tsStr = _remoteConfig.GetString(RemoteConfigDefaults.EventEndTimestamp, "0");
            long endTimestamp = long.TryParse(tsStr, out long ts) ? ts : 0;
            string potionId = _remoteConfig.GetString(RemoteConfigDefaults.EventPotionId);

            _weeklyEvent.StartEvent(eventId, eventName, endTimestamp, potionId);
        }

        private void InitializeNotifications()
        {
            INotificationScheduler scheduler;
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            scheduler = new UnityNotificationScheduler();
#else
            scheduler = new NullNotificationScheduler();
#endif
            _notificationManager = new NotificationManager(scheduler);
        }

        public void StartEventLevel(int eventLevelIndex)
        {
            _currentEventLevelIndex = eventLevelIndex;

            string themedId = _remoteConfig.GetString(RemoteConfigDefaults.EventThemedIngredientId);
            string path = $"levels/events/event_template_{(eventLevelIndex + 1):D2}";
            var textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
            {
                Debug.LogError($"Event level template not found: {path}");
                return;
            }

            _isPlayingEventLevel = true;

            _currentLevelConfig = LevelLoader.LoadEventLevel(textAsset.text, themedId);

            _levelSelectPanel.SetActive(false);
            _gameplayPanel.SetActive(true);
            _levelCompleteScreen.Hide();
            _levelFailScreen.Hide();

            _boardPresenter.OnLevelOutcome -= HandleLevelOutcome;
            _boardPresenter.InitializeWithLevel(_currentLevelConfig);
            _boardPresenter.OnLevelOutcome += HandleLevelOutcome;

            _hudController.Initialize(
                _boardPresenter.MoveTracker,
                _boardPresenter.ScoreCalculator,
                _boardPresenter.RecipeTracker);

            _hudController.UpdateWalletDisplay(
                _currencyManager.GetBalance(CurrencyType.Essence),
                _currencyManager.GetBalance(CurrencyType.Gems));

            _analytics.LogEventStart(_weeklyEvent.EventId, _weeklyEvent.EventName);
        }

        private void WireEvents()
        {
            _levelSelectScreen.OnLevelSelected += StartLevel;
            _levelCompleteScreen.OnNextLevel += AdvanceToNextLevel;
            _levelCompleteScreen.OnReplay += ReplayCurrentLevel;
            _levelCompleteScreen.OnDoubleRewardAd += HandleDoubleRewardAd;
            _levelFailScreen.OnRetry += ReplayCurrentLevel;
            _levelFailScreen.OnWatchAdForMoves += HandleWatchAdForMoves;
            _levelFailScreen.OnProtectStreakAd += HandleProtectStreakAd;
            _levelFailScreen.OnProtectStreakGems += HandleProtectStreakGems;
            _levelFailScreen.OnDismissStreakProtection += HandleDismissStreakProtection;

            _currencyManager.OnBalanceChanged += OnBalanceChanged;
            _winStreak.OnStreakChanged += OnStreakChanged;

            _potionShelf.OnMilestoneReached += OnMilestoneReached;
            _workshop.OnUpgradePurchased += OnWorkshopUpgraded;

            if (_dailyBrewUI != null) _dailyBrewUI.OnDailyBrewRequested += HandleDailyBrewRequest;

            if (_weeklyEventView != null)
                _weeklyEventView.OnEventLevelSelected += StartEventLevel;
        }

        private void OnDestroy()
        {
            if (_levelSelectScreen != null) _levelSelectScreen.OnLevelSelected -= StartLevel;
            if (_levelCompleteScreen != null)
            {
                _levelCompleteScreen.OnNextLevel -= AdvanceToNextLevel;
                _levelCompleteScreen.OnReplay -= ReplayCurrentLevel;
                _levelCompleteScreen.OnDoubleRewardAd -= HandleDoubleRewardAd;
            }
            if (_levelFailScreen != null)
            {
                _levelFailScreen.OnRetry -= ReplayCurrentLevel;
                _levelFailScreen.OnWatchAdForMoves -= HandleWatchAdForMoves;
                _levelFailScreen.OnProtectStreakAd -= HandleProtectStreakAd;
                _levelFailScreen.OnProtectStreakGems -= HandleProtectStreakGems;
                _levelFailScreen.OnDismissStreakProtection -= HandleDismissStreakProtection;
            }
            if (_weeklyEventView != null) _weeklyEventView.OnEventLevelSelected -= StartEventLevel;
            if (_currencyManager != null) _currencyManager.OnBalanceChanged -= OnBalanceChanged;
            if (_winStreak != null) _winStreak.OnStreakChanged -= OnStreakChanged;
            if (_potionShelf != null) _potionShelf.OnMilestoneReached -= OnMilestoneReached;
            if (_workshop != null) _workshop.OnUpgradePurchased -= OnWorkshopUpgraded;
            if (_dailyBrewUI != null) _dailyBrewUI.OnDailyBrewRequested -= HandleDailyBrewRequest;
        }

        private void ShowLevelSelect()
        {
            _isPlayingEventLevel = false;
            _levelSelectPanel.SetActive(true);
            _gameplayPanel.SetActive(false);
            _levelCompleteScreen.Hide();
            _levelFailScreen.Hide();
            _levelSelectScreen.Refresh();

            var utcNow = System.DateTime.UtcNow;
            _dailyBrew.CheckNewDay(utcNow);
            _adManager.CheckNewDay(utcNow);
            if (_dailyBrewUI != null) _dailyBrewUI.Refresh();

            if (_weeklyEventView != null && _weeklyEvent != null)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _weeklyEventView.RefreshState(_weeklyEvent.GetState(now));
            }

            if (SettingsManager.NotificationsEnabled)
            {
                _notificationManager.ScheduleDailyBrewReminder(
                    utcNow, _dailyBrew.IsCompletedToday);
            }
        }

        private void StartLevel(int levelId)
        {
            _isPlayingEventLevel = false;
            _currentLevelConfig = LevelLoader.LoadFromResources(levelId);

            _levelSelectPanel.SetActive(false);
            _gameplayPanel.SetActive(true);
            _levelCompleteScreen.Hide();
            _levelFailScreen.Hide();

            _boardPresenter.OnLevelOutcome -= HandleLevelOutcome;
            _boardPresenter.InitializeWithLevel(_currentLevelConfig);
            _boardPresenter.OnLevelOutcome += HandleLevelOutcome;

            _hudController.Initialize(
                _boardPresenter.MoveTracker,
                _boardPresenter.ScoreCalculator,
                _boardPresenter.RecipeTracker);

            _hudController.UpdateWalletDisplay(
                _currencyManager.GetBalance(CurrencyType.Essence),
                _currencyManager.GetBalance(CurrencyType.Gems));
            _hudController.UpdateStreakDisplay(_winStreak.CurrentStreak, _winStreak.CurrentMultiplier);

            _analytics.LogLevelStart(levelId);
        }

        private void HandleLevelOutcome(LevelOutcome outcome)
        {
            _boardPresenter.OnLevelOutcome -= HandleLevelOutcome;

            switch (outcome)
            {
                case LevelOutcome.Win:
                    HandleWin();
                    break;
                case LevelOutcome.Lose:
                    HandleLose();
                    break;
            }
        }

        private void HandleWin()
        {
            int score = _boardPresenter.ScoreCalculator.TotalScore;
            int bonus = _boardPresenter.MoveTracker.MovesRemaining * 50;
            int stars = ScoreCalculator.CalculateStars(score, _currentLevelConfig.StarThresholds);

            if (_isPlayingEventLevel)
            {
                HandleEventLevelWin(stars, score);
                return;
            }

            _winStreak.IncrementStreak();
            int essenceEarned = _rewardCalculator.CalculateEssence(stars, _winStreak.CurrentStreak);
            float streakMult = _winStreak.CurrentMultiplier;

            if (essenceEarned > 0)
                _currencyManager.Add(CurrencyType.Essence, essenceEarned, "level_complete");

            bool isNewPotion = _potionShelf.TryUnlockPotion(_currentLevelConfig.LevelId);
            if (isNewPotion)
            {
                var newMilestones = _potionShelf.CheckMilestones();
                foreach (var ms in newMilestones)
                {
                    _currencyManager.Add(CurrencyType.Essence, ms.EssenceReward, "shelf_milestone");
                    if (ms.GemReward > 0)
                        _currencyManager.Add(CurrencyType.Gems, ms.GemReward, "shelf_milestone");
                }
            }

            _progress.RecordLevelComplete(_currentLevelConfig.LevelId, stars);
            LocalSaveManager.Save(_progress);
            _cloudSave.MarkDirty();

            bool isTutorial = _currentLevelConfig.LevelId <= 5;
            _adManager.RecordLevelWin(isTutorial);
            bool canDouble = _adManager.CanShowRewarded(AdPlacement.DoublePotionReward);

            _analytics.LogLevelComplete(_currentLevelConfig.LevelId, stars, score,
                _boardPresenter.MoveTracker.MovesRemaining, 0);

            _levelCompleteScreen.Show(score, bonus, stars, essenceEarned, streakMult, isNewPotion, canDouble);

            ScheduleNotificationsAfterWin();

            if (_adManager.ShouldShowInterstitial())
            {
                _adManager.RecordInterstitialShown();
                _analytics.LogAdInterstitial();
            }
        }

        private void HandleEventLevelWin(int stars, int score)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            bool completed = _weeklyEvent.TryCompleteLevel(_currentEventLevelIndex, now);
            _isPlayingEventLevel = false;

            if (!completed)
            {
                ShowLevelSelect();
                return;
            }

            int essencePerLevel = _weeklyEvent.EssencePerLevel;
            if (essencePerLevel > 0)
                _currencyManager.Add(CurrencyType.Essence, essencePerLevel, "event_level");

            _analytics.LogEventLevelComplete(_weeklyEvent.EventId, _currentEventLevelIndex, stars);

            var milestoneRewards = _weeklyEvent.ClaimMilestoneRewards();
            foreach (var (type, amount) in milestoneRewards)
            {
                if (string.Equals(type, "Essence", System.StringComparison.OrdinalIgnoreCase))
                    _currencyManager.Add(CurrencyType.Essence, amount, "event_milestone");
                else if (string.Equals(type, "Gems", System.StringComparison.OrdinalIgnoreCase))
                    _currencyManager.Add(CurrencyType.Gems, amount, "event_milestone");
            }

            if (_weeklyEvent.GetState(now) == EventState.Completed)
                _analytics.LogEventComplete(_weeklyEvent.EventId, _weeklyEvent.CompletedLevelCount);

            _cloudSave.MarkDirty();

            _levelCompleteScreen.Show(score, 0, stars, essencePerLevel, 1f, false, false);
        }

        private void ScheduleNotificationsAfterWin()
        {
            if (!SettingsManager.NotificationsEnabled) return;

            var utcNow = System.DateTime.UtcNow;
            _notificationManager.ScheduleStreakAtRisk(
                utcNow, hasPlayedToday: true, _winStreak.CurrentStreak);

            if (_weeklyEvent != null)
            {
                string tsStr = _remoteConfig.GetString(RemoteConfigDefaults.EventEndTimestamp, "0");
                long endTs = long.TryParse(tsStr, out long ts) ? ts : 0;
                bool allDone = _weeklyEvent.GetState(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) == EventState.Completed;
                _notificationManager.ScheduleEventEndingSoon(utcNow, endTs, allDone);
            }
        }

        private void HandleLose()
        {
            bool canWatchAd = _adManager.CanShowRewarded(AdPlacement.FailRecovery);
            bool showStreakProtection = _winStreak.CurrentStreak >= 2;
            int streakGemCost = _economyConfig != null ? _economyConfig.StreakProtectionGemCost : 5;

            _analytics.LogLevelFail(_currentLevelConfig.LevelId,
                _currentLevelConfig.MoveLimit - _boardPresenter.MoveTracker.MovesRemaining,
                0, false);

            _levelFailScreen.Show(canWatchAd, showStreakProtection, streakGemCost);
        }

        private void HandleDoubleRewardAd()
        {
            _adManager.RecordRewardedAdWatched(AdPlacement.DoublePotionReward);
            int essenceBonus = _rewardCalculator.CalculateEssence(
                ScoreCalculator.CalculateStars(_boardPresenter.ScoreCalculator.TotalScore, _currentLevelConfig.StarThresholds),
                _winStreak.CurrentStreak);
            if (essenceBonus > 0)
                _currencyManager.Add(CurrencyType.Essence, essenceBonus, "double_reward_ad");

            _analytics.LogAdRewarded("double_potion_reward", "essence");
            _levelCompleteScreen.HideDoubleRewardButton();
        }

        private void HandleWatchAdForMoves()
        {
            _adManager.RecordRewardedAdWatched(AdPlacement.FailRecovery);
            _boardPresenter.MoveTracker.AddMoves(3);
            _analytics.LogAdRewarded("fail_recovery", "extra_moves");
            _levelFailScreen.Hide();
        }

        private void HandleProtectStreakAd()
        {
            _adManager.RecordRewardedAdWatched(AdPlacement.StreakProtection);
            _analytics.LogAdRewarded("streak_protection", "streak_preserved");
            _levelFailScreen.HideStreakProtection();
            ReplayCurrentLevel();
        }

        private void HandleProtectStreakGems()
        {
            int cost = _economyConfig != null ? _economyConfig.StreakProtectionGemCost : 5;
            if (_currencyManager.Spend(CurrencyType.Gems, cost, "streak_protection"))
            {
                _levelFailScreen.HideStreakProtection();
                ReplayCurrentLevel();
            }
        }

        private void HandleDismissStreakProtection()
        {
            _winStreak.ResetStreak();
            _analytics.LogStreakUpdate(0, 1f, true);
            _levelFailScreen.HideStreakProtection();
        }

        private void HandleDailyBrewRequest()
        {
            _analytics.LogDailyBrewStart();
            var (essence, gems) = _dailyBrew.TryComplete(System.DateTime.UtcNow);
            if (essence > 0)
                _currencyManager.Add(CurrencyType.Essence, essence, "daily_brew");
            if (gems > 0)
                _currencyManager.Add(CurrencyType.Gems, gems, "daily_brew");
            _analytics.LogDailyBrewComplete(essence, gems, _dailyBrew.CurrentDailyStreak);
            _cloudSave.MarkDirty();
        }

        private void AdvanceToNextLevel()
        {
            _levelCompleteScreen.Hide();
            int nextLevelId = _currentLevelConfig.LevelId + 1;

            if (nextLevelId <= 40)
                StartLevel(nextLevelId);
            else
                ShowLevelSelect();
        }

        private void ReplayCurrentLevel()
        {
            _levelCompleteScreen.Hide();
            _levelFailScreen.Hide();
            StartLevel(_currentLevelConfig.LevelId);
        }

        private void OnBalanceChanged(CurrencyType type, int newBalance)
        {
            _hudController.UpdateWalletDisplay(
                _currencyManager.GetBalance(CurrencyType.Essence),
                _currencyManager.GetBalance(CurrencyType.Gems));
        }

        private void OnStreakChanged(int streak, float multiplier)
        {
            _hudController.UpdateStreakDisplay(streak, multiplier);
            _analytics.LogStreakUpdate(streak, multiplier, streak == 0);
        }

        private void OnMilestoneReached(int requiredCount, int essenceReward, int gemReward, string title)
        {
            _analytics.LogPotionShelfUnlock(0, _potionShelf.BrewedCount, _potionShelf.CompletionPercent);
        }

        private void OnWorkshopUpgraded(int level, string name)
        {
            int cost = 0;
            if (_workshop.CurrentLevel > 0 && _workshop.CurrentLevel <= 12)
            {
                var costs = new[] { 50, 100, 200, 400, 600, 1000, 1500, 2500, 4000, 6000, 8000, 12000 };
                cost = costs[_workshop.CurrentLevel - 1];
            }
            _analytics.LogWorkshopUpgrade(level, name, cost);
            _cloudSave.MarkDirty();
        }
    }
}
