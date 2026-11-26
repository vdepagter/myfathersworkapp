# Google Play App Signing with OUR release key, so the Play version and the sideloaded APK are signed alike and update
# each other (a key that Google generates would make the Play version unable to update a sideloaded install, and
# the other way round).
#
# In Play Console: Test and release > App integrity > Play app signing > Change app signing key (before the first
# release: "Use a different key") > "Export and upload a key from Java keystore". Download pepk.jar and the encryption
# public key it offers into one folder, then run:
#
#   pwsh android-app/play-signing.ps1 -Folder <that folder>
#
# It writes <folder>\output.zip (the release key, encrypted for Google with that public key); upload it on the same
# page. The key itself never leaves this PC unencrypted. Needs Java (JDK 21, as for the Android build).
param(
    [Parameter(Mandatory)] [string]$Folder,
    [string]$Keystore = "$env:USERPROFILE\keys\myfathersworkapp-release.jks",
    [string]$Alias = ""
)
$ErrorActionPreference = "Stop"

$pepk = Join-Path $Folder "pepk.jar"
$publicKey = Get-ChildItem -LiteralPath $Folder -Filter "*.pem" | Select-Object -First 1
if (-not (Test-Path -LiteralPath $pepk)) { throw "pepk.jar not found in $Folder (download it from Play Console)" }
if (-not $publicKey) { throw "The encryption public key (.pem) from Play Console is not in $Folder" }
if (-not (Test-Path -LiteralPath $Keystore)) { throw "Release keystore not found: $Keystore" }

if (-not $Alias) {
    # The alias the Android build uses (android-app/README.md); asked when not given
    $Alias = Read-Host "Key alias in $Keystore"
}

$output = Join-Path $Folder "output.zip"
Write-Host "pepk asks for the keystore password and the key password."
java -jar $pepk --keystore="$Keystore" --alias="$Alias" --output="$output" --include-cert --rsa-aes-encryption --encryption-key-path="$($publicKey.FullName)"
if ($LASTEXITCODE) { throw "pepk failed" }
Write-Host "Done: $output - upload it in Play Console (App integrity > Play app signing)."
