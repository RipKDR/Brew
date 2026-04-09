# Post-Launch Live Operations

You are running live operations for **Brew**, a mobile puzzle game at `T:\Brew`. The game is live in production. This prompt covers the ongoing operational cadence: weekly events, KPI monitoring, A/B testing, economy tuning, content creation, escalation response, and monthly content arcs.

---

## Step 1 — Read Source Documents

Read these files before performing any live ops work:

1. `AGENTS.md` — project rules (these still apply post-launch)
2. `docs/liveops/content-calendar.md` — the 90-day content calendar: week-by-week plan of events, A/B tests, content drops, and seasonal themes
3. `docs/liveops/event-templates.md` — how the weekly event system works: theme configuration, level requirements, reward structure, Remote Config keys
4. `docs/analytics/kpi-framework.md` — KPI definitions, target thresholds, warning thresholds, no-go thresholds, and escalation triggers
5. `docs/analytics/ab-test-plan.md` — the full A/B test schedule with hypothesis, variants, metrics, and sample size requirements
6. `docs/economy/economy-model.md` — current economy values and pacing targets
7. `docs/project-management/session-handoff.md` — what happened last session, what's in progress

---

## Step 2 — Determine Current Week

Check the content calendar in `docs/liveops/content-calendar.md` and determine which week of the 90-day plan you're in. The calendar should specify:

- Which weekly event is scheduled
- Which A/B test should be running or concluding
- Any content drops (new levels, new potion families)
- Any seasonal themes starting or ending
- Any special promotions or sales

If the calendar doesn't cover the current week (past the 90-day plan), extend it following the established pattern: alternate event themes, maintain variety, align with real-world seasons/holidays.

---

## Step 3 — Deploy Weekly Event

Follow this process every week to deploy a new event:

### 3.1 Prepare Event Content

1. Read the event template from `docs/liveops/event-templates.md` for this week's theme
2. Create 7 event levels using the `08-level-design.md` prompt's event level section:
   - Levels 1-2: Easy (introduce theme)
   - Levels 3-5: Medium (core challenge)
   - Level 6: Hard (climax)
   - Level 7: Easy-medium (finale)
3. Generate levels:
```
python scripts/level-generator/generate_level.py --level event_1 --difficulty easy --event-theme <THEME_ID>
python scripts/level-generator/generate_level.py --level event_2 --difficulty easy --event-theme <THEME_ID>
python scripts/level-generator/generate_level.py --level event_3 --difficulty medium --event-theme <THEME_ID>
python scripts/level-generator/generate_level.py --level event_4 --difficulty medium --event-theme <THEME_ID>
python scripts/level-generator/generate_level.py --level event_5 --difficulty medium --event-theme <THEME_ID>
python scripts/level-generator/generate_level.py --level event_6 --difficulty hard --event-theme <THEME_ID>
python scripts/level-generator/generate_level.py --level event_7 --difficulty medium --event-theme <THEME_ID>
```
4. Save event levels to `Assets/Resources/Events/<event_id>/`
5. Validate: `python scripts/build/build_config.py --validate-levels`
6. Playtest all 7 levels — they should be completable in 20-30 minutes total

### 3.2 Configure Remote Config

Update Firebase Remote Config to activate the event:

| Key | Value |
|---|---|
| `event_active` | `true` |
| `event_id` | `<event_id>` (e.g., `crystal_caves`) |
| `event_name` | Display name (e.g., `"Crystal Caves"`) |
| `event_start_timestamp` | Unix timestamp for event start |
| `event_end_timestamp` | Unix timestamp for event end (7 days later) |
| `event_potion_id` | ID of the exclusive event potion |
| `event_gem_reward` | Gems granted for completing all 7 event levels |
| `event_theme_palette` | Color palette reference |

### 3.3 Deploy

- If event levels are bundled in the app: deploy a new build with the event content via the CI/CD pipeline (reference `docs/technical/infrastructure.md`)
- If event levels are loaded from server/CDN: upload the level configs and update the event manifest
- Publish the Remote Config changes
- Verify: launch the app, confirm the event banner appears on the main menu, confirm event levels load and play correctly

