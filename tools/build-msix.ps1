# Wraps the full offline Windows build (tools/build-windows.ps1: MyFathersWork.exe + wwwroot, all audio) as an MSIX
# package for the Microsoft Store. The Store signs packages itself; -Sign signs with the owner's Certum certificate
# only to install and test the package locally.
#
#   pwsh tools/build-msix.ps1 -Version 0.66.6.103 -IdentityName <Package/Identity/Name> -Publisher "CN=..." `
#        -PublisherDisplayName "<Publisher display name>"       # values from Partner Center > Product identity
#   pwsh tools/build-msix.ps1 -Version 0.66.6.103 -Sign          # local test: Publisher = the certificate's subject
#
# Input: dist\MyFathersWork-Windows-<version>.zip (build it first). Output: dist\MyFathersWork-<version>.msix.
# MSIX versions are Major.Minor.Build.Revision with Revision 0 (reserved by the Store): 0.66.6.<rev> -> 0.66.<rev>.0.
# makeappx / signtool come from the NuGet package Microsoft.Windows.SDK.BuildTools (see build-windows.ps1).
param(
    [Parameter(Mandatory)] [string]$Version,
    [string]$IdentityName = "MyFathersWorkUnofficial",
    [string]$Publisher = "",
    [string]$PublisherDisplayName = "Valentijn de Pagter",
    [switch]$Sign,
    [string]$CertThumbprint = ""
)
$ErrorActionPreference = "Stop"

$repo = Split-Path -Parent $PSScriptRoot
$dist = Join-Path $repo "dist"
$zip = Join-Path $dist "MyFathersWork-Windows-$Version.zip"
if (-not (Test-Path -LiteralPath $zip)) { throw "Build the Windows zip first: pwsh tools/build-windows.ps1 -Version $Version -Edition Full" }

$parts = $Version.Split(".")
if ($parts.Count -ne 4) { throw "Version must have four parts, e.g. 0.66.6.103" }
$msixVersion = "$($parts[0]).$($parts[1]).$($parts[3]).0"

$kit = @(Get-ChildItem "$env:USERPROFILE\.nuget\packages\microsoft.windows.sdk.buildtools\*\bin\*\x64\makeappx.exe" -ErrorAction SilentlyContinue
         Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin\*\x64\makeappx.exe" -ErrorAction SilentlyContinue) |
       Sort-Object FullName -Descending | Select-Object -First 1
if (-not $kit) { throw "makeappx.exe not found: restore the NuGet package Microsoft.Windows.SDK.BuildTools" }
$makeappx = $kit.FullName
$signtool = Join-Path $kit.DirectoryName "signtool.exe"

if ($Sign) {
    if (-not $CertThumbprint) {
        $CertThumbprint = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert |
            Where-Object { $_.HasPrivateKey -and $_.NotAfter -gt (Get-Date) } |
            Sort-Object NotAfter -Descending | Select-Object -First 1 -ExpandProperty Thumbprint
    }
    $cert = Get-Item "Cert:\CurrentUser\My\$CertThumbprint"
    # The manifest's Publisher must be the signing certificate's subject, exactly
    $Publisher = $cert.Subject
}
if (-not $Publisher) { throw "Pass -Publisher (Partner Center > Product identity > Package/Identity/Publisher), or -Sign for a local test" }

$stage = Join-Path $dist "msix"
if ((Test-Path -LiteralPath $stage) -and $stage.StartsWith($dist)) { Remove-Item -LiteralPath $stage -Recurse -Force }
New-Item -ItemType Directory $stage | Out-Null
Write-Host "Unpacking $zip..."
Expand-Archive -LiteralPath $zip -DestinationPath $stage
$app = Join-Path $stage "MyFathersWork-Windows"
Remove-Item -LiteralPath (Join-Path $app "README.txt") -ErrorAction SilentlyContinue

# Tile and Start menu images from the app icon, on the app's dark background
$assets = Join-Path $app "Assets"
New-Item -ItemType Directory $assets | Out-Null
$icon = Join-Path $repo "MyFathersWorkApp\MyFathersWorkApp\wwwroot\icon-512.png"
python (Join-Path $PSScriptRoot "msix-assets.py") $icon $assets
if ($LASTEXITCODE) { throw "making the tile images failed" }

function Escape-Xml([string]$s) { [System.Security.SecurityElement]::Escape($s) }
$name = "My Father's Work (Unofficial)"
@"
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
         xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
         IgnorableNamespaces="uap rescap">
  <Identity Name="$(Escape-Xml $IdentityName)" Publisher="$(Escape-Xml $Publisher)" Version="$msixVersion" ProcessorArchitecture="x64" />
  <Properties>
    <DisplayName>$(Escape-Xml $name)</DisplayName>
    <PublisherDisplayName>$(Escape-Xml $PublisherDisplayName)</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
  </Properties>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.26100.0" />
  </Dependencies>
  <Resources>
    <Resource Language="en-us" />
  </Resources>
  <Applications>
    <Application Id="MyFathersWork" Executable="MyFathersWork.exe" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements DisplayName="$(Escape-Xml $name)"
                          Description="Fan-made companion app for the board game My Father's Work: all three scenarios, narrated, offline."
                          BackgroundColor="#111111"
                          Square150x150Logo="Assets\Square150x150Logo.png" Square44x44Logo="Assets\Square44x44Logo.png">
        <uap:DefaultTile Wide310x150Logo="Assets\Wide310x150Logo.png" ShortName="My Father's Work" />
        <uap:SplashScreen Image="Assets\SplashScreen.png" BackgroundColor="#111111" />
      </uap:VisualElements>
    </Application>
  </Applications>
  <Capabilities>
    <rescap:Capability Name="runFullTrust" />
  </Capabilities>
</Package>
"@ | Set-Content (Join-Path $app "AppxManifest.xml") -Encoding utf8

$msix = Join-Path $dist "MyFathersWork-$Version.msix"
if (Test-Path -LiteralPath $msix) { Remove-Item -LiteralPath $msix -Force }
Write-Host "Packing $msix (version $msixVersion)..."
& $makeappx pack /d $app /p $msix /o
if ($LASTEXITCODE) { throw "makeappx failed" }

if ($Sign) {
    Write-Host "Signing for a local test ($Publisher)..."
    & $signtool sign /sha1 $CertThumbprint /fd sha256 /tr http://time.certum.pl /td sha256 $msix
    if ($LASTEXITCODE) { throw "signing failed (is SimplySign Desktop running and logged in?)" }
}

Remove-Item -LiteralPath $stage -Recurse -Force
Write-Host "Done: $msix ($([math]::Round((Get-Item $msix).Length / 1MB)) MB)"
