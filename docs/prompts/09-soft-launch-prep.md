# Soft Launch Preparation

You are preparing **Brew**, a mobile puzzle game at `T:\Brew`, for soft launch. This prompt covers final milestone verification, app store compliance, analytics dashboards, A/B test configuration, UA creative preparation, submission to test markets, and the go/no-go decision framework.

---

## Step 1 — Read Source Documents

Read these files before starting any soft launch work:

1. `AGENTS.md` — project rules
2. `docs/production/milestone-gates.md` — soft launch gate criteria (all previous gates + soft launch specific)
3. `docs/legal/app-store-compliance.md` — iOS App Store and Google Play submission requirements, privacy policy, age rating, content declarations
4. `docs/creative/ua-creative-strategy.md` — UA creative specs, ASO keywords, video ad concepts, screenshot compositions
5. `docs/analytics/kpi-framework.md` — KPI definitions, target thresholds, no-go thresholds
6. `docs/analytics/ab-test-plan.md` — A/B tests planned for soft launch (Test 1 and Test 2)
7. `docs/analytics/event-tracking-plan.md` — verify all events are firing before launch
8. `docs/competitive/market-positioning.md` — positioning for store listing copy
9. `docs/project-management/session-handoff.md` — current state

---

## Step 2 — Verify All Milestone Gates Pass

Before soft launch, EVERY previous milestone gate must have passed. Run through them systematically:

### Gate 1 (Foundation — Weeks 1-2)
- Board renders, tokens spawn, tap-to-fuse works, gravity fills gaps ✓

### Gate 2 (Core Loop — Weeks 3-4)
- Chain fusion, brew detection, recipes, level loading, win/lose ✓

### Gate 3 (Feel & Juice — Weeks 5-6)
- Tutorial, art, VFX, SFX, haptics, boosters ✓

### Gate 4 (Meta & Economy — Weeks 7-8)
- Workshop, shelf, streaks, daily brew, IAP, ads, Firebase, cloud save ✓

### Gate 5 (Content & Polish — Weeks 9-10)
- 40 tuned levels, performance budget met, regression 100%, store assets, stable build ✓

If any gate has unresolved failures, stop and fix them. Do not submit a build with known gate failures.

Run the full QA pass one more time using `06-qa-testing.md` to get a clean report. The report should show zero P0 and zero P1 issues.

---

## Step 3 — App Store Compliance

Reference: `docs/legal/app-store-compliance.md`.

### 3.1 iOS App Store (Apple)

Complete this checklist in App Store Connect:

- [ ] App Information: name ("Brew"), subtitle, category (Games > Puzzle), primary language (English)
- [ ] App Privacy: complete the privacy nutrition label. Declare: Analytics (Firebase Analytics), Advertising (AdMob device identifiers), Purchases (IAP transaction data). If no account creation required (anonymous auth), declare accordingly
- [ ] Age Rating: complete the questionnaire. Expected rating: 4+ (no violence, no mature content). If ads are shown to users, declare "Frequent/Intense" for "Ads" if required
- [ ] In-App Purchases: create all IAP products in App Store Connect matching `docs/economy/monetization-plan.md` catalog. Set prices, descriptions, and review screenshots for each
- [ ] App Review Information: provide demo account if needed (anonymous auth may not require this), contact info, notes explaining gameplay
- [ ] Export Compliance: app uses encryption (HTTPS/Firebase). Complete the export compliance documentation. If only using standard HTTPS, declare accordingly

### 3.2 Google Play Store

Complete this checklist in Google Play Console:

- [ ] Store listing: title, short description, full description, graphics (icon, feature graphic, screenshots)
- [ ] Content rating: complete the IARC questionnaire. Expected: Everyone / PEGI 3
- [ ] Data safety: declare all data collected (analytics events, purchase history, device identifiers for ads). Mark Firebase Analytics and AdMob data
- [ ] Ads declaration: app contains ads — declare this
- [ ] Target audience: if game could appeal to children (puzzle game, colorful art), review Families Policy. If NOT targeting children, declare this explicitly to avoid Families Policy requirements
- [ ] In-App Products: create all IAP products matching the catalog with prices and descriptions

### 3.3 Privacy Policy & Terms of Service

