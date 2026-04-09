# Firebase Configuration — Brew

This directory contains all Firebase project configuration files for the Brew mobile puzzle game.

## Files


| File                          | Purpose                                                                                                                                      |
| ----------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| `firebase.json`               | Root Firebase CLI config — maps rules, indexes, functions, hosting, and remote config to their file paths                                    |
| `firestore.rules`             | Firestore security rules — players own their data, levels/events are read-only, no public access                                             |
| `firestore.indexes.json`      | Composite index definitions — empty for MVP, add as query patterns emerge                                                                    |
| `remote-config-defaults.json` | Remote Config template with all tunable parameters and their default values (economy, gameplay, ads, streaks, feature flags, events, tuning) |


## Environment Setup

### First-time setup

```bash
npm install -g firebase-tools
firebase login
```

### Add project aliases

```bash
firebase use --add    # select dev project   → alias "dev"
firebase use --add    # select staging project → alias "staging"
firebase use --add    # select prod project   → alias "prod"
```

### Switch environments

```bash
firebase use dev
firebase use staging
firebase use prod
```

## Deployment Commands

### Firestore rules only

```bash
firebase deploy --only firestore:rules
```

### Firestore indexes only

```bash
firebase deploy --only firestore:indexes
```

### Remote Config only

```bash
firebase deploy --only remoteconfig
```

### Cloud Functions only

```bash
firebase deploy --only functions
```

### Everything

```bash
firebase deploy
```

## Notes

- `firebase.json` does not support comments. Refer to this README for context on each section.
- Firestore indexes are intentionally empty at MVP. As the app ships and query patterns stabilize, add composite indexes here rather than relying on auto-generated indexes from the Firebase Console.
- Remote Config values are **all strings** in the JSON template (Firebase convention). The client and Cloud Functions parse them to the appropriate types (int, float, bool, JSON array/object) at runtime.
- Always deploy rules to **dev** first, verify with the Firestore emulator or test suite, then promote to staging and prod.

