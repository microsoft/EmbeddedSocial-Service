#
# ValidateNuGetVersion.ps1 DLLFile NuspecFile
#
# This script takes in a DLL file and a Nuspec file and checks:
#   1/ The DLL's product version is equal the DLL's file version
#   2/ The DLL's product/file version is equal to the Nuspec version
#

param (
    [Parameter(
        Mandatory=$true,
        HelpMessage='Name of DLL file'
    )]
    [string] $DLLFileName,

    [Parameter(
        Mandatory=$true,
        HelpMessage='Name of Nuspec file'
    )]
    [string] $NuspecFileName
)

# Read DLL versions. Each DLL has a Product and a File version. Our convention is that they need to match
$DLLVersion = Get-Item $DLLFileName | Select-Object -ExpandProperty VersionInfo 
$DLLProductVersion = $DLLVersion.ProductVersion
$DLLFileVersion = $DLLVersion.FileVersion

# Read nuspec version
[xml]$Nuspec = Get-Content -Path $NuspecFileName
$NuspecLongVersion = $Nuspec.package.metadata.version

Write-Host "DLL: $DLLFileName; ProdVersion: $DLLProductVersion; FileVersion: $DLLFileVersion"
Write-Host "Nuspec: $NuspecFilename; Version: $NuspecLongVersion"

# Trim any suffix after "-" if present
$NuspecVersion = ($NuspecLongVersion -split '-')[0]

if ($DLLProductVersion -ne $DLLFileVersion) {
    Write-Error "DLLs product and file versions do not match"
    exit 1
}

# Check Nuspec version matches DLL version. 
if ($DLLProductVersion -ne $NuspecVersion)
{
    Write-Error "DLLs and Nuspec versions do not match"
    exit 1
}
exit 0
