# Builds the offline Windows release: dist\MyFathersWork-Windows-<version>.zip
# (MyFathersWork.exe + wwwroot). The target PC needs no .NET; it serves the app on http://localhost:51660.
#
#   pwsh tools/build-windows.ps1 [-Version 0.66.6.123]   (default: computed from git like the Pages deploy)
#
# Needs the Git LFS files (git lfs pull --include "MyFathersWorkApp/**"), otherwise images and audio are pointers.
param(
    [string]$Version = ""
)
$ErrorActionPreference = "Stop"

$repo = Split-Path -Parent $PSScriptRoot
$app = Join-Path $repo "MyFathersWorkApp\MyFathersWorkApp\MyFathersWorkApp.csproj"
$launcher = Join-Path $repo "MyFathersWorkApp\WindowsLauncher\WindowsLauncher.csproj"
$dist = Join-Path $repo "dist"
$work = Join-Path $dist "work"
$out = Join-Path $dist "MyFathersWork-Windows"

if (-not $Version) {
    # Same as deploy-pages.yml: 0.66.6.<commits (first parent) since 3d78856, minus 1>; the csproj only holds .0
    $count = git -C $repo rev-list --count --first-parent 3d78856..HEAD
    if ($LASTEXITCODE) { throw "could not count commits for the version (shallow clone?); pass -Version" }
    $Version = "0.66.6.$([math]::Max(0, [int]$count - 1))"
}
$versionArg = "-p:Version=$Version"

# An LFS pointer instead of an image means the release would show broken icons
$logo = Join-Path $repo "MyFathersWorkApp\MyFathersWorkApp\wwwroot\favicon.ico"
if ((Get-Item $logo).Length -lt 1024 -and (Get-Content $logo -TotalCount 1) -like "version https://git-lfs*") {
    throw "Git LFS files are missing. Run: git lfs pull --include `"MyFathersWorkApp/**`""
}

foreach ($dir in @($work, $out)) {
    if ((Test-Path -LiteralPath $dir) -and $dir.StartsWith($dist)) { Remove-Item -LiteralPath $dir -Recurse -Force }
}

Write-Host "Publishing the web app ($Version)..."
dotnet publish $app -c Release $versionArg -o (Join-Path $work "app")
if ($LASTEXITCODE) { throw "web app publish failed" }

Write-Host "Publishing the launcher..."
dotnet publish $launcher -c Release -r win-x64 $versionArg -o (Join-Path $work "launcher")
if ($LASTEXITCODE) { throw "launcher publish failed" }

New-Item -ItemType Directory -Force $out | Out-Null
Copy-Item (Join-Path $work "launcher\MyFathersWork.exe") $out
# The launcher serves plain files; the precompressed .br/.gz copies are not needed
Copy-Item (Join-Path $work "app\wwwroot") $out -Recurse
Get-ChildItem (Join-Path $out "wwwroot") -Recurse -Include *.br, *.gz | Remove-Item -Force

@"
My Father's Work - Fan Made Companion App ($Version), offline Windows version

1. Unzip this whole folder anywhere (keep MyFathersWork.exe next to the wwwroot folder).
2. Double-click MyFathersWork.exe. The app opens in its own window (Microsoft Edge in app mode).
   Closing the window stops the app. Double-clicking again while it runs opens another window.

No internet connection and no installation are needed. Saved games are kept on this PC
(%LOCALAPPDATA%\MyFathersWork) and stay when you unzip a newer version over this one.
Without Microsoft Edge the app opens in your default browser, with a console window you close to stop it.
Windows SmartScreen may warn about an unknown publisher the first time: More info > Run anyway.
"@ | Set-Content (Join-Path $out "README.txt") -Encoding utf8

$zip = Join-Path $dist "MyFathersWork-Windows-$Version.zip"
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
Compress-Archive -Path $out -DestinationPath $zip
Remove-Item -LiteralPath $work -Recurse -Force

Write-Host "Done: $zip ($([math]::Round((Get-Item $zip).Length / 1MB)) MB)"
