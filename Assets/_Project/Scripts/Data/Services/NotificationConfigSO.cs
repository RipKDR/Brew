using UnityEngine;

namespace Brew.Data.Services
{
    [CreateAssetMenu(fileName = "NotificationConfig", menuName = "Brew/Config/Notification Config")]
    public class NotificationConfigSO : ScriptableObject
    {
        [Header("Scheduling Limits")]
        [SerializeField] private int _maxNotificationsPerDay = 1;
        [SerializeField] private int _rePromptCooldownDays = 7;

        [Header("Daily Brew")]
        [SerializeField] private int _dailyBrewHourOffset = 1;
        [SerializeField] private string _dailyBrewTitle = "Your daily brew awaits!";
        [SerializeField] private string _dailyBrewBody = "A fresh challenge is ready. Keep your streak alive!";

        [Header("Streak At Risk")]
        [SerializeField] private int _streakRiskLocalHour = 20;
        [SerializeField] private string _streakAtRiskTitle = "Your streak is at risk!";
        [SerializeField] private string _streakAtRiskBody = "Play today to keep your win streak going.";

        [Header("Event Ending Soon")]
        [SerializeField] private int _eventEndingHoursBeforeEnd = 24;
        [SerializeField] private string _eventEndingTitle = "Event ending soon!";
        [SerializeField] private string _eventEndingBody = "Complete the remaining event levels before time runs out.";

        public int MaxNotificationsPerDay => _maxNotificationsPerDay;
        public int RePromptCooldownDays => _rePromptCooldownDays;
        public int DailyBrewHourOffset => _dailyBrewHourOffset;
        public int StreakRiskLocalHour => _streakRiskLocalHour;
        public int EventEndingHoursBeforeEnd => _eventEndingHoursBeforeEnd;
        public string DailyBrewTitle => _dailyBrewTitle;
        public string DailyBrewBody => _dailyBrewBody;
        public string StreakAtRiskTitle => _streakAtRiskTitle;
        public string StreakAtRiskBody => _streakAtRiskBody;
        public string EventEndingTitle => _eventEndingTitle;
        public string EventEndingBody => _eventEndingBody;
    }
}