- Check if privacy policy exists at `docs/legal/privacy-policy.md` or a hosted URL. If not, create one covering: data collected (analytics, purchase data, ad identifiers), how data is used, third-party services (Firebase, AdMob), data retention, user rights (deletion request), contact info
- Check if terms of service exist at `docs/legal/terms-of-service.md`. If not, create from template
- Both documents must be hosted at a publicly accessible URL before submission. Set up hosting (GitHub Pages, simple static site, or existing website)
- Link to both from within the app (Settings screen) and from the store listing

---

## Step 4 — Analytics Dashboards

Set up Firebase Analytics dashboards to monitor soft launch health:

### 4.1 Retention Dashboard
- **D1 Retention:** % of users who return 1 day after install. Target: ≥40%. No-go: <30%
- **D7 Retention:** % of users who return 7 days after install. Target: ≥20%. No-go: <12%
- **D30 Retention:** % of users who return 30 days after install. Target: ≥8%. No-go: <5%

### 4.2 Monetization Dashboard
- **ARPDAU:** Average Revenue Per Daily Active User. Target: $0.05-0.10 for soft launch
- **IAP Conversion Rate:** % of users who make at least one purchase. Target: 2-5%
- **Ad ARPDAU:** Revenue from ads per DAU
- **ARPPU:** Average Revenue Per Paying User

### 4.3 Engagement Dashboard
- **Tutorial Completion Rate:** % of new users who complete the tutorial. Target: ≥85%. No-go: <70%
- **Session Length:** Average session duration. Target: 8-15 minutes
- **Sessions Per Day:** Average daily sessions per user. Target: 2-3
- **Levels Completed Per Day:** Average across active users

### 4.4 Economy Dashboard
- **Currency Earned vs Spent:** Essence and Gem flows over time
- **Workshop Upgrade Rate:** average upgrades per user per week
- **Booster Usage Rate:** % of levels where a booster is used
- **Ad Opt-in Rate:** % of rewarded ad opportunities accepted

Reference `docs/analytics/kpi-framework.md` for the full KPI list and exact threshold definitions.

---

## Step 5 — Configure A/B Tests

Reference: `docs/analytics/ab-test-plan.md`.

Set up the first two A/B tests for soft launch using Firebase A/B Testing (backed by Remote Config):

### Test 1 (from the plan)
- Read the test definition from `docs/analytics/ab-test-plan.md` Test 1
- Configure the control and variant groups in Firebase A/B Testing
- Set the target metric (likely D7 retention or session length)
- Set minimum sample size for statistical significance (typically 1000+ users per arm)
- Set test duration (typically 7-14 days)
- Deploy via Remote Config — the app already reads all values from Remote Config, so no code changes needed

### Test 2 (from the plan)
- Same setup process as Test 1
- May run concurrently with Test 1 if they test independent variables (non-overlapping parameters)
- If they test related variables, run sequentially

Verify both tests are configured but NOT yet active. They should be activated after soft launch data starts flowing (Day 3-5).

---

## Step 6 — UA Creatives

Reference: `docs/creative/ua-creative-strategy.md`.

Prepare a minimum of 5 video ad variants for user acquisition testing:

### Video Concepts (30 seconds each)
1. **"Satisfying Cascade"** — focus on a big cascade chain reaction. No text, just gameplay. Tap → fuse → cascade → brew → celebration. Visually satisfying
2. **"Fail and Win"** — show a player failing with 1 recipe left, then using +3 moves (rewarded ad or booster), then winning. Tension → relief
3. **"Workshop Transformation"** — time-lapse of workshop going from run-down to beautiful. Show the meta progression loop
4. **"Potion Collection"** — showcase potion shelf filling up. Collector's appeal
5. **"One More Level"** — fast cuts between levels, streaks incrementing, multipliers growing. Energy and momentum

### Format Requirements
- Aspect ratios: 9:16 (portrait, primary), 1:1 (square), 16:9 (landscape)
- Duration: 15s and 30s versions of each
- First 3 seconds must hook — show the most visually appealing moment immediately
- End card with app icon, name, and CTA

### Static Creatives
- 5 static image ads in the same themes as the videos
- App store feature graphic (1024×500 for Google Play)

Export all creatives to `marketing/ua-creatives/`.

---

## Step 7 — Submit to Test Markets

