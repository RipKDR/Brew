# Brew — Soft Launch Plan

**Version:** 1.0
**Date:** 2026-04-09
**Owner:** Game Designer / Producer

---

## 1. Overview

Brew is ready for soft launch after completing Milestone Gate 5. The purpose of the soft launch is to validate retention, monetization, and user acquisition metrics in a controlled market before committing to global launch.

## 2. Target Markets

| Market | Rationale |
|--------|-----------|
| Canada | English-speaking, high mobile gaming engagement, representative of NA market |
| Australia | English-speaking, high ARPU, timezone diversity from NA, strong puzzle game market |
| New Zealand | Clean test market with low cost, similar demographics to Australia |

## 3. Timeline

| Phase | Duration | Dates (Estimated) |
|-------|----------|--------------------|
| Store submission | 1 week | Week 10 |
| Review + approval | 3-7 days | Week 10-11 |
| Soft launch live | 4-8 weeks | Weeks 11-18 |
| Week 2 check-in | -- | Week 13 |
| Week 4 check-in | -- | Week 15 |
| Week 8 decision | -- | Week 18 |

## 4. Budget

| Item | Amount | Notes |
|------|--------|-------|
| UA spend (Meta Ads) | $500 | Broad targeting, 3 ad creative variants |
| UA spend (Google UAC) | $500 | App campaigns for installs |
| Total UA | $1,000 | Split 50/50 between platforms |
| Ad creative production | $0 | In-house from existing gameplay captures |
| Influencer/UGC | $0 | Deferred to post-soft-launch |

## 5. UA Strategy

### Creative Mix (Soft Launch)
- 2 short video ads (15s) — "One More Fusion" and "Satisfying Chains" concepts
- 1 playable ad (20s) — Level 3 gameplay with guided tap
- Rotate weekly; pause underperformers after 48 hours

### Targeting
- **Meta:** Broad targeting, ages 25-54, interest in puzzle games, Candy Crush, Merge Mansion, cozy games
- **Google UAC:** tCPI target of $1.50, optimize for installs initially, shift to tROAS after 500 installs

### Install Target
- Minimum 2,000 installs across all markets for statistically meaningful retention data
- Target: 1,000 installs per market (Canada + Australia primary, NZ overflow)

## 6. Success Metrics

Reference: `docs/production/milestone-gates.md` — Soft Launch Gate

| Metric | Target | Minimum | Kill |
|--------|--------|---------|------|
| D1 Retention | ≥ 45% | ≥ 40% | < 32% |
| D7 Retention | ≥ 18% | ≥ 15% | < 10% |
| D30 Retention | ≥ 8% | ≥ 6% | < 4% |
| Tutorial Completion | ≥ 90% | ≥ 85% | < 70% |
| Crash Rate | < 0.5% | < 1% | > 2% |
| CPI (Meta, broad) | < $1.50 | < $2.50 | > $4.00 |
| Avg Session Length | ≥ 8 min | ≥ 5 min | < 3 min |
| Sessions/Day (D1) | ≥ 2.0 | ≥ 1.5 | < 1.2 |

## 7. Kill Criteria

Abandon if **any two** of the following are true after 2 weeks with ≥ 1,000 installs:
1. D1 retention < 32%
2. D7 retention < 10%
3. Tutorial completion < 70%
4. CPI > $4.00 on Meta (broad)

**Kill process:** GP presents data → 1-day team discussion → document learnings → archive.

## 8. Decision Checkpoints

### Week 2 (≥ 1,000 installs)
- **Review:** D1 retention, tutorial completion, crash rate, CPI
- **Actions if continuing:** Identify top 3 funnel drop-off points, plan targeted fixes
- **Possible outcomes:** Continue / Kill / Pivot tutorial

### Week 4 (≥ 2,000 installs)
- **Review:** D1 + D7 retention, session metrics, IAP conversion, ARPDAU
- **Actions if continuing:** Begin planning content updates (levels 41-60). Estimate D30 from D7 cohort curve
- **Possible outcomes:** Continue current approach / Major pivot / Kill

### Week 8 (≥ 3,000 installs, D30 data available)
- **Review:** D30 retention, LTV projection, LTV/CPI ratio
- **Green light to scale:** D1 ≥ 40%, D7 ≥ 15%, D30 ≥ 6%, LTV/CPI > 2×, crash rate < 0.5%
- **Actions if scaling:** 5× UA budget, begin levels 41-80 development, plan global launch

## 9. Technical Readiness

### Build Configuration
- iOS: IL2CPP, code stripping, production Firebase, production AdMob units
- Android: IL2CPP, signed AAB, production Firebase, production AdMob units
- Crashlytics enabled, analytics in production mode, debug logs disabled
- Remote Config live with all default values

### Monitoring Stack
- Firebase Crashlytics — real-time crash monitoring
- Firebase Analytics — event tracking and funnel analysis
- Firebase Remote Config — server-side economy tuning
- AdMob dashboard — revenue and fill rate monitoring
- App Store Connect / Google Play Console — install and retention metrics

### A/B Tests (First Sprint)
Reference: `docs/analytics/ab-test-plan.md`
1. Tutorial length (3 vs 5 levels)
2. Move budget on early levels (±20%)
3. Ad reward moves (3 vs 5)

## 10. Contingency Plans

| Scenario | Response |
|----------|----------|
| Crash rate > 1% | Halt UA spend. Emergency fix sprint. Hotfix via TestFlight/Internal Testing |
| D1 < 35% | Analyze tutorial drop-off. A/B test simplified onboarding. Adjust first 5 levels |
| CPI > $3.00 | Rotate ad creative. Test new hooks and formats. Reduce budget if no improvement after 72h |
| IAP conversion 0% | Verify sandbox → production switch. Check pricing localization. Test store kit integration |
| No D7 data after 2 weeks | Increase UA spend to $300/day for 3 days to accelerate cohort fill |

## 11. Post-Soft-Launch Artifacts

After each checkpoint, produce:
- Retention cohort chart (D1/D7/D30 by week)
- Funnel analysis (install → tutorial → L5 → L10 → L20 → D7 return)
- Revenue summary (ARPDAU, IAP conversion, ad revenue split)
- A/B test results with statistical significance
- Updated economy model based on actual player behavior
- Bug priority list for next sprint
