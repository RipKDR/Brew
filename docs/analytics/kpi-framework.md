# Brew — KPI Framework

> Owner: Analytics Lead · Review cadence: daily dashboard + weekly deep-dive
> Last updated: 2026-04-09

---

## 1. Primary KPIs (Daily Dashboard)

Reviewed every morning by the core team. Any critical-threshold breach triggers the escalation playbook (see `liveops/content-calendar.md`).

| # | KPI | Definition | Formula | Target | Warning | Critical | Frequency |
|---|-----|-----------|---------|--------|---------|----------|-----------|
| P1 | **DAU** | Unique devices with ≥ 1 session in a calendar day (UTC) | `COUNT(DISTINCT device_id) WHERE session_start IN day` | Soft-launch: 5 K+; scale: trend up WoW | < 90% of 7-day avg | < 80% of 7-day avg | Daily |
| P2 | **D1 Retention** | % of new installs who return on calendar day 1 | `users_active_on(install_date+1) / installs_on(install_date)` | ≥ 40% | < 36% | < 32% (no-go) | Daily (cohort) |
| P3 | **D7 Retention** | % of new installs who return on calendar day 7 | `users_active_on(install_date+7) / installs_on(install_date)` | ≥ 15% | < 12% | < 10% (no-go) | Daily (cohort) |
| P4 | **D30 Retention** | % of new installs who return on calendar day 30 | `users_active_on(install_date+30) / installs_on(install_date)` | ≥ 7% | < 5% | < 3% | Daily (cohort) |
| P5 | **ARPDAU** | Average revenue per daily active user | `(ad_revenue + iap_revenue) / DAU` | $0.08+ (soft-launch) | < $0.05 | < $0.03 | Daily |
| P6 | **Session Count** | Average sessions per DAU | `total_sessions / DAU` | ≥ 2.5 | < 2.0 | < 1.5 | Daily |
| P7 | **Session Length** | Median session duration in minutes | `MEDIAN(session_end.ts − session_start.ts)` | 5–8 min | < 4 min or > 15 min | < 3 min | Daily |
| P8 | **Tutorial Completion Rate** | % of installs that complete the tutorial | `tutorial_complete / tutorial_start` | ≥ 85% | < 78% | < 70% (no-go) | Daily |
| P9 | **Level Completion Rate (by level)** | % of level starts that result in a win, bucketed by level_id | `level_complete(level_id) / level_start(level_id)` | 70–85% (varies by difficulty tier) | < 60% on any level ≤ 20 | < 50% on any level ≤ 10 | Daily |
| P10 | **Revenue** | Total gross revenue split by ads and IAP | `SUM(ad_revenue) + SUM(iap_revenue)` | Trend up WoW | WoW decline > 10% | WoW decline > 25% | Daily |

---

## 2. Secondary KPIs (Weekly Review)

Reviewed every Monday in the live-ops standup. Used for tuning decisions and A/B test evaluation.

| # | KPI | Definition | Formula | Target | Warning | Critical | Frequency |
|---|-----|-----------|---------|--------|---------|----------|-----------|
| S1 | **Payer Conversion Rate** | % of DAU who made ≥ 1 IAP (lifetime) | `payers / DAU` | ≥ 2% | < 1.5% | < 1% | Weekly |
| S2 | **Rewarded Ad Opt-in Rate** | % of rewarded-ad impressions offered that are watched to completion | `rewarded_ad_watched / rewarded_ad_offered` | ≥ 60% | < 50% | < 40% | Weekly |
| S3 | **Streak Average Length** | Mean number of consecutive daily-play days for users with streaks ≥ 2 | `AVG(streak_length) WHERE streak_length >= 2` | ≥ 4 days | < 3 days | < 2 days | Weekly |
| S4 | **Workshop Progress Distribution** | Histogram of workshop upgrade stages across all active users | `PERCENTILE(workshop_stage, [25, 50, 75, 90])` | Median at stage ≥ 3 by D30 | Median < stage 2 by D30 | Median < stage 1 by D14 | Weekly |
| S5 | **Potion Collection Completion %** | Average % of total potions collected across all D30+ users | `AVG(potions_collected / total_potions)` | ≥ 30% for D30 cohort | < 20% | < 10% | Weekly |
| S6 | **Event Participation Rate** | % of DAU who start ≥ 1 event level during an active event | `event_participants / DAU` (during event window) | ≥ 35% | < 25% | < 15% | Weekly |
| S7 | **CPI by Channel** | Cost per install segmented by UA channel | `channel_spend / channel_installs` | iOS ≤ $2.50; Android ≤ $1.50 | iOS > $3.50; Android > $2.00 | iOS > $5.00; Android > $3.00 | Weekly |

---

## 3. Funnel KPIs

