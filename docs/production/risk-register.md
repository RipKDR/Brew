# Brew — Risk Register

**Version:** 1.0  
**Last updated:** 2026-04-09  
**Owner:** Game Designer / Producer  

---

## Scoring Guide

| Score | Likelihood                        | Impact                                   |
| ----- | --------------------------------- | ---------------------------------------- |
| 1     | Very unlikely (< 10%)            | Negligible — minor inconvenience         |
| 2     | Unlikely (10–25%)                | Low — delays ≤ 2 days, localized issue   |
| 3     | Possible (25–50%)                | Moderate — delays 3–5 days, affects milestone |
| 4     | Likely (50–75%)                  | High — delays 1–2 weeks, rework required |
| 5     | Very likely (> 75%)              | Critical — project viability threatened  |

**Risk Score** = Likelihood × Impact. Scores ≥ 12 require active mitigation. Scores ≥ 16 require contingency plan.

---

## Risk Register

### R-01: Core loop not fun enough to retain players

| Field       | Value |
| ----------- | ----- |
| **Category**    | Design |
| **Description** | The cluster-tap → fuse → brew mechanic fails to produce satisfying gameplay. Players understand it but don't find it compelling. D1 retention falls below 40%. |
| **Likelihood**  | 3 |
| **Impact**      | 5 |
| **Risk Score**   | 15 |
| **Mitigation**  | Run external playtests at Gate 3 (end of Week 6) with 5 non-team members. Measure: do ≥ 3 of 5 voluntarily continue playing past the tutorial? If not, invest 1 week in feel polish: tighten animation timing (fusion < 0.2 s), amplify chain/brew SFX and particles, add combo multiplier text popups. If second playtest still fails, convene a pivot meeting to evaluate mechanic redesign before investing in meta systems. |
| **Owner**       | GP |
| **Status**      | Open |

---

### R-02: Difficulty curve too steep — players churn at mid-game

| Field       | Value |
| ----------- | ----- |
| **Category**    | Design |
| **Description** | Levels 15–25 are significantly harder than levels 1–14, causing a difficulty cliff. Players who enjoyed the early game abandon during mid-game. Manifests as a completion rate drop from >80% to <40% between level bands. |
| **Likelihood**  | 4 |
| **Impact**      | 3 |
| **Risk Score**   | 12 |
| **Mitigation**  | GP playtests all 40 levels sequentially during Week 9 with target completion rates per band (L1–10: 95%, L11–25: 80%, L26–35: 65%, L36–40: 45%). If any 3 consecutive levels exceed a 15-percentage-point drop from their band target, add +2 moves and re-test. Use Firebase Remote Config to adjust move limits post-launch without a client update. Track per-level completion rates in analytics from Day 1 of soft launch. |
| **Owner**       | GP |
| **Status**      | Open |

---

### R-03: Difficulty curve too flat — no challenge, players bored

| Field       | Value |
| ----------- | ----- |
| **Category**    | Design |
| **Description** | Players complete all 40 levels too easily. No level requires more than 1 attempt. Game feels like a checklist rather than a challenge. Average session length drops below 5 minutes because there's no friction. |
| **Likelihood**  | 2 |
| **Impact**      | 4 |
| **Risk Score**   | 8 |
| **Mitigation**  | Track average attempts per level in analytics. If levels 26–40 average < 1.2 attempts, tighten move limits by 15% via Remote Config. Introduce "hard mode" recipes requiring specific brew orders (e.g., brew fire potion before ice potion). Reserve design space for 3-star challenges that require perfect play. |
| **Owner**       | GP |
| **Status**      | Open |

---

### R-04: Performance issues on low-end devices during cascades

| Field       | Value |
| ----------- | ----- |
| **Category**    | Technical |
| **Description** | Large cascades (10+ chain steps) with simultaneous particle systems, animations, and SFX cause frame drops below 20 fps on low-end Android devices (2 GB RAM, Snapdragon 4xx). Players perceive the game as laggy. |
| **Likelihood**  | 4 |
| **Impact**      | 3 |
| **Risk Score**   | 12 |
| **Mitigation**  | Implement quality tiers detected at startup via `SystemInfo.systemMemorySize` and `SystemInfo.processorFrequency`. Low tier: cap simultaneous particles at 3 systems, reduce particle count per system by 50%, disable screen shake, reduce cascade animation resolution. Profile on iPhone SE 2nd gen and a 2 GB Android device weekly starting Week 5. Set cascade cap at 20 to prevent unbounded chains. Use object pooling for all particles and token GameObjects. |
| **Owner**       | UD-1 |
| **Status**      | Open |