### 7.1 iOS — TestFlight External Testing

1. Upload the release build to App Store Connect (should already be done from Week 10)
2. Create an external testing group for soft launch markets
3. Submit for TestFlight Beta App Review (Apple reviews external TestFlight builds)
4. Target markets: **Canada, Australia, Philippines**
   - Canada: high-value market similar to US, English-speaking
   - Australia: English-speaking, good LTV benchmark
   - Philippines: high-volume, low-CPI, good for scale testing
5. Once approved, distribute to external testers and/or enable public TestFlight link

### 7.2 Android — Closed Testing / Open Testing

1. Upload the signed AAB to Google Play Console
2. Create a Closed Testing track for the soft launch
3. Target countries: **Canada, Australia, Philippines** (same as iOS for consistent data)
4. Set up store listing for the testing track (may use draft listing)
5. Submit for review
6. Once approved, the app is available in those markets via the Play Store

### 7.3 Post-Submission Checklist

- [ ] Both builds are live in test markets
- [ ] Firebase Analytics is receiving events from real users
- [ ] Crashlytics is active and no immediate crashes
- [ ] IAP products are purchasable in test markets (verify with a test purchase)
- [ ] Ads are serving (AdMob may take 24-48 hours to ramp up)
- [ ] Remote Config is serving correct values
- [ ] Push notifications are deliverable

---

## Step 8 — Monitor Metrics Daily

For the first 2 weeks of soft launch, check metrics daily:

### Daily Monitoring Checklist

1. **Crashlytics:** Any crash-free rate below 99.5%? If yes, investigate immediately (P0)
2. **D1 retention** (available after Day 2): Is it trending toward ≥40%?
3. **Tutorial completion:** Is it ≥85%? If below 80%, the tutorial needs work — this is the biggest early lever
4. **Session metrics:** Average session length, sessions per day — are they in range?
5. **Economy health:** Are players earning and spending at expected rates? Any exploits visible?
6. **Ad metrics:** Fill rate, opt-in rate, eCPM — are ads serving and generating revenue?
7. **IAP metrics:** Any purchases yet? Conversion rate starting to form?
8. **Negative reviews:** Check store reviews daily. Address any recurring complaints
9. **Support requests:** Monitor for common issues

### Weekly Deep Dive

At the end of each week:
1. Pull full KPI report per `docs/analytics/kpi-framework.md`
2. Activate A/B tests if not yet running (start after Day 3-5 once baseline data exists)
3. Review A/B test preliminary results
4. Run economy simulation with real player data inputs if available

---

## Step 9 — Go / No-Go Decision Framework

Reference: `docs/analytics/kpi-framework.md` thresholds and `docs/production/milestone-gates.md` soft launch gate.

After 14 days of soft launch data, make the go/no-go decision:

### GO — Proceed to Global Launch
All of these must be true:
- D1 retention ≥ 40%
- D7 retention ≥ 20%
- Tutorial completion ≥ 85%
- Crash-free rate ≥ 99.5%
- No P0 or P1 bugs outstanding
- Economy pacing confirmed by real data (workshop completion trajectory in 30-45 day range)
- At least one A/B test has a clear winner

### CONDITIONAL GO — Proceed with Fixes
Some metrics below target but above no-go:
- Identify the weakest metrics
- Create a fix plan (1-2 week sprint)
- Deploy fixes to soft launch markets
- Re-evaluate after another 7 days

### NO-GO — Stop and Diagnose
Any of these trigger a stop:
- D1 retention < 30%
- Tutorial completion < 70%
- Crash-free rate < 99%
- Economy is broken (players completing workshop in <15 days or >90 days)
- Widespread negative feedback about core mechanic

If no-go is triggered:
1. Do NOT submit to additional markets
2. Analyze the data to identify root cause
3. Determine if the issue is fixable (tuning problem) or fundamental (design problem)
4. Create a remediation plan
5. Re-soft-launch after fixes

---

## Step 10 — Document and Handoff

1. Save the go/no-go decision and supporting data to `docs/production/soft-launch-report.md`
2. Update `docs/project-management/session-handoff.md`
3. Update `docs/project-management/project-summary.md` with soft launch status and key metrics
4. If proceeding to global launch, the next prompt is `10-post-launch-liveops.md`