### 3.4 Monitor Event Performance

During the event week, track:
- Event participation rate (% of DAU who play at least 1 event level)
- Event completion rate (% of participants who finish all 7 levels)
- Event potion claim rate
- Impact on daily session length and sessions per day
- Any event-related crashes or bugs

Target: 40-60% participation, 50-70% completion among participants.

---

## Step 4 — Monitor KPIs Daily

Reference: `docs/analytics/kpi-framework.md`.

Check these metrics every day (or every session when you're actively managing the game):

### Critical Metrics (check immediately)
| Metric | Healthy | Warning | Escalation |
|---|---|---|---|
| Crash-free rate | ≥99.5% | 99.0-99.5% | <99.0% — hotfix immediately |
| DAU trend | Stable or growing | Declining 5-10% WoW | Declining >10% WoW |
| D1 retention | ≥40% | 35-40% | <35% |

### Daily Metrics
| Metric | Target | Action if Below |
|---|---|---|
| D1 retention | ≥40% | Investigate tutorial, first session experience |
| D7 retention | ≥20% | Investigate meta loop engagement, content pacing |
| Tutorial completion | ≥85% | Simplify tutorial, check for confusion points |
| Session length | 8-15 min | Check content depth, difficulty pacing |
| ARPDAU | $0.05-0.15 | Review ad fill rates, IAP conversion |

### Weekly Metrics
| Metric | Target | Action if Below |
|---|---|---|
| D30 retention | ≥8% | Long-term engagement issues — meta systems, content variety |
| IAP conversion | 2-5% | Review store UI, offer timing, value perception |
| Workshop upgrade rate | 2-3/week/user | Economy pacing issue — see economy tuning prompt |
| Event participation | 40-60% of DAU | Event visibility, reward appeal, difficulty |

### Escalation Protocol

If a metric hits the escalation threshold:
1. Verify the data is correct (not a tracking bug)
2. Identify the likely cause (recent change, external event, seasonal pattern)
3. If caused by a recent change: roll back via Remote Config if possible, or deploy a hotfix
4. If cause is unclear: run deeper analysis, cross-reference with other metrics
5. Document the incident and response in `docs/liveops/incident-log.md`

---

## Step 5 — Execute A/B Tests

Reference: `docs/analytics/ab-test-plan.md`.

Follow the A/B test schedule from the plan. For each test:

### Starting a Test
1. Read the test definition: hypothesis, control values, variant values, target metric, minimum sample size, test duration
2. Configure in Firebase A/B Testing:
   - Create experiment with control and variant(s)
   - Set target metric
   - Set audience (% of users, new users only vs. all users)
   - Set start and end dates
3. Activate the experiment
4. Do NOT change any related Remote Config values while the test is running

### Monitoring a Test
- Check daily: is the test receiving sufficient traffic? Is there early signal?
- Do NOT stop a test early unless there's a clear negative impact on user experience (crash, major bug)
- Wait for statistical significance (p < 0.05) before declaring a winner

### Concluding a Test
1. Review results when the test reaches its planned end date or sufficient sample size
2. If a variant wins with statistical significance:
   - Update `docs/analytics/ab-test-plan.md` with the result
   - Roll the winning variant to 100% of users via Remote Config
   - Update `docs/economy/economy-model.md` or relevant spec to reflect the new default
3. If no significant difference: keep the control (simpler) and document the null result
4. If the variant loses: keep the control and document learnings

---

## Step 6 — Economy Tuning

Use the `07-economy-tuning.md` prompt whenever economy adjustments are needed. Triggers for economy tuning:

- Workshop completion pace is outside 30-45 day range based on real player data
- Players are accumulating too much or too little currency
- IAP conversion is below target (may indicate free currency is too generous or too scarce)
- Booster usage is extremely high (>50% of levels) or extremely low (<5% of levels)
- Player feedback indicates grind or paywall frustration

When tuning with real data, use actual player behavior instead of simulation assumptions:
- What's the real average levels-per-day? (May differ from the assumed 10)
- What's the real star distribution? (May differ from assumed even split)
- What's the real ad opt-in rate? (May differ from assumed 2/day)

Feed these actuals into the simulation for more accurate projections.

---

## Step 7 — Create New Content

As the live game needs fresh content, use the `08-level-design.md` prompt to create:

### New Campaign Levels
- If the game launches with 40 levels and retention data shows players completing them in ~3 weeks, plan for 10 new levels every 2-3 weeks
- New levels extend the existing difficulty curve — don't reset to easy
- Each batch of 10 should include 2 breather levels and 1 milestone level

### New Event Themes
- Rotate through event themes to maintain freshness
- Target: at least 8 unique event themes before repeating
- Seasonal events (Halloween, Winter, etc.) get special treatment — unique ingredients, exclusive potions, higher rewards

### New Potion Families
- Per the content calendar, introduce new potion families periodically
- New potions expand the shelf collection (adds long-term goals)
- New potions may require new ingredient combinations or new recipe types

---

## Step 8 — Respond to Escalation Triggers

When metrics drop, follow this playbook from the content calendar:

### Retention Drop
1. Check if caused by: app update bug, seasonal dip, content drought, difficulty spike
2. Quick fixes: adjust difficulty via Remote Config, launch a special event, increase rewards temporarily
3. If structural: investigate the specific drop-off point (which session, which level, which feature)

### Revenue Drop
1. Check ad fill rates (network issue?) and IAP conversion
2. Review recent Remote Config changes — did you accidentally reduce monetization?
3. Check competitive landscape — new competitor launch?
4. Quick fix: limited-time IAP sale, bonus ad rewards

### Engagement Drop (Sessions/Length)
1. Check content freshness — has it been too long since new levels or an event?
2. Check push notification delivery — are reminders working?
3. Launch an event or content drop to re-engage

### Crash Spike
1. Immediate priority — identify the crash in Crashlytics
2. Determine: is it in new code (recent update) or old code triggered by new conditions?
3. If new code: roll back the update or deploy a hotfix within 24 hours
4. If old code: investigate the trigger, deploy a fix in the next scheduled update

---

## Step 9 — Monthly Content Arcs

At the start of each month, plan the month's content arc:

1. **Theme:** What's the monthly narrative? (e.g., "Autumn Harvest" — warm colors, harvest-themed potions, pumpkin ingredients)
2. **Events:** 4 weekly events, at least 2 with the monthly theme, 1-2 standalone variety events
3. **Content Drop:** New campaign levels (10 levels mid-month), new potion family
4. **Economy Adjustment:** Review real data, make any tuning adjustments
5. **A/B Tests:** 1-2 tests per month per the plan
6. **UA:** Refresh ad creatives to feature monthly theme

Document the monthly plan in `docs/liveops/monthly-plans/<month>.md`.

---

## Step 10 — Track Production Overhead

A sustainable live ops cadence is essential. Track how long each weekly cycle takes:

| Task | Target Time | If Exceeding |
|---|---|---|
| Create 7 event levels | <1 hour | Improve generator tooling, create more templates |
| Configure Remote Config | <15 min | Script the deployment |
| QA the event | <30 min | Automate event level validation |
| Monitor daily KPIs | <15 min/day | Build automated alert dashboards |
| Total weekly overhead | <2 hours | If consistently over, invest in automation |

If weekly event production consistently exceeds 2 hours:
1. Identify the bottleneck (level creation? QA? config deployment?)
2. Invest in tooling: better generators, automated validation, one-click deployment scripts
3. Consider pre-producing events in batches (create 4 weeks of events in one sitting)

The goal is sustainable operations that one person can manage alongside other development work.

---

## Step 11 — Session Completion

Before ending this session:

1. Update `docs/project-management/session-handoff.md` with:
   - Current week in the content calendar
   - What was deployed this session (event, config change, content)
   - Current KPI readings and any concerns
   - Next scheduled actions (next event, A/B test phase, content drop)
2. Update any changed specs (economy model, content calendar, test plan results)
3. If a monthly plan was created, save to `docs/liveops/monthly-plans/`
4. Run `python scripts/context/build_context_snapshot.py --project-root .`
