
$envName = "sp-dev-alec";
$path = Join-Path (Get-Location) "..\..\UtilsInternal\Config"
$appKey = $null

$envUnderscore = $envName -replace '-','_'
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
    Write-Host "Cannot determine AppKey for $envName"
    return
}

$manageServerStateRelPath = "..\..\Server\ManageServerState\bin\debug\"
$manageServerStateExe = ".\ManageServerState.exe"
$forceOpt = "-Force"
$cleanOpt = "-Clean"
$createOpt = "-Create"
$allOpt = "-All"
$envOpt = "-Name=" + $envName
$startLocation = Get-Location

Set-Location $manageServerStateRelPath

# step 1: clean server state
& "$manageServerStateExe" $forceOpt $cleanOpt $allOpt $envOpt 

# sleep for 30 seconds to allow table deletion to succeed
Write-Host "sleeping for 30 seconds..."
Start-Sleep -Seconds 30

# step 2: create server state
& "$manageServerStateExe" $forceOpt $createOpt $allOpt $envOpt 

Set-Location $startLocation

$manageAppsRelPath = "..\..\Server\ManageApps\bin\debug\"
$manageAppsExe = ".\ManageApps.exe"
$envOpt = "-Name=" + $envName
$actionOpt = "-CreateAppAndDeveloper"
$platformOpt = "-PlatformType=Windows"
$appName = "EndToEndTestsApp"
$appNameOpt = "-AppName="+ $appName

Set-Location $manageAppsRelPath

# step 3: create a developer and an application
$output = & "$manageAppsExe" $envOpt $actionOpt $platformOpt $appNameOpt

$expectedResultPattern = "Application " + $appName + " created, appHandle = ([\w-]+) developerId = ([\w-]+)"
if ($output -match $expectedResultPattern) {
    $appHandle = $matches[1]
    $developerId = $matches[2]
} else {
    Write-Host "Unexpected output from ManageApps.exe:" + $output
    Return
}

$manageAppsExe = ".\ManageApps.exe"
$envOpt = "-Name=" + $envName
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
    Write-Host "Unexpected output from ManageApps.exe:" + $output
    Return
}

Set-Location $startLocation
Write-Host "Success!  Created app key $appKeyOutput"

