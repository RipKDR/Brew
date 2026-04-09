# Localization Plan

## Overview

Brew is a mobile puzzle game targeting a global mass-market audience. This document defines the localization strategy, implementation details, workflow, and budget for bringing Brew to international players.

---

## Launch Languages

### MVP Soft Launch

| Language | Locale | Notes |
|----------|--------|-------|
| English | en | Primary development language |

### Global Launch (priority order by puzzle-game revenue potential)

| Priority | Language | Locale | Target Markets |
|----------|----------|--------|----------------|
| 1 | English | en | US, UK, AU, CA |
| 2 | Japanese | ja | #2 mobile game market globally |
| 3 | Korean | ko | High ARPU puzzle market |
| 4 | German | de | Largest EU market |
| 5 | French | fr | France + francophone Africa/Canada |
| 6 | Spanish | es | Spain + LATAM |
| 7 | Portuguese | pt-BR | Brazil, large mobile market |
| 8 | Simplified Chinese | zh-Hans | Mainland China (requires separate regulatory considerations) |
| 9 | Traditional Chinese | zh-Hant | Taiwan, Hong Kong |
| 10 | Thai | th | Growing SEA market |

### China-Specific Considerations (zh-Hans)

- Requires ISBN approval from NPPA for monetized games
- Must partner with a local publisher
- Separate app store distribution (no Google Play; use TapTap, Huawei AppGallery, etc.)
- May require content review and gameplay modifications
- Consider deferring until post-global-launch traction is established

---

## What Needs Localization

### Localized Content

| Category | Estimated String Count | Notes |
|----------|----------------------|-------|
| UI text (buttons, labels, menus, popups) | ~200 | All interactive elements |
| Tutorial text (5 tutorial levels) | ~15 | Step-by-step prompts |
| Potion names | ~30 | Themed fantasy names |
| Workshop upgrade names | ~15 | Upgrade labels and descriptions |
| Event names and descriptions | ~10 per event template | Rotating seasonal/weekly events |
| App store metadata | ~5 per platform | Title, subtitle, description, keywords, what's new |
| Legal | 2 per region | Privacy policy and terms of service URLs |

### Not Localized

| Category | Reason |
|----------|--------|
| Ingredient names | Visual icons used instead of text labels |
| Sound effects | Language-neutral audio |
| Music | Instrumental, no vocals |
| Ingredient sprites/art | Universal visual language |

---

## Implementation Approach

### Technology Stack

- **Package:** Unity Localization (`com.unity.localization`)
- **String storage:** String Tables exported as CSV, managed in Google Sheets
- **Runtime:** `LocalizedString` references bound to UI components via `LocalizeStringEvent`
- **Locale detection:** Device locale on first launch, user-overridable in settings

### Localization Key Convention

Keys follow the pattern `{domain}.{screen}.{element}`:

```
ui.hud.moves_remaining
ui.popup.level_complete.title
game.potion.ember_elixir
ui.shop.gems_pack_small
```

- `ui.*` — user interface strings
- `game.*` — gameplay content (potion names, booster names)
- `meta.*` — app store metadata, legal

### Text Rendering

| Consideration | Approach |
|---------------|----------|
| RTL support | Not required for launch languages. Arabic/Hebrew deferred to post-launch. |
| CJK fonts | Separate font assets for ja, ko, zh-Hans, zh-Hant. Use Noto Sans CJK or Source Han Sans. |
| Text overflow | All UI elements must be tested with longest translation per key. German and French tend to be 30–40% longer than English. |
| Dynamic text | Use `{0}` placeholders for runtime values (e.g., `{0} moves left`). Never concatenate strings. |
| Pluralization | Use Unity Smart Strings for plural rules (English: 1/other; Japanese: no plural; etc.). |
| Date/time formatting | Use `System.Globalization.CultureInfo` from device locale. |
| Number formatting | Use device locale (1,000 vs 1.000 vs 1 000). |
| Currency display | IAP prices handled natively by Apple App Store and Google Play — no custom formatting needed. |

### Font Requirements

