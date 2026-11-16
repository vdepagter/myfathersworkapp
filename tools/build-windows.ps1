# Builds the offline Windows release (MyFathersWork.exe + wwwroot). The target PC needs no .NET; it serves the app on
# http://localhost:51660. Three downloads, the same app in each:
#   dist\MyFathersWork-Windows-<version>.zip           full: audio as on the website (~450 MB)
#   dist\MyFathersWork-Windows-<version>-light.zip     lighter audio (~270 MB): speech 48 kbps mono, music 96 kbps (needs ffmpeg)
#   dist\MyFathersWork-Windows-<version>-no-audio.zip  no audio files (~60 MB); js/edition.js tells the app to hide sound controls
#
#   pwsh tools/build-windows.ps1 [-Version 0.66.6.123] [-Edition All|Full|Light|NoAudio]
#   (version default: computed from git like the Pages deploy; edition default: All)
#
# Needs the Git LFS files (git lfs pull --include "MyFathersWorkApp/**"), otherwise images and audio are pointers.
param(
    [string]$Version = "",
    [ValidateSet("All", "Full", "Light", "NoAudio")]
    [string]$Edition = "All"
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

# The light edition: speech (wwwroot/audio/vo) as 48 kbps mono, music and effects at 96 kbps. A file that would not get
# smaller stays as it is. Durations do not change, so Oliver's sentence timings (*.pauses.json) still fit.
function Compress-Audio([string]$audioDir) {
    if (-not (Get-Command ffmpeg -ErrorAction SilentlyContinue)) { throw "The light edition needs ffmpeg on the PATH" }
    $files = Get-ChildItem -LiteralPath $audioDir -Recurse -Filter *.mp3
    Write-Host "Compressing $($files.Count) audio files..."
    $failed = $files | ForEach-Object -ThrottleLimit ([Environment]::ProcessorCount) -Parallel {
        $speech = $_.FullName -match '[\\/]vo[\\/]'
        $opts   = if ($speech) { @("-ac", "1", "-b:a", "48k") } else { @("-b:a", "96k") }
        $tmp    = $_.FullName + ".tmp.mp3"
        & ffmpeg -v error -nostdin -y -i $_.FullName -map_metadata -1 @opts $tmp
        if ($LASTEXITCODE -or -not (Test-Path -LiteralPath $tmp)) { $_.FullName; return }
        if ((Get-Item -LiteralPath $tmp).Length -lt $_.Length) { Move-Item -LiteralPath $tmp -Destination $_.FullName -Force }
        else { Remove-Item -LiteralPath $tmp -Force }
    }
    if ($failed) { throw "ffmpeg failed on: $($failed -join ', ')" }
}

$editions = if ($Edition -eq "All") { @("Full", "Light", "NoAudio") } else { @($Edition) }
$zips     = @()

foreach ($ed in $editions) {
    if ((Test-Path -LiteralPath $out) -and $out.StartsWith($dist)) { Remove-Item -LiteralPath $out -Recurse -Force }
    New-Item -ItemType Directory -Force $out | Out-Null
    Copy-Item (Join-Path $work "launcher\MyFathersWork.exe") $out
    # The launcher serves plain files; the precompressed .br/.gz copies are not needed
    Copy-Item (Join-Path $work "app\wwwroot") $out -Recurse
    Get-ChildItem (Join-Path $out "wwwroot") -Recurse -Include *.br, *.gz | Remove-Item -Force

    $audioDir = Join-Path $out "wwwroot\audio"
    $suffix   = ""
    $about    = "Full version, with music, sound effects and narration."
    if ($ed -eq "Light") {
        Compress-Audio $audioDir
        $suffix = "-light"
        $about  = "Light version: the same app with smaller audio files (narration at a lower quality)."
    }
    elseif ($ed -eq "NoAudio") {
        # Oliver's clip lists are read when a game starts: keep them as empty lists, so nothing asks for a missing file
        $lists = Get-ChildItem -LiteralPath (Join-Path $audioDir "vo\oliver") -Filter *.json |
                 Where-Object { $_.Name -notlike "*.pauses.json" } | ForEach-Object Name
        if ($audioDir.StartsWith($out)) { Remove-Item -LiteralPath $audioDir -Recurse -Force }
        $listDir = New-Item -ItemType Directory -Force (Join-Path $audioDir "vo\oliver")
        foreach ($list in $lists) { Set-Content (Join-Path $listDir $list) "{}" -Encoding utf8 }
        Set-Content (Join-Path $out "wwwroot\js\edition.js") "window.mfwEdition = { audio: false };" -Encoding utf8
        $suffix = "-no-audio"
        $about  = "Version without audio: no music, sound effects or narration, and no sound controls."
    }

    @"
My Father's Work App (Unofficial) ($Version), offline Windows version
$about

1. Unzip this whole folder anywhere (keep MyFathersWork.exe next to the wwwroot folder).
2. Double-click MyFathersWork.exe. The app opens in its own window (Microsoft Edge in app mode).
   Closing the window stops the app. Double-clicking again while it runs opens another window.

No internet connection and no installation are needed. Saved games are kept on this PC
(%LOCALAPPDATA%\MyFathersWork) and stay when you unzip a newer version over this one.
Without Microsoft Edge the app opens in your default browser, with a console window you close to stop it.
Windows SmartScreen may warn about an unknown publisher the first time: More info > Run anyway.
"@ | Set-Content (Join-Path $out "README.txt") -Encoding utf8

    $zip = Join-Path $dist "MyFathersWork-Windows-$Version$suffix.zip"
    if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
    Write-Host "Zipping $ed..."
    Compress-Archive -Path $out -DestinationPath $zip
    $zips += $zip
}

Remove-Item -LiteralPath $work -Recurse -Force
foreach ($zip in $zips) { Write-Host "Done: $zip ($([math]::Round((Get-Item $zip).Length / 1MB)) MB)" }
