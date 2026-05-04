# Level Authoring Directory

This directory is an **authoring mirror** of `Assets/_Project/Resources/Levels/`.

The authoritative level files used at runtime live in:

- Campaign levels: `Assets/_Project/Resources/Levels/`
- Event levels: `Assets/_Project/Resources/EventLevels/`

After editing levels here, sync to Resources before committing:

```powershell
Copy-Item levels/*.json Assets/_Project/Resources/Levels/ -Force
Copy-Item levels/events/*.json Assets/_Project/Resources/EventLevels/ -Force
```

DO NOT delete the `levels/` directory since it may be used by Python tooling scripts.
