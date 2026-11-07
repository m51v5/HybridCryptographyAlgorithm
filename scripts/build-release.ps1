<#
.SYNOPSIS
  Builds the release exe with its license files, and optionally zips them.

.DESCRIPTION
  1. Stops a running copy (a running exe can't be overwritten).
  2. Publishes one self-contained HybridCryptographyAlgorithm.exe to -Out.
  3. Writes THIRD_PARTY_LICENSES.txt for every package that ships in the exe, including the .NET runtime.
     A package under a license not in $allowed stops the build until it's reviewed.
  4. Copies LICENSE next to them, and zips the three files if -Zip is given.

.EXAMPLE
  .\scripts\build-release.ps1
  .\scripts\build-release.ps1 -Zip hybrid-cryptography-algorithm-v1.0.0-windows-x64.zip
#>
param(
    # Folder that gets the exe and license files (default: dist\HybridCryptographyAlgorithm in the repo).
    [string]$Out = (Join-Path $PSScriptRoot '..\dist\HybridCryptographyAlgorithm'),
    # Zip file to create from those files.
    [string]$Zip
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Set-Location $root

$project = 'src\HybridCryptographyAlgorithm\HybridCryptographyAlgorithm.csproj'
$rid = 'win-x64'
# Licenses a shipped package may have. Add one only after reading it.
$allowed = @('MIT', 'Apache-2.0', 'BSD-2-Clause', 'BSD-3-Clause')

function Step($text) { Write-Host "`n== $text" -ForegroundColor Cyan }

function Invoke-Checked($what, [scriptblock]$command) {
    & $command
    if ($LASTEXITCODE -ne 0) { throw "$what failed (exit code $LASTEXITCODE)" }
}

function Get-Licenses {
    $assets = Get-Content "src\HybridCryptographyAlgorithm\obj\project.assets.json" -Raw -Encoding UTF8 | ConvertFrom-Json
    $packages = @($assets.packageFolders.PSObject.Properties.Name)[0]
    $target = $assets.targets.PSObject.Properties | Where-Object Name -like "*/$rid" | Select-Object -First 1

    # Packages with code that ends up in the exe
    $shipped = foreach ($lib in $target.Value.PSObject.Properties) {
        if ($lib.Value.type -eq 'package' -and $lib.Value.runtime) {
            $id, $version = $lib.Name -split '/'
            [pscustomobject]@{ Id = $id; Version = $version }
        }
    }
    # The .NET runtime, bundled because the exe is self-contained
    $framework = @($assets.project.frameworks.PSObject.Properties)[0].Value
    $shipped += foreach ($dep in $framework.downloadDependencies) {
        if ($dep.name -match "^Microsoft\.(NETCore|WindowsDesktop)\.App\.Runtime\.$rid$") {
            [pscustomobject]@{ Id = $dep.name; Version = ($dep.version -replace '[\[\]\s]', '' -split ',')[0] }
        }
    }

    $mit = Get-Content (Join-Path $root 'LICENSE') -Raw -Encoding UTF8
    $mitBody = $mit.Substring($mit.IndexOf('Permission is hereby granted'))

    $text = "Third-party licenses for Hybrid Cryptography Algorithm.`r`n" +
            "The exe bundles these packages, including the .NET runtime.`r`n"
    foreach ($p in $shipped | Sort-Object Id) {
        $dir = Join-Path $packages "$($p.Id.ToLower())\$($p.Version)"
        $nuspec = Get-ChildItem $dir -Filter *.nuspec | Select-Object -First 1
        $meta = ([xml](Get-Content $nuspec.FullName -Raw -Encoding UTF8)).package.metadata
        $license = if ($meta.license) { $meta.license.'#text' } else { '' }
        if ($meta.license.type -eq 'expression' -and $allowed -notcontains $license) {
            throw "$($p.Id) $($p.Version) is under '$license', which isn't in `$allowed. Read it, then add it if it's fine."
        }

        $text += "`r`n" + ('=' * 78) + "`r`n$($p.Id) $($p.Version)`r`nLicense: $license`r`n"
        if ($meta.projectUrl) { $text += "Project: $($meta.projectUrl)`r`n" }
        $text += ('=' * 78) + "`r`n`r`n"

        $files = Get-ChildItem $dir -File | Where-Object Name -match '^(LICENSE|THIRD-PARTY-NOTICES)(\.txt|\.md)?$'
        if ($files) {
            foreach ($f in $files) { $text += (Get-Content $f.FullName -Raw -Encoding UTF8).TrimEnd() + "`r`n`r`n" }
        }
        elseif ($license -eq 'MIT') {
            $holder = if ($meta.copyright) { $meta.copyright } else { "Copyright (c) $($meta.authors)" }
            $text += "$holder`r`n`r`n$($mitBody.TrimEnd())`r`n`r`n"
        }
        else {
            throw "$($p.Id) $($p.Version) has no license text in its package. Add it by hand."
        }
    }
    return $text
}

$running = Get-Process HybridCryptographyAlgorithm -ErrorAction SilentlyContinue
if ($running) {
    Step 'Stopping the running Hybrid Cryptography Algorithm'
    $running | Stop-Process -Force
    Start-Sleep -Milliseconds 500
}

Step 'Publishing the self-contained exe (takes a minute)'
Invoke-Checked 'dotnet publish' {
    dotnet publish $project -c Release -r $rid --self-contained `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true -p:DebugType=none -o $Out
}
$Out = (Resolve-Path $Out).Path

Step 'Writing THIRD_PARTY_LICENSES.txt'
$licenses = Join-Path $Out 'THIRD_PARTY_LICENSES.txt'
[IO.File]::WriteAllText($licenses, (Get-Licenses), (New-Object Text.UTF8Encoding $false))
Copy-Item -Force LICENSE $Out

$exe = Join-Path $Out 'HybridCryptographyAlgorithm.exe'
$size = [math]::Round((Get-Item $exe).Length / 1MB, 1)
Write-Host "`nDone: $exe ($size MB)" -ForegroundColor Green

if ($Zip) {
    Step "Zipping to $Zip"
    Compress-Archive -Force -Path $exe, (Join-Path $Out 'LICENSE'), $licenses -DestinationPath $Zip
    Write-Host "Zip: $((Resolve-Path $Zip).Path) ($([math]::Round((Get-Item $Zip).Length / 1MB, 1)) MB)" -ForegroundColor Green
}
