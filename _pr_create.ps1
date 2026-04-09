$prBody = @"
## Summary

- **Event system wiring**: LevelLoader.LoadEventLevel() with theme overlay, WeeklyEventManager wired in GameFlowController, RemoteConfigDefaults synced to all 68 JSON keys
- **Notification integration**: UnityNotificationScheduler (iOS/Android) and NullNotificationScheduler (Editor), gated behind SettingsManager.NotificationsEnabled
- **Config SO integration**: GameFlowController reads from PotionShelfConfigSO, WorkshopConfigSO, DailyBrewConfigSO, WinStreakConfigSO, EconomyConfigSO with fallback defaults
- **Test coverage**: 32+ new tests across AdManager, NotificationManager, CurrencyFormatter, EventLevelLoader
- **Level tuning**: Levels 36-40 adjusted
- **Soft-launch prep**: soft-launch plan, store listing, .firebaserc, AudioConfig

## Review fixes applied

- Added missing using System (compilation fix)
- Fixed iOS notification timezone bug
- Fixed Android notification ID overflow
- Fixed event level Resources.Load path
- Separated event lose flow from campaign streak protection
- Fixed WeeklyEventView.RefreshState panel leakage
- Complete OnDestroy cleanup

## Test plan

- [ ] Verify compilation in Unity Editor
- [ ] Run all EditMode tests
- [ ] Create and assign config SO assets
- [ ] Test event level flow end-to-end
- [ ] Verify notification scheduling
- [ ] Play levels 36-40 and validate difficulty
"@

$jsonBody = @{
    title = "Gate 5 integration: event system, notifications, config SOs, level tuning"
    body = $prBody
    head = "cursor/add-changelog-adr-session-handoff"
    base = "master"
    draft = $true
} | ConvertTo-Json

$credInput = "protocol=https`nhost=github.com"
$credOutput = $credInput | git credential fill 2>$null
$token = ($credOutput | Select-String "password=(.+)").Matches[0].Groups[1].Value

if (-not $token) {
    Write-Error "No GitHub token found"
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Accept" = "application/vnd.github+json"
}

try {
    $result = Invoke-RestMethod -Uri "https://api.github.com/repos/RipKDR/Brew/pulls" -Method Post -Headers $headers -Body $jsonBody -ContentType "application/json"
    Write-Output "PR created: $($result.html_url)"
} catch {
    Write-Output "API Error: $($_.Exception.Message)"
    Write-Output $_.ErrorDetails.Message
}
