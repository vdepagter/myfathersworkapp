# Stores the Android release keystore as the four repository secrets .github/workflows/android-apk.yml signs with.
# Run once on the machine that holds the keystore (gh must be logged in with access to the repository):
#     pwsh android-app/set-secrets.ps1 [-Keystore <path>] [-PasswordFile <path>]
# The password file is the one written next to the keystore ("Keystore password: ..." / "Alias: ..." lines).
# The values are piped to gh and never printed.
param(
    [string]$Keystore     = "$env:USERPROFILE\keys\myfathersworkapp-release.jks",
    [string]$PasswordFile = "$env:USERPROFILE\keys\myfathersworkapp-release-password.txt",
    [string]$Repo         = "vdepagter/myfathersworkapp"
)
$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $Keystore))     { throw "Keystore not found: $Keystore" }
if (-not (Test-Path -LiteralPath $PasswordFile)) { throw "Password file not found: $PasswordFile" }

$lines    = Get-Content -LiteralPath $PasswordFile
$password = (($lines | Where-Object { $_ -like "Keystore password: *" } | Select-Object -First 1) -replace '^Keystore password: ', '').Trim()
$alias    = (($lines | Where-Object { $_ -like "Alias: *" } | Select-Object -First 1) -replace '^Alias: ', '').Trim()
if (-not $password -or -not $alias) { throw "No 'Keystore password:' / 'Alias:' line in $PasswordFile" }

[Convert]::ToBase64String([IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $Keystore))) | gh secret set ANDROID_KEYSTORE_BASE64 --repo $Repo
$password | gh secret set ANDROID_KEYSTORE_PASSWORD --repo $Repo
$alias    | gh secret set ANDROID_KEY_ALIAS --repo $Repo
$password | gh secret set ANDROID_KEY_PASSWORD --repo $Repo
gh secret list --repo $Repo