Measured as cumulative conversion from install. Targets represent the expected % of all installs that reach each milestone.

| Step | Milestone | Target Conversion | Warning | Critical |
|------|-----------|-------------------|---------|----------|
| 0 | Install | 100% | — | — |
| 1 | Tutorial Start | ≥ 95% | < 90% | < 85% |
| 2 | Tutorial Complete | ≥ 85% | < 78% | < 70% |
| 3 | Level 5 Complete | ≥ 75% | < 65% | < 55% |
| 4 | Level 10 Complete | ≥ 55% | < 45% | < 35% |
| 5 | Level 20 Complete | ≥ 35% | < 28% | < 20% |
| 6 | Level 30 Complete | ≥ 22% | < 17% | < 12% |
| 7 | Level 40 Complete | ≥ 14% | < 10% | < 7% |

**Measurement:** daily cohort analysis on D0 installs tracked through milestones. Each user counted once at their highest achieved step.

**Step-to-step drop-off targets:**

| Transition | Max Acceptable Drop | Action Trigger |
|------------|---------------------|----------------|
| Install → Tutorial Start | 5% | If > 10%, investigate FTUE loading / permission screens |
| Tutorial Start → Tutorial Complete | 10% | If > 15%, shorten or simplify tutorial (see A/B test 1) |
| Tutorial Complete → Level 5 | 12% | If > 20%, review early difficulty curve |
| Level 5 → Level 10 | 20% | If > 27%, review levels 6–10 difficulty / booster availability |
| Level 10 → Level 20 | 20% | Natural attrition — monitor but don't panic |
| Level 20 → Level 30 | 15% | Check economy pressure; players should have unlocked workshop |
| Level 30 → Level 40 | 10% | Long-term retention; correlate with event engagement |

---

## 4. Economy KPIs

Tracked daily, reviewed in weekly economy health check.

| # | KPI | Definition | Formula | Target | Warning | Critical | Frequency |
|---|-----|-----------|---------|--------|---------|----------|-----------|
| E1 | **Essence Earn Rate** | Average Essence earned per session | `SUM(currency_earn WHERE type='essence') / sessions` | 150–250 / session | < 100 (too stingy) or > 400 (too generous) | < 50 or > 600 | Daily |
| E2 | **Essence Spend Rate** | Average Essence spent per session | `SUM(currency_spend WHERE type='essence') / sessions` | 100–200 / session | Earn/Spend ratio < 0.8 or > 2.0 | Ratio < 0.5 or > 3.0 | Daily |
| E3 | **Gem Earn Rate** | Average Gems earned per day per DAU (free sources only) | `SUM(currency_earn WHERE type='gem' AND source!='iap') / DAU` | 5–10 / day | < 3 or > 15 | < 1 or > 25 | Daily |
| E4 | **Gem Spend Rate** | Average Gems spent per day per DAU | `SUM(currency_spend WHERE type='gem') / DAU` | 4–8 / day | Earn/Spend ratio < 0.6 or > 2.5 | Ratio < 0.3 or > 4.0 | Daily |
| E5 | **Currency Balance Distribution** | P25 / P50 / P75 / P90 of Essence and Gem balances | `PERCENTILE(balance, [25,50,75,90]) GROUP BY currency` | No single percentile > 10× median | P90 > 15× median (inflation) | P90 > 25× median | Weekly |
| E6 | **Booster Usage Rate** | % of level attempts where ≥ 1 booster is used, by level tier | `level_starts_with_booster / level_starts` | 15–30% for levels 10+ | < 10% (boosters unattractive) or > 50% (levels too hard) | < 5% or > 65% | Daily |

**Economy health heuristic:** a healthy economy shows Essence earn/spend ratio between 1.0–1.5 and Gem earn/spend ratio between 0.8–1.2. Ratios outside these ranges for 3+ consecutive days trigger an economy review.

---

## 5. Engagement KPIs

| # | KPI | Definition | Formula | Target | Warning | Critical | Frequency |
|---|-----|-----------|---------|--------|---------|----------|-----------|
| G1 | **Sessions per Day** | Average sessions per DAU | `total_sessions / DAU` | 2.5–4.0 | < 2.0 | < 1.5 | Daily |
| G2 | **Time per Session** | Median session duration (minutes) | `MEDIAN(session_duration_sec) / 60` | 5–8 min | < 4 min or > 15 min | < 3 min or > 20 min | Daily |
| G3 | **Levels per Session** | Average levels completed per session | `SUM(level_complete) / sessions` | 3–6 | < 2 | < 1 | Daily |
| G4 | **Levels per Day** | Average levels completed per DAU per day | `SUM(level_complete) / DAU` | 6–12 | < 4 | < 2 | Daily |
| G5 | **Days Active (D7)** | Average distinct active days in first 7 calendar days | `AVG(distinct_active_days) WHERE install_age <= 7` | ≥ 3.0 | < 2.5 | < 2.0 | Weekly (cohort) |
| G6 | **Days Active (D14)** | Average distinct active days in first 14 calendar days | `AVG(distinct_active_days) WHERE install_age <= 14` | ≥ 5.0 | < 4.0 | < 3.0 | Weekly (cohort) |
| G7 | **Days Active (D30)** | Average distinct active days in first 30 calendar days | `AVG(distinct_active_days) WHERE install_age <= 30` | ≥ 8.0 | < 6.0 | < 4.0 | Weekly (cohort) |