---

### R-05: Firebase Firestore costs spike unexpectedly

| Field       | Value |
| ----------- | ----- |
| **Category**    | Technical |
| **Description** | Cloud save writes per session exceed Firestore free tier limits. At scale (10,000+ DAU), monthly costs exceed $500 — unacceptable for a soft launch with minimal revenue. |
| **Likelihood**  | 3 |
| **Impact**      | 3 |
| **Risk Score**   | 9 |
| **Mitigation**  | Limit cloud save writes to 1 per level completion (not per action). Batch multiple field updates into a single Firestore write. Use local save as primary, sync to cloud only on level complete, daily brew complete, and app backgrounding. Set up Firebase budget alerts at $50 and $200 thresholds. During soft launch, monitor Firestore usage dashboard daily for the first week. If costs trend above $100/month at 2,000 DAU, switch to a custom batch sync endpoint (Cloud Function that accepts a progress blob). |
| **Owner**       | UD-1 |
| **Status**      | Open |

---

### R-06: CPI too high — can't acquire users profitably

| Field       | Value |
| ----------- | ----- |
| **Category**    | Commercial |
| **Description** | Cost per install on Meta exceeds $4.00 with broad targeting in soft launch markets. The game concept or creative assets don't resonate with the target audience. Scaling UA becomes unviable. |
| **Likelihood**  | 3 |
| **Impact**      | 5 |
| **Risk Score**   | 15 |
| **Mitigation**  | Prepare 3 distinct ad creative variants before soft launch: (A) gameplay footage focused on satisfying cascades, (B) "oddly satisfying" brew completion loop, (C) fail-state with "Can YOU solve this?" hook. Test each with $150 budget over 3 days. If all three exceed $4.00 CPI, the concept likely has a discoverability problem — evaluate repositioning (different visual style, different hook) or accept that the game is niche. CPI > $4.00 is a kill criterion if combined with another kill metric (see milestone-gates.md). |
| **Owner**       | GP |
| **Status**      | Open |

---

### R-07: LTV too low to sustain — poor monetization fit

| Field       | Value |
| ----------- | ----- |
| **Category**    | Commercial |
| **Description** | Players enjoy the game but don't spend money or watch ads. D7 ARPU < $0.05. Projected LTV < CPI, making the game unprofitable even with good retention. |
| **Likelihood**  | 3 |
| **Impact**      | 5 |
| **Risk Score**   | 15 |
| **Mitigation**  | Design natural monetization friction points: (1) fail recovery ad at exact moment of frustration, (2) streak protection ad leverages loss aversion, (3) daily brew 2× reward is a clear value proposition. Test IAP pricing pre-launch with team members to verify purchase flow is frictionless. During soft launch, track ad engagement rate per placement (target: ≥ 30% opt-in for rewarded ads on fail). If ARPU is below target, A/B test via Remote Config: increase interstitial frequency (every 2 levels vs. every 3), adjust Gem bundle pricing, add a starter pack ($2.99 one-time offer with 5 of each booster + 200 gems). |
| **Owner**       | GP |
| **Status**      | Open |

---

### R-08: Retention below threshold — players don't return

| Field       | Value |
| ----------- | ----- |
| **Category**    | Commercial |
| **Description** | D1 retention falls between 32–40% (below target but above kill threshold). Game has engagement issues: daily brew isn't compelling enough, win streaks don't motivate return, no external triggers (notifications) driving re-engagement. |
| **Likelihood**  | 3 |
| **Impact**      | 4 |
| **Risk Score**   | 12 |
| **Mitigation**  | Ensure daily brew reminder notification fires at user's preferred time (default 7 PM local). Implement streak-at-risk notification ("Your 7-day streak is at risk! Play today to keep it."). Track D1 return reasons in analytics (which screen do returning users land on first — level select, daily brew, workshop?). If D1 is 35–40%, run A/B test: (A) add a "welcome back" bonus (50 Essence for returning), (B) surface the daily brew more prominently on app open. If D1 < 35%, the core loop or content progression likely needs fundamental work. |
| **Owner**       | GP |
| **Status**      | Open |

