# Brew — Content Calendar (First 90 Days Post-Soft-Launch)

> Owner: Live-Ops Lead
> Soft-launch start: assumed **Monday, 2026-07-06** (Week 1 Day 1)
> This calendar runs through **Sunday, 2026-10-04** (Week 13)
> Last updated: 2026-04-09

---

## Calendar Overview

The first 90 days are divided into three phases:

| Phase | Weeks | Focus |
|-------|-------|-------|
| **Validate** | 1–4 | Verify core retention, economy, and tutorial. Run foundational A/B tests. Ship weekly events to test pipeline. |
| **Tune** | 5–8 | React to A/B results. Tune difficulty curve, economy, and ad frequency. Introduce streak and workshop optimization tests. |
| **Scale** | 9–13 | Lock in winning configs. Prepare for global launch. Build 4-week content buffer. Run seasonal event. |

---

## Week-by-Week Plan

### Week 1 (Jul 6–12): Soft-Launch + Baseline

| Area | Detail |
|------|--------|
| **Event** | Lunar Brew (weekly event #1) |
| **Daily Brew** | Active from Day 1 for all players who clear level 3 |
| **A/B Tests** | `AB_TUTORIAL_LENGTH` starts (enrollment Day 1). `AB_MOVE_BUDGET` starts (enrollment Day 1). |
| **Economy** | Baseline economy live. No tuning — observe only. |
| **Content Production** | Ember Festival (Week 2) already staged. Frost Market (Week 3) in QA. Bloom Season (Week 4) in design. |
| **Metrics Focus** | D1 retention (first cohort available Jul 7). Tutorial completion rate. Install → tutorial start conversion. Level 1–5 completion funnel. |

**Key milestone:** D1 retention of Jul 6 cohort available Jul 7 morning. If < 32%, trigger no-go escalation.

---

### Week 2 (Jul 13–19): Early Retention Read

| Area | Detail |
|------|--------|
| **Event** | Ember Festival (weekly event #2) |
| **Daily Brew** | Running |
| **A/B Tests** | `AB_TUTORIAL_LENGTH` and `AB_MOVE_BUDGET` continue enrolling. `AB_AD_REWARD_MOVES` starts Day 15 (Jul 20 — actually starts in Week 3; enrollment prep this week). |
| **Economy** | First economy health check (Wed Jul 15). Review Essence earn/spend ratio. |
| **Content Production** | Bloom Season (Week 4) in QA. Week 5 theme ("Twilight Alchemy") in design. |
| **Metrics Focus** | D1 trend (3 daily cohorts now). Level 5–10 completion rates. Rewarded ad opt-in baseline. Event #1 participation and completion rates. |

---

### Week 3 (Jul 20–26): First A/B Read + Ad Test

| Area | Detail |
|------|--------|
| **Event** | Frost Market (weekly event #3) |
| **Daily Brew** | Running |
| **A/B Tests** | `AB_TUTORIAL_LENGTH` reaches enrollment target (~Day 14). Begin observation window. `AB_AD_REWARD_MOVES` starts enrollment (Day 15). `AB_MOVE_BUDGET` continues. |
| **Economy** | If Essence balance P90 > 10× median, consider increasing workshop costs. Otherwise hold. |
| **Content Production** | Week 5 ("Twilight Alchemy") in QA. Week 6 ("Storm Surge") in design. |
| **Metrics Focus** | D7 retention (first cohort available Jul 13). Tutorial test interim results. Level retry rate after fail. Booster usage rates by level tier. |

**Key milestone:** D7 retention of Jul 6 cohort. If < 10%, trigger no-go escalation.

---

### Week 4 (Jul 27–Aug 2): Tutorial Decision + Economy Check

| Area | Detail |
|------|--------|
| **Event** | Bloom Season (weekly event #4) |
| **Daily Brew** | Running |
| **A/B Tests** | `AB_TUTORIAL_LENGTH` — **decision point** (Day 21). Ship winner or iterate. `AB_MOVE_BUDGET` nearing enrollment target. `AB_AD_REWARD_MOVES` continues. |
| **Economy** | Second economy review. Evaluate: are players reaching workshop by level 8–10? Is Gem earn/spend balanced? |
| **Content Production** | Week 6 ("Storm Surge") in QA. Week 7 ("Harvest Moon") in design. **Buffer check: 2 weeks of events must be fully staged.** |
| **Metrics Focus** | Tutorial A/B final results. D1 trend (now have 3+ weeks of cohorts). Event completion rate trend across 4 events. Daily Brew participation rate. Streak distribution. |

**Actions:**
- Ship tutorial test winner to 100% via Remote Config.
- If move budget test shows clear winner, prepare for Day 28 decision.

---

### Week 5 (Aug 3–9): Move Budget Decision + Streak Test

| Area | Detail |
|------|--------|
| **Event** | Twilight Alchemy (weekly event #5) |
| **Daily Brew** | Running |
| **A/B Tests** | `AB_MOVE_BUDGET` — **decision point** (Day 28). Ship or reject. `AB_AD_REWARD_MOVES` nearing enrollment target. `AB_STREAK_MULTIPLIER` starts enrollment (Day 22 — started late Week 4, full enrollment this week). |
| **Economy** | Apply move budget decision: if +20% ships, recalibrate level difficulty curve for levels 15–40. |
| **Content Production** | Week 7 ("Harvest Moon") in QA. Week 8 ("Mystic Tides") in design. |
| **Metrics Focus** | Move budget A/B final results. Ad reward A/B interim results. ARPDAU trend (now have meaningful revenue data). Payer conversion rate baseline. |

---

### Week 6 (Aug 10–16): Ad Reward Decision

| Area | Detail |
|------|--------|
| **Event** | Storm Surge (weekly event #6) |
| **Daily Brew** | Running |
| **A/B Tests** | `AB_AD_REWARD_MOVES` — **decision point** (Day 42). Ship winner. `AB_STREAK_MULTIPLIER` continues. `AB_WORKSHOP_COST` starts enrollment (Day 30). `AB_INTERSTITIAL_FREQ` starts enrollment (Day 30). |
| **Economy** | Third economy review. Comprehensive balance check: currency sinks vs sources, booster usage, workshop progression. |
| **Content Production** | Week 8 ("Mystic Tides") in QA. Week 9 ("Sunstone Fair") in design. |
| **Metrics Focus** | Ad reward A/B final results. Rewarded ad revenue impact. D30 retention (first cohort available Aug 5 — Jul 6 installs). |

**Key milestone:** D30 retention of launch cohort. If < 3%, critical. If ≥ 7%, validate.

---

### Week 7 (Aug 17–23): Mid-Phase Stabilization

| Area | Detail |
|------|--------|
| **Event** | Harvest Moon (weekly event #7) |
| **Daily Brew** | Running |
| **A/B Tests** | `AB_STREAK_MULTIPLIER` nearing enrollment target. `AB_WORKSHOP_COST` and `AB_INTERSTITIAL_FREQ` continue enrollment. |
| **Economy** | Implement any economy rebalancing from Week 6 review. |
| **Content Production** | Week 9 ("Sunstone Fair") in QA. Week 10 ("Phantom Brew") in design. Begin planning **seasonal event** for Week 11–12 (see Seasonal Planning below). |
| **Metrics Focus** | Streak test interim results. Session length and levels-per-session trends. Workshop stage distribution across D30+ users. |

---

### Week 8 (Aug 24–30): Streak Decision + Scale Prep

| Area | Detail |
|------|--------|
| **Event** | Mystic Tides (weekly event #8) |
| **Daily Brew** | Running |
| **A/B Tests** | `AB_STREAK_MULTIPLIER` — **decision point** (Day 52). Ship or reject. `AB_WORKSHOP_COST` and `AB_INTERSTITIAL_FREQ` continue. |
| **Economy** | If streak multiplier ships, model Essence inflation impact. Pre-emptively adjust sinks if needed. |
| **Content Production** | Week 10 ("Phantom Brew") in QA. Week 11 ("Equinox Elixirs" — seasonal) in design. |
| **Metrics Focus** | Streak A/B final results. D7 retention trend (should be stable or improving). Revenue mix: ads vs IAP split. CPI by channel (if UA is active). |

---

### Week 9 (Sep 1–7): Interstitial Decision + Seasonal Prep

| Area | Detail |
|------|--------|
| **Event** | Sunstone Fair (weekly event #9) |
| **Daily Brew** | Running |
| **A/B Tests** | `AB_INTERSTITIAL_FREQ` — **decision point** (Day 58). Ship or reject. `AB_WORKSHOP_COST` continues (waiting for D30 data). |
| **Economy** | Stable state. Minor tuning only. |
| **Content Production** | Week 11 ("Equinox Elixirs") entering QA. Week 12 theme in design. Week 13 theme in design. **Build 4-week buffer target: Weeks 13–16 should be in pipeline.** |
| **Metrics Focus** | Interstitial A/B final results. Revenue impact of frequency change. Prepare global-launch readiness report (D1, D7, D30, ARPDAU, tutorial CR). |

---

### Week 10 (Sep 8–14): Workshop Decision + Launch Readiness

| Area | Detail |
|------|--------|
| **Event** | Phantom Brew (weekly event #10) |
| **Daily Brew** | Running |
| **A/B Tests** | `AB_WORKSHOP_COST` — **decision point** (Day 65). Ship or reject. All 6 initial A/B tests now resolved. |
| **Economy** | Final economy lock for global launch. Document all parameter values. |
| **Content Production** | Week 12 in QA. Week 13 in design. Weeks 14–16 themes selected and assigned. |
| **Metrics Focus** | Workshop A/B final results. **Global launch readiness dashboard:** all primary KPIs must be at target or above warning. |

**Key milestone:** Go/no-go decision for global launch (typically 2–4 weeks after this point).

---

### Week 11 (Sep 15–21): Seasonal Event — Autumn Equinox

| Area | Detail |
|------|--------|
| **Event** | **Equinox Elixirs** (seasonal event — enhanced weekly format, see below) |
| **Daily Brew** | Running — special equinox-themed daily brew all week |
| **A/B Tests** | No new tests. Consolidation week. |
| **Economy** | Monitor seasonal event reward impact on balances. |
| **Content Production** | Week 13 in QA. Weeks 14–16 in pipeline. |
| **Metrics Focus** | Seasonal event participation vs regular weekly. Revenue uplift during seasonal. Engagement spike analysis. |

**Equinox Elixirs special format:**
- 10 levels instead of 7 (E1–E10).
- 2 exclusive potions (instead of 1).
- 50% bonus Essence on all level completions (event and main campaign).
- Special equinox badge for completing all 10 levels with 3 stars.

---

### Week 12 (Sep 22–28): Post-Seasonal Analysis

| Area | Detail |
|------|--------|
| **Event** | Regular weekly event ("Verdant Depths" — forest/green theme) |
| **Daily Brew** | Running |
| **A/B Tests** | Plan second wave of A/B tests based on 90-day learnings. Candidates: IAP price points, booster variety, new fail-screen flows. |
| **Economy** | Post-seasonal balance check. Did the 50% Essence bonus cause inflation? |
| **Content Production** | Weeks 14–16 in pipeline. Begin planning holiday events (Halloween, if applicable to markets). |
| **Metrics Focus** | Post-seasonal retention: did the event drive a lasting engagement bump or just a spike? Compare D7 of users who participated vs didn't. |

---

### Week 13 (Sep 29–Oct 4): 90-Day Retrospective

| Area | Detail |
|------|--------|
| **Event** | Regular weekly event ("Crystal Cavern" — purple/crystal theme) |
| **Daily Brew** | Running |
| **A/B Tests** | Second-wave test planning finalized. |
| **Economy** | 90-day economy retrospective: total Essence/Gem generated vs sunk. Identify any long-term inflation trends. |
| **Content Production** | 4-week buffer achieved (Weeks 14–17 staged). Production cadence validated. |
| **Metrics Focus** | **90-day retrospective report**: all KPIs vs targets. Funnel analysis install → level 40. Revenue projection based on soft-launch LTV curves. |

---

## Content Production Cadence

### Pipeline Stages

```
Design (Week N−3) → Localization (Week N−2.5) → QA (Week N−2) → Staged (Week N−1) → Live (Week N)
```

| Stage | Duration | Owner | Output |
|-------|----------|-------|--------|
| **Design** | 2 hours | Live-Ops Designer | Theme YAML + copy + difficulty tweaks |
| **Localization** | 2 days turnaround | Loc Vendor | Translated strings for all supported languages |
| **QA** | 1 day | QA Analyst | Play E1, E4, E7; verify rewards, visuals, analytics events |
| **Staged** | Automatic | CI/CD | Theme config deployed to staging Remote Config |
| **Live** | Automatic | Server clock | Config activates at Monday 00:00 UTC |

### Buffer Policy

- **Minimum buffer:** 2 weeks of events fully staged and QA-approved at all times.
- **Target buffer:** 4 weeks (achieved by Week 10).
- **Emergency event:** A generic "Alchemist's Challenge" template exists with neutral colors and base ingredients. Can be deployed with zero prep time if the pipeline breaks.

### Production Capacity

One designer can produce **2 weekly events per week** (4 hours total), meaning a single person can maintain the pipeline with time left for daily brew tuning, seasonal planning, and A/B test support.

---

## Seasonal Planning

### Major Dates in the 90-Day Window (Jul 6 – Oct 4, 2026)

| Date | Holiday/Event | Game Response |
|------|--------------|---------------|
| Jul 4 (before soft-launch) | US Independence Day | No action (pre-launch) |
| Aug 1 | Lammas / Start of August | No action (too niche) |
| Sep 7 | US Labor Day | Optional: "Brewer's Day Off" — double daily brew rewards for the weekend (Sep 5–7) |
| Sep 22 | Autumn Equinox | **Equinox Elixirs** seasonal event (Week 11) |
| Oct 1–4 | Start of spooky season | Tease Halloween event (ships Week 14, outside this calendar) |

### Seasonal Event Cadence (Post-90-Day Planning)

| Season | Tentative Date | Theme |
|--------|---------------|-------|
| Halloween | Oct 26 – Nov 1 | Witch's Brew — spooky potions, purple/green palette |
| Thanksgiving (US) | Nov 23 – Nov 29 | Harvest Feast — autumn colors, cornucopia ingredients |
| Winter Holiday | Dec 21 – Dec 28 | Frostfire Festival — combined ice/fire, red/blue/gold |
| Lunar New Year | Jan 25 – Feb 1, 2027 | Dragon's Brew — red/gold, dragon-scale ingredient |

Seasonal events use the same template system as weekly events but with enhanced rewards and 10 levels.

---

## Live-Ops Team Workflow

### Roles

| Role | Responsibility | Time Commitment |
|------|---------------|-----------------|
| **Live-Ops Lead** | Calendar ownership, go/no-go decisions, escalation management | 30% of time |
| **Live-Ops Designer** | Theme creation, difficulty tuning, daily brew calibration | 50% of time |
| **Analytics Lead** | KPI monitoring, A/B test analysis, economy health | 40% of time |
| **QA Analyst** | Event QA, daily brew validation, regression testing | 20% of time |
| **Engineering (on-call)** | Remote Config deploys, emergency fixes, pipeline automation | 10% of time (on-call) |

### Review Cadence

| Meeting | When | Duration | Attendees | Agenda |
|---------|------|----------|-----------|--------|
| **Daily Standup** | Mon–Fri 09:30 UTC | 15 min | All live-ops | KPI check, blockers, today's plan |
| **Weekly Review** | Monday 14:00 UTC | 60 min | All live-ops + product lead | Full KPI review, A/B test updates, content pipeline status, next week preview |
| **Economy Review** | Bi-weekly Wednesday 14:00 UTC | 45 min | Analytics Lead + Design Lead + Live-Ops Lead | Currency balance analysis, booster usage, workshop progression, reward tuning |
| **Monthly Retrospective** | First Friday of month 14:00 UTC | 90 min | Full team + stakeholders | 30-day trends, A/B test outcomes, content pipeline health, next month planning |

### Approval Process

```
Designer creates theme PR
  → Self-QA (play 3 levels)
  → Design Lead review (variety check, difficulty sanity)
  → Analytics Lead sign-off (reward values within guardrails)
  → PR merged → auto-deploy to staging
  → Live-Ops Lead final go/no-go (48 h before event start)
```

Turnaround SLA: each approval step must complete within 24 hours. Total pipeline from PR to staged: ≤ 3 business days.

---

## Escalation Triggers & Response Playbook

### Automatic Escalation Triggers

| Trigger | Severity | Who Is Notified | Response |
|---------|----------|-----------------|----------|
| D1 retention < 36% for 2 consecutive cohorts | **Warning** | Live-Ops Lead + Product Lead | Investigate: check tutorial CR, level 1–3 fail rates, any client crashes. If D1 < 32%, escalate to Critical. |
| D1 retention < 32% for any cohort | **Critical (No-Go)** | Full team + exec | War room within 4 h. Possible actions: pause UA, emergency tutorial fix, disable interstitials for new users. |
| D7 retention < 12% for 2 consecutive cohorts | **Warning** | Live-Ops Lead + Analytics Lead | Deep-dive: level completion funnel, economy pressure, ad fatigue. Cross-reference with A/B test assignments. |
| D7 retention < 10% | **Critical (No-Go)** | Full team + exec | Decision meeting within 24 h. Evaluate if soft-launch should continue. |
| Tutorial completion < 78% | **Warning** | Design Lead + Engineering | Review tutorial flow for UX bugs, confusing steps, excessive permissions. |
| Tutorial completion < 70% | **Critical (No-Go)** | Full team | Emergency fix. Ship shortest viable tutorial immediately. |
| Level retry rate < 32% | **Warning** | Design Lead | Review fail screen: is the rewarded-ad offer clear? Is the "retry" button prominent? Check if levels feel unfair. |
| Level retry rate < 25% | **Critical (No-Go)** | Full team | Emergency difficulty nerf on worst-performing levels. Add free retry incentive (e.g., first retry free). |
| ARPDAU < $0.05 for 3 consecutive days | **Warning** | Analytics Lead + Product Lead | Check ad fill rates, IAP store connectivity, rewarded ad opt-in. |
| ARPDAU < $0.03 for any day | **Critical** | Full team | Investigate ad mediation, check for ad SDK crashes, verify IAP products are live in stores. |
| Event participation < 25% | **Warning** | Live-Ops Lead | Check event visibility (is the banner showing?), entry point, eligible population size. |
| Daily Brew participation < 15% | **Warning** | Design Lead | Review difficulty (are daily brews too hard?), reward visibility, unlock gate (level 3 too late?). |
| Any A/B test guardrail triggered | **Warning** | Analytics Lead | Pause test enrollment. Evaluate if the guardrail breach is caused by the test or external factors. |
| Server error rate > 1% | **Critical** | Engineering on-call | Investigate immediately. If player-facing, disable affected feature via Remote Config. |

### Response Playbook Summary

| Situation | Immediate Action (< 4 h) | Short-Term Fix (< 24 h) | Root Cause (< 72 h) |
|-----------|--------------------------|-------------------------|---------------------|
| **Retention crisis** | Pause UA spend. Check for client crashes/ANRs. | Disable interstitials for D0 users. Add emergency Essence gift. | Analyze cohort funnels. Identify specific level or flow causing churn. |
| **Economy broken** | No immediate action unless players are stuck. | Hotfix reward values via Remote Config. | Full earn/spend audit. Model long-term balance impact. |
| **Event broken** | Swap to emergency "Alchemist's Challenge" template. | Fix and re-deploy broken event. | Post-mortem: why did QA miss it? |
| **Ad revenue collapse** | Check ad SDK dashboard for outages. | Switch mediation priority. Enable backup network. | Contact ad network partner. Review waterfall config. |
| **A/B test causing harm** | Pause enrollment. Do NOT roll back existing users (data integrity). | If guardrail breach is severe, force all users to control variant. | Analyze what went wrong. Document in test log. |

---

## Appendix: Event Theme Rotation (Weeks 1–13)

| Week | Dates | Event Theme | Notes |
|------|-------|-------------|-------|
| 1 | Jul 6–12 | Lunar Brew | Launch week |
| 2 | Jul 13–19 | Ember Festival | |
| 3 | Jul 20–26 | Frost Market | |
| 4 | Jul 27–Aug 2 | Bloom Season | |
| 5 | Aug 3–9 | Twilight Alchemy | New theme |
| 6 | Aug 10–16 | Storm Surge | |
| 7 | Aug 17–23 | Harvest Moon | |
| 8 | Aug 24–30 | Mystic Tides | |
| 9 | Sep 1–7 | Sunstone Fair | Labor Day weekend — optional double daily brew rewards |
| 10 | Sep 8–14 | Phantom Brew | |
| 11 | Sep 15–21 | **Equinox Elixirs** | Seasonal (10 levels, enhanced rewards) |
| 12 | Sep 22–28 | Verdant Depths | |
| 13 | Sep 29–Oct 4 | Crystal Cavern | 90-day mark; tease Halloween |
