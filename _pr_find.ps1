$credInput = "protocol=https`nhost=github.com"
$credOutput = $credInput | git credential fill 2>$null
$token = ($credOutput | Select-String "password=(.+)").Matches[0].Groups[1].Value
$headers = @{
    "Authorization" = "Bearer $token"
    "Accept" = "application/vnd.github+json"
}
$result = Invoke-RestMethod -Uri "https://api.github.com/repos/RipKDR/Brew/pulls?head=RipKDR:cursor/add-changelog-adr-session-handoff&state=open" -Headers $headers
if ($result.Count -gt 0) {
    Write-Output "PR #$($result[0].number): $($result[0].html_url)"
    Write-Output "Draft: $($result[0].draft)"
} else {
    Write-Output "No open PR found"
}
