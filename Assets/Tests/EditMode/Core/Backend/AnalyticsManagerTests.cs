using System;
using System.Collections.Generic;
using Brew.Core.Backend;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class AnalyticsManagerTests
    {
        private AnalyticsManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new AnalyticsManager();
        }

        [Test]
        public void LogLevelStart_RecordsEventWithCorrectNameAndLevelId()
        {
            _manager.LogLevelStart(7);

            var log = _manager.GetEventLog();
            Assert.AreEqual(1, log.Count);
            Assert.AreEqual("level_start", log[0].name);
            Assert.AreEqual(7, log[0].parameters["level_id"]);
        }

        [Test]
        public void LogLevelComplete_RecordsAllFiveParameters()
        {
            _manager.LogLevelComplete(3, 2, 1500, 4, 120);

            var p = _manager.GetEventLog()[0].parameters;
            Assert.AreEqual("level_complete", _manager.GetEventLog()[0].name);
            Assert.AreEqual(3, p["level_id"]);
            Assert.AreEqual(2, p["star_rating"]);
            Assert.AreEqual(1500, p["score"]);
            Assert.AreEqual(4, p["moves_remaining"]);
            Assert.AreEqual(120, p["duration_seconds"]);
        }

        [Test]
        public void LogLevelFail_RecordsAllFourParameters()
        {
            _manager.LogLevelFail(5, 12, 3, true);

            var p = _manager.GetEventLog()[0].parameters;
            Assert.AreEqual("level_fail", _manager.GetEventLog()[0].name);
            Assert.AreEqual(5, p["level_id"]);
            Assert.AreEqual(12, p["moves_used"]);
            Assert.AreEqual(3, p["recipe_remaining"]);
            Assert.AreEqual(true, p["near_miss"]);
        }

        [Test]
        public void LogCurrencyEarned_RecordsCorrectFields()
        {
            _manager.LogCurrencyEarned("gems", 50, "level_reward", 200);

            var p = _manager.GetEventLog()[0].parameters;
            Assert.AreEqual("currency_earned", _manager.GetEventLog()[0].name);
            Assert.AreEqual("gems", p["currency_type"]);
            Assert.AreEqual(50, p["amount"]);
            Assert.AreEqual("level_reward", p["source"]);
            Assert.AreEqual(200, p["balance_after"]);
        }

        [Test]
        public void LogCurrencySpent_RecordsCorrectFields()
        {
            _manager.LogCurrencySpent("essence", 30, "booster_shop", 70);

            var p = _manager.GetEventLog()[0].parameters;
            Assert.AreEqual("currency_spent", _manager.GetEventLog()[0].name);
            Assert.AreEqual("essence", p["currency_type"]);
            Assert.AreEqual(30, p["amount"]);
            Assert.AreEqual("booster_shop", p["sink"]);
            Assert.AreEqual(70, p["balance_after"]);
        }

        [Test]
        public void LogBoosterUsed_RecordsCorrectFields()
        {
            _manager.LogBoosterUsed("shake", 2);

            var p = _manager.GetEventLog()[0].parameters;
            Assert.AreEqual("booster_used", _manager.GetEventLog()[0].name);
            Assert.AreEqual("shake", p["booster_type"]);
            Assert.AreEqual(2, p["charges_remaining"]);
        }

        [Test]
        public void LogAdRewarded_RecordsPlacementAndRewardType()
        {
            _manager.LogAdRewarded("end_level", "extra_moves");

            var p = _manager.GetEventLog()[0].parameters;
            Assert.AreEqual("ad_rewarded", _manager.GetEventLog()[0].name);
            Assert.AreEqual("end_level", p["placement"]);
            Assert.AreEqual("extra_moves", p["reward_type"]);
        }

        [Test]
        public void LogAdInterstitial_RecordsEventWithEmptyParameters()
        {
            _manager.LogAdInterstitial();

            var entry = _manager.GetEventLog()[0];
            Assert.AreEqual("ad_interstitial", entry.name);
            Assert.IsNotNull(entry.parameters);
            Assert.AreEqual(0, entry.parameters.Count);
        }

        [Test]
        public void OnEventLogged_FiresWithCorrectEventNameAndParameters()
        {
            string receivedName = null;
            Dictionary<string, object> receivedParams = null;
            _manager.OnEventLogged += (name, parameters) =>
            {
                receivedName = name;
                receivedParams = parameters;
            };

            _manager.LogLevelStart(9);

            Assert.AreEqual("level_start", receivedName);
            Assert.IsNotNull(receivedParams);
            Assert.AreEqual(9, receivedParams["level_id"]);
        }

        [Test]
        public void GetEventLog_ReturnsAllLoggedEventsInOrder()
        {
            _manager.LogLevelStart(1);
            _manager.LogAdInterstitial();
            _manager.LogLevelFail(2, 1, 0, false);

            var log = _manager.GetEventLog();
            Assert.AreEqual(3, log.Count);
            Assert.AreEqual("level_start", log[0].name);
            Assert.AreEqual("ad_interstitial", log[1].name);
            Assert.AreEqual("level_fail", log[2].name);
        }

        [Test]
        public void MultipleEvents_AccumulateCorrectly()
        {
            _manager.LogCurrencyEarned("gems", 10, "a", 10);
            _manager.LogCurrencySpent("gems", 5, "b", 5);
            _manager.LogBoosterUsed("catalyst", 1);

            var log = _manager.GetEventLog();
            Assert.AreEqual(3, log.Count);
            Assert.AreEqual("currency_earned", log[0].name);
            Assert.AreEqual("currency_spent", log[1].name);
            Assert.AreEqual("booster_used", log[2].name);
        }

        [Test]
        public void NullStringParameters_DefaultToStringEmpty()
        {
            _manager.LogCurrencyEarned(null, 1, null, 0);
            _manager.LogCurrencySpent(null, 1, null, 0);
            _manager.LogIAPPurchase(null, 0f, false);
            _manager.LogAdRewarded(null, null);
            _manager.LogWorkshopUpgrade(1, null, 10);
            _manager.LogBoosterUsed(null, 0);
            _manager.LogEventStart(null, null);
            _manager.LogEventComplete(null, 0);

            var log = _manager.GetEventLog();
            Assert.AreEqual(8, log.Count);

            Assert.AreEqual(string.Empty, log[0].parameters["currency_type"]);
            Assert.AreEqual(string.Empty, log[0].parameters["source"]);

            Assert.AreEqual(string.Empty, log[1].parameters["currency_type"]);
            Assert.AreEqual(string.Empty, log[1].parameters["sink"]);

            Assert.AreEqual(string.Empty, log[2].parameters["product_id"]);

            Assert.AreEqual(string.Empty, log[3].parameters["placement"]);
            Assert.AreEqual(string.Empty, log[3].parameters["reward_type"]);

            Assert.AreEqual(string.Empty, log[4].parameters["upgrade_name"]);

            Assert.AreEqual(string.Empty, log[5].parameters["booster_type"]);

            Assert.AreEqual(string.Empty, log[6].parameters["event_id"]);
            Assert.AreEqual(string.Empty, log[6].parameters["event_name"]);

            Assert.AreEqual(string.Empty, log[7].parameters["event_id"]);
        }

        [Test]
        public void LogBoardReshuffle_RecordsLevelRecoveredAndReason()
        {
            _manager.LogBoardReshuffle(23, true, "deadlock");

            var entry = _manager.GetEventLog()[0];
            Assert.AreEqual("board_reshuffle", entry.name);
            Assert.AreEqual(23, entry.parameters["level_id"]);
            Assert.AreEqual(true, entry.parameters["recovered"]);
            Assert.AreEqual("deadlock", entry.parameters["reason"]);
        }
    }
}
