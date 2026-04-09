# Brew — Glossary

> Canonical definitions for every term used across Brew documentation.
> Alphabetized. When in doubt about terminology, this document is the source of truth.
> Last updated: 2026-04-09

---

| Term | Definition |
|------|-----------|
| **Adjacency** | Two cells sharing an edge (up, down, left, right). Diagonals are not adjacent. Adjacency does not wrap around grid edges. |
| **ARPDAU** | Average Revenue Per Daily Active User. Primary revenue health metric. Target: >= $0.08. |
| **Bloom** | Visual effect triggered when a Brew Orb reaches the brew threshold — a burst of light and particles radiating from the orb as it transforms into a completed potion. |
| **Blocker** | An obstacle occupying a grid cell that restricts normal token behavior. MVP types: Stone (inert, cleared by adjacent brew) and Ice (locks a token in place for one turn after adjacent fusion). |
| **Board** | The NxM grid of cells where gameplay occurs. Default 7×9. Configurable per level. |
| **BoardConfig** | ScriptableObject defining a level's grid dimensions, cell size, and spacing. |
| **Booster** | A consumable power-up the player activates before or during a level. Three MVP types: Shake, Catalyst, and Extra Moves. Purchased with Essence or Gems. |
| **Brew / Brewing** | The moment a Brew Orb's accumulated token_count reaches the configured threshold and transforms into a completed potion. Triggers a celebration animation and fills the corresponding Potion Vial. |
| **Brew Orb** | The glowing merged token created by fusing a cluster. Has a `color` and a `token_count` representing the number of tokens absorbed. Grows visually as token_count increases. Can chain-fuse with adjacent same-colored tokens or orbs. |
| **Cascade** | The automatic gravity-and-refill sequence that follows a fusion or brew. Tokens above gaps fall downward, new tokens spawn at the top, and the system checks for chain-fusion opportunities. Cascades are free — they do not cost moves. Capped at 20 iterations. |
| **Catalyst** | Booster that auto-fuses one player-selected cluster without costing a move. Essence price: 100. Gem price: 10. |
| **Cell** | A single position on the grid at coordinates (col, row). Contains exactly one of: Token, Brew Orb, or Empty. |
| **Chain Fusion** | Automatic fusion that triggers when a Brew Orb ends up adjacent to same-colored tokens or other orbs after a cascade or gravity settle. Chains are resolved recursively until no more valid fusions exist. Free — does not cost a move. |
| **Cluster** | A connected group of 3 or more same-colored adjacent tokens. The minimum tappable unit. Found via flood-fill from any token in the group. |
| **CPI** | Cost Per Install. The acquisition cost of one new player from a paid UA channel. Target: iOS < $2.50, Android < $1.50. Kill threshold: > $4.00. |
| **D1 / D7 / D30 Retention** | Percentage of new installs who return on calendar day 1, 7, or 30 after install. Primary health metrics. D1 target: >= 40%. D7 target: >= 15%. D30 target: >= 7%. |
| **Daily Brew** | A daily challenge level available every 24 hours (resets at 00:00 UTC). Offers bonus Essence and Gem rewards. Consecutive daily completions build a Daily Brew Streak with escalating bonuses. |
| **DAU** | Daily Active Users. Unique devices with at least one session in a calendar day. |
| **Difficulty Lever** | A tunable parameter that affects level difficulty. Introduced in strict sequence: grid size, ingredient colors, move budget, recipe complexity, board shape, stone blockers, ice blockers. |
| **Ember** | Red ingredient type. One of five token colors. Hex: defined in art-direction.md. |
| **Essence** | Soft currency earned through gameplay (level completion, daily brew, events, shelf milestones). Primary sinks: Workshop upgrades and booster purchases. Cannot be purchased directly with real money. |
| **Extra Moves** | Booster that adds +5 moves to the current level's move counter. Also offered as a paid retry option when a player fails a level. Gem price: 10. |
| **Feature Flag** | A boolean or enum value in Firebase Remote Config that enables or disables a feature server-side without requiring an app update. |
| **Flood Fill** | The algorithm used to discover clusters. Starting from a tapped token, recursively visits all orthogonally adjacent same-colored tokens to identify the full connected group. |
| **Frost** | Blue ingredient type. One of five token colors. |
| **Funnel** | The sequence of player progression milestones from install through end-game, measured as cumulative conversion rates. Key steps: Install → Tutorial Start → Tutorial Complete → Level 5 → Level 10 → Level 20 → Level 30 → Level 40. |
| **Fusion** | The core player action: tapping a valid cluster (>= 3 same-colored adjacent tokens) to merge them into a Brew Orb at the tap position. Costs one move. |
| **Gate** | A milestone checkpoint with binary pass/fail criteria. Five gates across the 10-week MVP. Failure triggers rework or project reassessment. See milestone-gates.md. |
| **Gems** | Premium (scarce) currency. Primary source: in-app purchases. Secondary sources: Daily Brew, shelf milestones, weekly event completion. Used for boosters at premium prices, extra moves on fail, streak protection, and future cosmetics. |
| **Gravity** | The system that moves tokens downward to fill empty cells after fusions, brews, or blocker clears. Tokens above gaps fall toward higher row indices. New tokens spawn off-screen at the top. |
| **Grid Mask** | A boolean 2D array defining which cells in the NxM grid are active. Enables irregular board shapes (L-shapes, plus-shapes, etc.). Introduced at Level 11. |
| **Haptic** | Device vibration feedback mapped to game events (fusion, chain, brew, level win). Defined in sound-design.md. Intensity scales with event significance. |
| **Ice Blocker** | A blocker type that locks a token in place, preventing it from participating in clusters or falling via gravity. Cleared after one adjacent fusion. Introduced at Level 22. |
| **Ingredient** | Generic term for any of the five token types: Ember (red), Frost (blue), Vine (green), Sun (yellow), Shadow (purple). |
| **Ingredient Pool** | The subset of all five ingredient types available to spawn on a given level. Tutorial levels use 2–3; later levels use all 5. |
| **Level** | A single puzzle defined by grid dimensions, ingredient pool, recipe targets, move limit, blocker placements, and star thresholds. Won by fulfilling all recipe targets. Lost when moves reach zero. |
| **Level Config** | A JSON (or ScriptableObject) file specifying all parameters for a single level. Loaded by the LevelManager at level start. |
| **Mediation** | The ad-serving strategy that waterfalls or bid-auctions across multiple ad networks (Meta Audience Network, Unity Ads, Vungle, Pangle) to maximize eCPM. Managed via AppLovin MAX. |
| **Meta Layer** | Game systems outside the core puzzle loop: Potion Shelf, Workshop, Streaks, Daily Brew, Weekly Events, Booster Inventory, Economy. Drives long-term retention. |
| **Milestone** | A project checkpoint at the end of Weeks 2, 4, 6, 8, and 10. Each has specific pass criteria defined in milestone-gates.md. |
| **Move** | One player-initiated fusion. Tapping a valid cluster costs exactly one move. Auto-chains, cascades, and gravity are free. The move counter is the level's primary constraint. |
| **Near-Miss** | A designed failure state where the player can see they were close to completing the recipe (e.g., one more brew away). Triggers retry motivation and optional extra-moves purchase. |
| **No-Ads Pass** | A one-time IAP ($4.99) that permanently removes interstitial ads. Rewarded ads remain available as opt-in. |
| **Potion** | The crafted result of a successful brew. Each level's recipe produces specific potions. Collected potions appear on the Potion Shelf. |
| **Potion Shelf** | A scrollable collection screen showing every unique potion the player has brewed. Organized into 5 shelves (one per world, 20 potions each). Doubles as progress map and milestone reward track. |
| **Potion Vial** | UI element displayed alongside the game board that fills progressively as the player brews potions matching the level's recipe targets. One vial per recipe target color. |
| **Recipe** | A level's win condition: the specific set of potions the player must brew. Expressed as color + count pairs (e.g., "2 Red, 1 Blue"). Complexity ranges from single-color single-count to multi-color multi-count. |
| **Remote Config** | Firebase Remote Config. A server-side key-value store for tuning game values (economy rates, difficulty parameters, feature flags, ad frequency caps) without requiring an app update. |
| **Rewarded Ad** | An opt-in video ad the player can choose to watch in exchange for a gameplay benefit (e.g., +3 moves, 2× end-of-level rewards). Never forced. |
| **ScriptableObject** | A Unity data container used for configuration that designers can edit without touching code. Used for BoardConfig, LevelConfig, IngredientDefinition, economy values, and more. |
| **Session** | A continuous period of app usage. Starts on foreground entry (30-second debounce), ends on background or termination. Target: 5–8 minutes median. |
| **Settle** | The pause after a cascade resolves where the board stabilizes and the system checks for new chain-fusion opportunities. If no chains are found, input returns to the player. |
| **Shadow** | Purple ingredient type. One of five token colors. The fifth color introduced (Level 13). |
| **Shake** | Booster that randomizes the positions of all tokens on the board without adding or removing any. Useful when no good clusters are available. Essence price: 50. Gem price: 5. |
| **Soft Launch** | A limited-geography release (typically 2–3 countries) used to validate retention, monetization, and CPI metrics before global rollout. |
| **Star Rating** | 1–3 stars awarded at level completion based on moves remaining. Thresholds defined per level in the level config. Affects Essence payout. |
| **Starter Bundle** | A one-time IAP ($1.99) containing 50 Gems + 500 Essence + 3 Catalysts. Offered after completing Level 5. |
| **State Machine** | The board's flow control system. States include: Initializing, WaitingForInput, Fusing, Cascading, Settling, Brewing, CheckingWin, LevelComplete, LevelFailed. Transitions are deterministic and event-driven. |
| **Stone Blocker** | A blocker type that occupies a cell, preventing tokens from occupying it. Cleared when the player brews a potion in an adjacent cell. Introduced at Level 16. |
| **Streak** | Win Streak: consecutive level completions without a loss. Multiplies Essence rewards (1.5× at 3-win, 2.0× at 5-win, 2.5× at 10-win). Resets on a loss unless the player uses Streak Protection (5 Gems). |
| **Sun** | Yellow ingredient type. One of five token colors. The fourth color introduced (Level 8). |
| **Tier** | The size level of a Brew Orb, determined by its token_count. Higher-tier orbs are visually larger and closer to the brew threshold. |
| **Token** | A single ingredient piece occupying one cell on the board. The basic unit of gameplay. Has a `color` property matching one of the five ingredient types. |
| **token_count** | Integer property of a Brew Orb representing how many individual tokens have been absorbed into it through fusion. When token_count reaches the brew threshold, the orb brews into a potion. |
| **Tutorial** | The first 5 levels of the game, each teaching exactly one concept through guided, fail-safe play. Progressive disclosure; one new mechanic per level. See tutorial-flow.md. |
| **UA (User Acquisition)** | Paid marketing efforts to acquire new players. Primary channels: Meta (Facebook/Instagram), Google Ads, TikTok, Unity Ads. See ua-creative-strategy.md. |
| **Vine** | Green ingredient type. One of five token colors. The third color introduced (Level 3). |
| **Weekly Deal** | A recurring IAP ($2.99) with rotating contents on a 4-week cycle. Resets Monday 00:00 UTC. |
| **Weekly Event** | A themed 7-level challenge running Monday–Sunday with unique recipes and exclusive rewards (unique potion, Essence, Gems). Participation tracked as an engagement KPI. |
| **World** | A thematic grouping of 20 levels on the Potion Shelf. Five MVP worlds: Ember Glen, Frost Hollow, Vine Thicket, Sun Ridge, Shadow Deep. |
| **Workshop** | The player's home screen — a single-room alchemist's lab restored through Essence-funded upgrades across 12 tiers. Primary Essence sink. Each upgrade reveals new decorations and visual features. |