---

### R-09: Key person dependency — single points of failure

| Field       | Value |
| ----------- | ----- |
| **Category**    | Team |
| **Description** | With a team of 3–8, losing any one person (illness, departure, availability change) can stall an entire workstream. Highest risk: UD-1 (owns core mechanic + Firebase) and ART (sole artist). |
| **Likelihood**  | 3 |
| **Impact**      | 4 |
| **Risk Score**   | 12 |
| **Mitigation**  | Ensure UD-2 has read-access and basic familiarity with all code UD-1 writes — schedule a 1-hour code walkthrough every 2 weeks. UD-1 and UD-2 pair-program on the core fusion system during Week 1–2 so both understand it. Document all Firebase configuration in a runbook (project ID, Firestore schema, Remote Config keys, analytics event catalog). For ART: front-load all critical art deliverables (tokens, orbs, vials) into Weeks 3–5 so that Weeks 7–10 are polish, not blocking. Identify one freelance artist who can step in with 1-week notice if ART is unavailable. |
| **Owner**       | GP |
| **Status**      | Open |

---

### R-10: Scope creep — features expand beyond MVP

| Field       | Value |
| ----------- | ----- |
| **Category**    | Team |
| **Description** | Team members add features not in the MVP spec: additional workshop rooms, social features, achievement system, more boosters, animated cutscenes, etc. Each feature seems small but collectively delays the build by 2–4 weeks. |
| **Likelihood**  | 4 |
| **Impact**      | 4 |
| **Risk Score**   | 16 |
| **Mitigation**  | Maintain a strict "MVP backlog" document. Any feature not in the build plan goes into a "Post-MVP" list — no exceptions, even if it takes "only 2 hours." GP reviews all work-in-progress at Friday demos; if anything isn't on the sprint plan, it gets reverted or shelved. Enforce the rule: "If it's not in this week's deliverables table, it doesn't get built this week." Give the team a constructive outlet — maintain a "Cool Ideas" channel where anyone can propose features for the Post-MVP list. Review the list as a team at Week 10 retro. |
| **Owner**       | GP |
| **Status**      | Open |

---

### R-11: Level design bottleneck — can't produce 40 quality levels in time

| Field       | Value |
| ----------- | ----- |
| **Category**    | Content |
| **Description** | GP is the sole level designer. Designing, playtesting, and tuning 40 levels while also managing production, writing GDD, and handling economy tuning exceeds available bandwidth. Levels are rushed, poorly tuned, or incomplete by Week 10. |
| **Likelihood**  | 4 |
| **Impact**      | 3 |
| **Risk Score**   | 12 |
| **Mitigation**  | Stagger level production across the build: 10 levels in Weeks 3–4, 15 levels in Weeks 5–6, 10 levels in Weeks 7–8, 5 levels in Week 9. This distributes load and allows earlier levels to inform later design. Build a level editor tool in Unity (Week 2 stretch goal for UD-2): visual grid editor, drag-drop ingredient placement, playtest button. If GP falls behind, UD-2 can author levels using the tool with GP reviewing. Pre-define level templates (board size × ingredient count × recipe complexity) so level creation follows a formula, not freeform design. |
| **Owner**       | GP |
| **Status**      | Open |

---

### R-12: Art production delays — final art not ready for integration

