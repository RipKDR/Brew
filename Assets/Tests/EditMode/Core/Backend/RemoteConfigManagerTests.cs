using System;
using System.Collections.Generic;
using Brew.Core.Backend;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class RemoteConfigManagerTests
    {
        [Test]
        public void GetInt_ReturnsDefaultValue_FromConstructorDefaults()
        {
            var defaults = new Dictionary<string, object> { { "stars", 3 } };
            var mgr = new RemoteConfigManager(defaults);

            Assert.AreEqual(3, mgr.GetInt("stars"));
        }

        [Test]
        public void GetFloat_ReturnsDefaultValue_FromConstructorDefaults()
        {
            var defaults = new Dictionary<string, object> { { "mul", 1.5f } };
            var mgr = new RemoteConfigManager(defaults);

            Assert.AreEqual(1.5f, mgr.GetFloat("mul"), 0.0001f);
        }

        [Test]
        public void ApplyServerValues_OverridesDefaults()
        {
            var defaults = new Dictionary<string, object> { { "a", 1 }, { "b", 2 } };
            var mgr = new RemoteConfigManager(defaults);

            mgr.ApplyServerValues(new Dictionary<string, object> { { "a", 100 } });

            Assert.AreEqual(100, mgr.GetInt("a"));
            Assert.AreEqual(2, mgr.GetInt("b"));
        }

        [Test]
        public void GetInt_FallsBackWhenKeyMissing()
        {
            var mgr = new RemoteConfigManager(new Dictionary<string, object>());

            Assert.AreEqual(42, mgr.GetInt("missing", 42));
        }

        [Test]
        public void IsStale_ReturnsTrue_WhenNeverFetched()
        {
            var mgr = new RemoteConfigManager(new Dictionary<string, object> { { "k", 1 } });

            Assert.IsTrue(mgr.IsStale);
            Assert.IsNull(mgr.LastFetchUtc);
        }

        [Test]
        public void MarkFetched_ClearsStaleStatus()
        {
            var mgr = new RemoteConfigManager(new Dictionary<string, object> { { "k", 1 } });

            mgr.MarkFetched(DateTime.UtcNow);

            Assert.IsFalse(mgr.IsStale);
            Assert.IsTrue(mgr.LastFetchUtc.HasValue);
        }
    }
}
