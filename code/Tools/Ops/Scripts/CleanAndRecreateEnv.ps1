<#
.NOTES
    Name: CleanAndRecreateEnv.ps1
    Requires: ManageServerState.exe and ManageApps.exe 
.SYNOPSIS
    This script cleans up the state of a social plus environment and recreates it.
.DESCRIPTION
    Uses ManageServerState.exe to clean up and then recreate a social plus environment.  After the
    new environment is created, it uses ManageApps.exe to create an app. To determine which AppKey to 
    insert into the social plus environment, this script parses the source code of 
    UtilsInternal\Config\TestConstants.cs
.PARAMETER EnvName
    Name of the environment to clean and recreate.
.PARAMETER Delay
    Amount of time in seconds to sleep between the clean step and the recreate step.
#>

param(
    # caller must specify the environment name
    [Parameter(Mandatory=$true)]
    [string]$EnvName,

    [Parameter(
        HelpMessage='Number of seconds to sleep after clean before create'
    )]
    [int]$Delay = 30
)

$path = Join-Path $PSScriptRoot "..\..\..\UtilsInternal\Config"
$appKey = $null

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
        $appKey = $matches[2]
    }
}

if ($appKey -eq $null) 
{
    throw "Cannot determine AppKey for $EnvName"
}

# step 1: clean server state

$manageServerStateRelPath = "..\ManageServerState\bin\debug\"
$manageServerStateExe = ".\ManageServerState.exe"
$forceOpt = "-Force"
$cleanOpt = "-Clean"
$createOpt = "-Create"
$allOpt = "-All"
$envOpt = "-Name=" + $EnvName
$startLocation = Get-Location

Set-Location $manageServerStateRelPath

& "$manageServerStateExe" $forceOpt $cleanOpt $allOpt $envOpt 

# sleep for delay seconds to allow table deletion to succeed
Write-Verbose "sleeping for $Delay seconds..."
Start-Sleep -Seconds $Delay

# step 2: create new (empty) server state

& "$manageServerStateExe" $forceOpt $createOpt $allOpt $envOpt 

Set-Location $startLocation

$manageAppsRelPath = "..\ManageApps\bin\debug\"
$manageAppsExe = ".\ManageApps.exe"
$envOpt = "-Name=" + $EnvName
$actionOpt = "-CreateAppAndDeveloper"
$platformOpt = "-PlatformType=Windows"
$appName = "EndToEndTestsApp"
$appNameOpt = "-AppName="+ $appName

Set-Location $manageAppsRelPath

# step 3: create a new developer and a new application

$output = & "$manageAppsExe" $envOpt $actionOpt $platformOpt $appNameOpt

$expectedResultPattern = "Application " + $appName + " created, appHandle = ([\w-]+) developerId = ([\w-]+)"
if ($output -match $expectedResultPattern) {
    $appHandle = $matches[1]
    $developerId = $matches[2]
} else {
    Set-Location $startLocation
    throw "Unexpected output from ManageApps.exe:" + $output
}

$manageAppsExe = ".\ManageApps.exe"
$envOpt = "-Name=" + $EnvName
$actionOpt = "-CreateAppKey"
$appHandleOpt = "-AppHandle=" + $appHandle
$developerIdOpt = "-DeveloperId=" + $developerId
$appKeyOpt = "-AppKey=" + $appKey

# step 4: use a specific app key with the new application

$output = & "$manageAppsExe" $envOpt $actionOpt $appHandleOpt $developerIdOpt $appKeyOpt

$expectedResultPattern = "AppKey = ([\w-]+) created for appHandle = ([\w-]+)"
if ($output -match $expectedResultPattern) {
    $appKeyOutput = $matches[1]
} else {
    Set-Location $startLocation
    throw "Unexpected output from ManageApps.exe:" + $output
}

$manageAppsExe = ".\ManageApps.exe"
$envOpt = "-Name=" + $EnvName
$actionOpt = "-UpdateValidation"
$appHandleOpt = "-AppHandle=" + $appHandle
$developerIdOpt = "-DeveloperId=" + $developerId
$enableValidationOpt = "-EnableValidation"
$validateTextOpt = "-ValidateText=true"
$validateImagesOpt = "-ValidateImages=true"
$allowMatureContentOpt = "-AllowMatureContent=true"
$userReportThresholdOpt = "-UserReportThreshold=1"
$contentReportThresholdOpt = "-ContentReportThreshold=1"

# step 5: configure validation options

$output = & "$manageAppsExe" $envOpt $actionOpt $appHandleOpt $developerIdOpt $enableValidationOpt $validateTextOpt $validateImagesOpt $allowMatureContentOpt $userReportThresholdOpt $contentReportThresholdOpt

$expectedResultPattern = "Updated content validation configuration for appHandle = ([\w-]+), developerId = ([\w-]+)"
if ($output -match $expectedResultPattern) {
    $appHandle = $matches[1]
    $developerId = $matches[2]
} else {
    Set-Location $startLocation
    throw "Unexpected output from ManageApps.exe:" + $output
}

# step 6: set AADS2S credentials

$clientId = "72f988bf-86f1-41af-91ab-2d7cd011db47"
$clientSecret = "N/A"
$redirectUri = "https://embeddedsocial.microsoft.com/testclient1"

$manageAppsExe = ".\ManageApps.exe"
$envOpt = "-Name=" + $EnvName
$actionOpt = "-UpdateIdentityProvider"
$appHandleOpt = "-AppHandle=" + $appHandle
$developerIdOpt = "-DeveloperId=" + $developerId
$identityProviderOpt = "-IdentityProvider=AADS2S"
$clientIdOpt = "-ClientId=" + $clientId
$clientSecretOpt = "-ClientSecret=" + $clientSecret
$redirectUriOpt = "-RedirectUri=" + $redirectUri

$output = & "$manageAppsExe" $envOpt $actionOpt $appHandleOpt $developerIdOpt $identityProviderOpt $clientIdOpt $clientSecretOpt $redirectUriOpt
$expectedResultPattern = "Updated identity provider config for appHandle " + $appHandle + ", identity provider type AADS2S"
if (!($output -match $expectedResultPattern)) {
    Set-Location $startLocation
    throw "Unexpected output from ManageApps.exe:" + $output
}

# step 7: set DisableHandleValidation to true for our own app keys

$disableHandleValidationOpt = $true

$manageAppsExe = ".\ManageApps.exe"
$envOpt = "-Name=" + $EnvName
$actionOpt = "-UpdateAppProfile"
$appHandleOpt = "-AppHandle=" + $appHandle
$developerIdOpt = "-DeveloperId=" + $developerId
$platformTypeOpt = "-PlatformType=Windows"
$disableHandleValidationOpt = "-DisableHandleValidation=" + $disableHandleValidationOpt

$output = & "$manageAppsExe" $envOpt $actionOpt $appHandleOpt $developerIdOpt $platformTypeOpt $disableHandleValidationOpt 
$expectedResultPattern = "Updated app profile for appHandle " + $appHandle + " platform type Windows"
if (!($output -match $expectedResultPattern)) {
    Set-Location $startLocation
    $developerId
    throw "Unexpected output from ManageApps.exe:" + $output
}

# step 8: all done with app creation
Set-Location $startLocation
Write-Host "Success!  Created app key $appKeyOutput"