| Field       | Value |
| ----------- | ----- |
| **Category**    | Content |
| **Description** | ART falls behind on deliverables. Token art, orb tiers, workshop scene, or app store assets arrive late, forcing developers to work with placeholders longer than planned or delaying milestone gates. |
| **Likelihood**  | 3 |
| **Impact**      | 3 |
| **Risk Score**   | 9 |
| **Mitigation**  | Prioritize art by integration dependency. Art delivery order: (1) ingredient tokens (Week 3 — blocks core loop visuals), (2) orb tiers (Week 4 — blocks feel pass), (3) vial UI (Week 4), (4) workshop scene (Week 7 — can use placeholder longer since it's meta), (5) app store assets (Week 10). Developers build with placeholder art that matches final dimensions and aspect ratios so swap-in is seamless. If ART falls 1+ week behind, GP contracts a freelance artist for app store assets (lowest creative judgment required) to free ART for in-game work. |
| **Owner**       | ART |
| **Status**      | Open |

---

### R-13: Competitor launches similar game during development

| Field       | Value |
| ----------- | ----- |
| **Category**    | Market |
| **Description** | A well-funded studio releases a potion-brewing puzzle game before Brew's soft launch, capturing mindshare and making Brew feel derivative. UA costs increase as the market becomes more competitive. |
| **Likelihood**  | 2 |
| **Impact**      | 4 |
| **Risk Score**   | 8 |
| **Mitigation**  | Monitor App Store "New Games" and top puzzle game charts weekly (GP, 15 min/week). If a competitor appears: (1) download and play it — identify what they do differently and what Brew does better, (2) lean into Brew's differentiators in UA creative and store listing, (3) do not panic-pivot — a validated market is better than no market. If the competitor's game is nearly identical to Brew, accelerate the soft launch by 1 week to establish presence. Long-term differentiation comes from the workshop/collection meta — ensure this is polished. |
| **Owner**       | GP |
| **Status**      | Open |

---

### R-14: Platform policy changes affect monetization

| Field       | Value |
| ----------- | ----- |
| **Category**    | Market |
| **Description** | Apple or Google changes ad SDK requirements, IAP policies, or privacy rules (e.g., new ATT restrictions, changes to rewarded ad formats). This could break monetization features or require rework. |
| **Likelihood**  | 2 |
| **Impact**      | 3 |
| **Risk Score**   | 6 |
| **Mitigation**  | Use a mediation SDK (AdMob Mediation or Unity LevelPlay) rather than a single ad network — insulates against any one network's policy changes. Subscribe to Apple Developer News and Google Play Developer Newsletter. Check for policy updates at each milestone gate (biweekly). Build monetization with a clean abstraction layer (`IAdProvider`, `IIAPProvider` interfaces) so swapping ad/IAP providers requires only implementation changes, not architectural changes. |
| **Owner**       | UD-1 |
| **Status**      | Open |

---

### R-15: IAP receipt validation vulnerabilities in production

| Field       | Value |
| ----------- | ----- |
| **Category**    | Technical |
| **Description** | MVP uses Unity IAP's client-side receipt validation for speed. Sophisticated users could exploit this with receipt spoofing or modified APKs to gain unlimited Gems without paying. |
| **Likelihood**  | 2 |
| **Impact**      | 3 |
| **Risk Score**   | 6 |
| **Mitigation**  | Accept client-side validation risk for soft launch (low user volume, limited financial exposure). Implement server-side receipt validation via Firebase Cloud Function as a post-soft-launch priority (target: within 2 weeks of scaling UA). Track IAP metrics: if Gem balance distribution shows outliers (players with >10,000 Gems and $0 spend), investigate. Use Remote Config to feature-flag the validation mode (client vs. server) so the switch is seamless. |
| **Owner**       | UD-1 |
| **Status**      | Open |

---

### R-16: Ad SDK initialization failures on fragmented Android devices

| Field       | Value |
| ----------- | ----- |
| **Category**    | Technical |
| **Description** | Ad SDKs (AdMob, Unity Ads) fail to initialize on certain Android devices due to Google Play Services version issues, missing WebView, or OEM modifications. Affected users see no ads, breaking rewarded ad flows (fail recovery, streak protection). |
| **Likelihood**  | 3 |
| **Impact**      | 2 |
| **Risk Score**   | 6 |
| **Mitigation**  | Implement `AdManager` with initialization state tracking: `NotInitialized`, `Initializing`, `Ready`, `Failed`. If state is `Failed`, hide all ad-related UI buttons (don't show "Watch ad" if no ad is available). Log `ad_init_failed` event to analytics with device model and Android version. Provide fallback for fail recovery: if ad unavailable, offer Gem-based recovery (5 Gems for +5 moves). Test ad initialization on 5+ Android devices including at least one low-end device without Google Play Services. |
| **Owner**       | UD-2 |
| **Status**      | Open |

---

### R-17: Cloud save conflicts cause progress loss

| Field       | Value |
| ----------- | ----- |
| **Category**    | Technical |
| **Description** | Player plays on two devices without syncing, or plays offline and then reconnects. Conflicting save states result in progress loss (levels reverted, currency lost). Player trust is damaged. |
| **Likelihood**  | 3 |
| **Impact**      | 4 |
| **Risk Score**   | 12 |
| **Mitigation**  | Implement conflict resolution rules: (1) for level progress, take the maximum (if Device A has level 15 unlocked and Device B has level 12, keep 15), (2) for currency, server value wins (server is authoritative for Essence and Gems), (3) for daily brew, take the latest timestamp. On conflict detection, show the player a brief toast: "Progress synced from another device." Write automated tests for conflict scenarios: play offline, go online, verify merge. Add a `last_sync_timestamp` field to the save and log `save_conflict_resolved` events to analytics. |
| **Owner**       | UD-1 |
| **Status**      | Open |

---

### R-18: Tutorial fails to teach — players skip or don't understand

| Field       | Value |
| ----------- | ----- |
| **Category**    | Design |
| **Description** | Tutorial is either too long (players skip it), too short (players don't learn the brew mechanic), or poorly paced (players learn to tap but never understand chains or recipes). Tutorial completion drops below 85%. |
| **Likelihood**  | 3 |
| **Impact**      | 4 |
| **Risk Score**   | 12 |
| **Mitigation**  | Design tutorial with "show, don't tell" principle: each tutorial level forces exactly one action (highlight the cluster they must tap, grey out everything else). Keep text to ≤ 15 words per instruction overlay. Add skip button but track skip rate in analytics — if >20% skip, tutorial is too long. Gate 3 external playtest is the validation point: 4/5 must complete without help. If tutorial fails validation, simplify: cut tutorial level 4 (booster tutorial) and teach boosters through a tooltip on first natural encounter instead. Budget 1 day of rework after playtest feedback. |
| **Owner**       | GP |
| **Status**      | Open |

---

## Risk Matrix Summary

```
Impact →       1         2         3         4         5
Likelihood ↓
    5                                                    
    4                            R-04     R-10     
                                 R-02                
                                 R-11                
    3                   R-16     R-05     R-08     R-01
                                 R-12     R-17     R-06
                                          R-18     R-07
    2                            R-14     R-13          
                                 R-15                    
    1                                                    
```

### Priority Actions (Score ≥ 12)

| Risk | Score | Required Action                                          | Deadline   |
| ---- | ----- | -------------------------------------------------------- | ---------- |
| R-10 | 16    | Publish MVP backlog. Enforce weekly scope review.        | Week 1     |
| R-01 | 15    | Design playtest at Gate 3. Prepare pivot criteria.       | Week 6     |
| R-06 | 15    | Prepare 3 ad creative variants. Set CPI kill threshold.  | Week 9     |
| R-07 | 15    | Design monetization friction points. Plan A/B tests.     | Week 8     |
| R-02 | 12    | Define per-band completion rate targets. Set up tracking. | Week 4     |
| R-04 | 12    | Implement quality tiers. Profile on low-end weekly.      | Week 5     |
| R-08 | 12    | Build notification system. Plan retention A/B tests.     | Week 9     |
| R-09 | 12    | Schedule code walkthroughs. Identify backup freelancers. | Week 2     |
| R-11 | 12    | Build level editor. Stagger level production schedule.   | Week 3     |
| R-17 | 12    | Define conflict resolution rules. Write merge tests.     | Week 8     |
| R-18 | 12    | Design tutorial with skip tracking. Validate at Gate 3.  | Week 6     |

---

## Review Cadence

- **Weekly:** GP scans register at Friday retro. Update status of any risk that changed.
- **At each milestone gate:** Full team reviews register. Re-score any risk where likelihood or impact changed. Add new risks identified during the sprint.
- **Post-soft-launch:** Review weekly for first 4 weeks. Add new risks discovered from live data (e.g., specific device crashes, unexpected player behavior).