---

## 6. Monetization KPIs

| # | KPI | Definition | Formula | Target | Warning | Critical | Frequency |
|---|-----|-----------|---------|--------|---------|----------|-----------|
| M1 | **ARPDAU** | Average revenue per DAU (ads + IAP) | `total_revenue / DAU` | ≥ $0.08 | < $0.05 | < $0.03 | Daily |
| M2 | **ARPPU** | Average revenue per paying user (IAP only, 30-day window) | `iap_revenue_30d / payers_30d` | ≥ $12.00 | < $8.00 | < $5.00 | Weekly |
| M3 | **Payer Conversion %** | % of DAU with ≥ 1 lifetime IAP | `lifetime_payers / DAU` | ≥ 2.0% | < 1.5% | < 1.0% | Weekly |
| M4 | **Rewarded Ad Views / DAU** | Average rewarded-ad completions per DAU per day | `rewarded_ad_watched / DAU` | 1.5–3.0 | < 1.0 | < 0.5 | Daily |
| M5 | **Interstitial Impressions / Session** | Average interstitial ads shown per session | `interstitial_shown / sessions` | 0.8–1.5 | > 2.0 (fatigue risk) | > 3.0 | Daily |
| M6 | **IAP Revenue by SKU** | Gross IAP revenue broken down by SKU | `SUM(iap_purchase_complete.price) GROUP BY sku` | Top SKU ≥ 30% of IAP rev | Single SKU > 70% (concentration risk) | — | Weekly |
| M7 | **Ad Revenue by Placement** | Ad revenue broken down by placement ID | `SUM(ad_revenue) GROUP BY placement` | Rewarded ads ≥ 50% of ad rev | Rewarded < 40% | Rewarded < 25% | Weekly |

---

## 7. Dashboard & Alerting

### Daily Dashboard (auto-generated, sent 08:00 UTC)
- All Primary KPIs (P1–P10) with sparkline trend (7 day)
- D1 retention chart by install cohort
- Revenue waterfall: IAP vs Rewarded vs Interstitial
- Top 5 levels by fail rate (anomaly detection)

### Weekly Report (generated Monday 10:00 UTC)
- All Secondary KPIs (S1–S7)
- Full funnel chart with WoW delta
- Economy balance distribution histograms
- Active A/B test interim results (if sample > 50% of target)
- Content calendar review (upcoming week preview)

### Alert Thresholds
| Severity | Trigger | Notification | Response SLA |
|----------|---------|--------------|--------------|
| **Info** | Any KPI crosses warning threshold | Slack #brew-analytics | Acknowledge within 24 h |
| **Warning** | Any KPI stays at warning for 3+ consecutive days | Slack + email to leads | Investigation within 12 h |
| **Critical** | Any KPI crosses critical threshold | Slack + PagerDuty to on-call | War room within 4 h |
| **No-Go** | D1 < 32%, D7 < 10%, tutorial completion < 70%, retry rate < 25% | All channels + exec escalation | Decision meeting within 24 h |

---

## 8. Cohort Definitions

| Cohort | Definition | Use |
|--------|-----------|-----|
| **New User** | install_date = today | FTUE analysis |
| **D1–D7** | 1–7 days since install | Early retention |
| **D8–D30** | 8–30 days since install | Mid-term engagement |
| **D31+** | 31+ days since install | Long-term / core |
| **Payer** | ≥ 1 lifetime IAP | Monetization analysis |
| **Whale** | Lifetime IAP spend ≥ $50 | Revenue concentration |
| **Event Participant** | ≥ 1 event level started in current event | Live-ops effectiveness |

---

## 9. Level Retry Rate (Special KPI)

| KPI | Definition | Formula | Target | Warning | Critical |
|-----|-----------|---------|--------|---------|----------|
| **Level Retry Rate After Fail** | % of level_fail events followed by a level_start for the same level_id within 5 minutes | `retry_starts / level_fail` | ≥ 40% | < 32% | < 25% (no-go) |

This is a core engagement signal. Low retry rate indicates levels feel unfair, rewards for success feel insufficient, or the fail screen does not motivate replay. Cross-reference with rewarded-ad opt-in on the fail screen.