| Language Group | Font Asset | Estimated Size |
|----------------|-----------|----------------|
| Latin (en, de, fr, es, pt-BR) | Primary game font | ~200KB |
| Japanese (ja) | Noto Sans JP subset | ~2–4MB |
| Korean (ko) | Noto Sans KR subset | ~1–3MB |
| Simplified Chinese (zh-Hans) | Noto Sans SC subset | ~3–6MB |
| Traditional Chinese (zh-Hant) | Noto Sans TC subset | ~3–6MB |
| Thai (th) | Noto Sans Thai | ~200KB |

Use TextMesh Pro font atlases with character presets per language to minimize memory.

---

## Localization Workflow

```
Developer adds English string with localization key
        │
        ▼
Export string table to Google Sheets (automated via CI script)
        │
        ▼
Professional translator fills in target language columns
        │
        ▼
Import translated sheet back to Unity string tables
        │
        ▼
QA validates in-game per language
  • Text overflow / truncation
  • Placeholder correctness
  • Cultural appropriateness
  • Screenshot comparison vs English
        │
        ▼
Merge to main branch
```

### Roles

| Role | Responsibility |
|------|---------------|
| Developer | Add keys, write English strings, integrate localization components |
| Localization manager | Maintain Google Sheet, coordinate translators, track completion |
| Translators (per language) | Translate strings, flag ambiguous context |
| QA (per language) | In-game validation, screenshot review |

### Tooling

- **Google Sheets** — single source of truth for all string tables
- **Unity Localization Tables** — runtime string lookup
- **CI script** — `sheets-to-unity` export pipeline (CSV download → StringTable asset generation)
- **Screenshot testing** — automated screenshots per locale for visual regression

---

## Budget Estimation

### Translation Costs

| Item | Calculation | Cost |
|------|------------|------|
| In-game strings | ~300 strings × ~10 words/string × $0.10/word × 9 languages | ~$2,700 |
| App store metadata | ~$200/language × 9 languages | ~$1,800 |
| **Total translation** | | **~$4,500** |

### Ongoing Costs (Post-Launch)

| Item | Frequency | Estimated Cost |
|------|-----------|---------------|
| New event strings (~10/event) | Bi-weekly | ~$90/event × 9 languages |
| App store "What's New" updates | Per release | ~$50/update × 9 languages |
| New feature strings | Per milestone | Variable |

---

## Cultural Considerations

### Theme Safety

- Alchemy/potion brewing is a **globally neutral fantasy theme** with no religious sensitivity
- Ingredient icons are abstract (ember, frost, vine, sun, shadow) — no culturally specific imagery
- Color meanings vary by culture (red = luck in China, danger in the West), but Brew's color usage is tied to ingredient types, not symbolic meaning — **low risk**

### Naming Guidelines

- Potion names should be reviewed by native speakers for unintended meanings
- Avoid puns that only work in English — prefer evocative fantasy words
- Workshop decoration names should not reference culturally specific objects

### Event Theming

| Instead of | Use |
|------------|-----|
| Christmas Event | Winter Brew Festival |
| Halloween Event | Shadow Brew Night |
| Easter Event | Spring Bloom Brew |
| Thanksgiving Event | Harvest Brew |

Design events around seasons and fantasy themes rather than specific cultural or religious holidays.

### Regulatory

| Region | Requirement |
|--------|------------|
| EU (GDPR) | Consent dialogs, privacy policy in local language |
| China | NPPA ISBN, real-name verification for minors, playtime limits |
| South Korea | Age-rating disclosure, probability disclosure for gacha/loot |
| Japan | JARO guidelines for IAP advertising, kompu gacha rules |
| Brazil (LGPD) | Privacy policy in Portuguese, data processing disclosures |

---

## Milestones

| Milestone | Deliverable |
|-----------|------------|
| Soft launch | English only, all keys finalized |
| Localization prep | String table exported, translators engaged |
| Translation complete | All 9 languages translated, imported to Unity |
| LQA pass | All languages validated in-game, bugs filed and fixed |
| Global launch | All languages live, app store metadata localized |
