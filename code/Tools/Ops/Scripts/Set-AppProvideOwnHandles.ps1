<#
.NOTES
    Name: Set-AppProvideOwnHandles.ps1
    Requires: ManageApps.exe 
.SYNOPSIS
    This script configures an application with the permission to provide its own user handles
.DESCRIPTION
    Uses ManageApps.exe to set the flag ProvideOwnHandle in the configuration of an application.  
    If AppKey is not passed as a parameter, this script parses the source code of 
    UtilsInternal\Config\TestConstants.cs and uses the AppKey found there as a default
.PARAMETER EnvName
    Name of the environment.
.PARAMETER AppKey
    Application key already created in the environment. If parameter left empty, then this script will use the
    app key from TestConstants.cs
.PARAMETER ProvideOwnHandles
    Value of this flag [true|false]
#>

param(
    # caller must specify the environment name
    [Parameter(Mandatory=$true)]
    [string]$EnvName,

    [Parameter()]
    [string]$AppKey,

    [Parameter(Mandatory=$true)]
    [bool]$ProvideOwnHandles
)

if (!$AppKey)
{
    $path = Join-Path $PSScriptRoot "..\..\..\UtilsInternal\Config"

    # parse the contents of TestConstants.cs to determine the AppKey
    $envUnderscore = $EnvName -replace '-','_'
    $envUpper = $envUnderscore.ToUpper()
    $startOfRegion = "#if " + $envUpper + "$"
    $endOfRegion = "#endif"
    $appKeyPattern = '([\s]+)AppKey = "([\w-]+)";'
    $path = Join-Path $path "TestConstants.cs"
    $reader = [System.IO.File]::OpenText($path)
    while (($line = $reader.ReadLine()) -ne $null) {
        if ($line -match $startOfRegion) {
        $inRegion = $true
        }
        if ($inRegion -and ($line -match $endOfRegion)) {
        $inRegion = $false
        }
        if ($inRegion -and ($line -match $appKeyPattern)) {
            $AppKey = $matches[2]
        }
    }
}

if ($AppKey -eq $null) 
{
    throw "Cannot determine AppKey for $EnvName"
}

$startLocation = Get-Location

# step 1: get the app handle

$manageAppsRelPath = "..\ManageApps\bin\debug\"
$manageAppsExe = ".\ManageApps.exe"
$envOpt = "-Name=" + $EnvName
$actionOpt = "-GetAppHandle"
$appKeyOpt = "-AppKey=" + $AppKey

Set-Location $manageAppsRelPath

$output = & "$manageAppsExe" $envOpt $actionOpt $appKeyOpt

$expectedResultPattern = "AppHandle = ([\w-]+) for application with appKey = " + $AppKey
if ($output -match $expectedResultPattern) {
    $appHandle = $matches[1]
} else {
    Set-Location $startLocation
    throw "Unexpected output from ManageApps.exe:" + $output
}

# step 2: get the developerId

$manageAppsExe = ".\ManageApps.exe"
$envOpt = "-Name=" + $EnvName
$actionOpt = "-GetAppDeveloperId"
$appHandleOpt = "-AppHandle=" + $appHandle

$output = & "$manageAppsExe" $envOpt $actionOpt $appHandleOpt

$expectedResultPattern = "DeveloperId = ([\w-]+) for appHandle " + $appHandle
if ($output -match $expectedResultPattern) {
    $developerId = $matches[1]
} else {
    Set-Location $startLocation
    throw "Unexpected output from ManageApps.exe:" + $output
}

# step 3: set ProvideOwnHandles

$manageAppsExe = ".\ManageApps.exe"
$envOpt = "-Name=" + $EnvName
$actionOpt = "-UpdateAppProfile"
$appHandleOpt = "-AppHandle=" + $appHandle
$developerIdOpt = "-DeveloperId=" + $developerId
$platformTypeOpt = "-PlatformType=Windows"
$provideOwnHandlesOpt = "-ProvideOwnHandles=" + $provideOwnHandles

$output = & "$manageAppsExe" $envOpt $actionOpt $appHandleOpt $developerIdOpt $platformTypeOpt $provideOwnHandlesOpt 
$expectedResultPattern = "Updated app profile for appHandle " + $appHandle + " platform type Windows"
if (!($output -match $expectedResultPattern)) {
    Set-Location $startLocation
    $developerId
    throw "Unexpected output from ManageApps.exe:" + $output
}

# step 4: all done

Set-Location $startLocation

Write-Host "Success! ProvideOwnHandle set to ${provideOwnHandles}"
