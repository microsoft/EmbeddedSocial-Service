#
# Get-Table.ps1
#
# This script downloads a table from Azure to the local directory specified.
# This script requires AzCopy v4.0 or higher (older versions of AzCopy cannot copy tables)
#

function Get-Table 
{
    param (
        [string]$Name,
        [string]$StorageAcct,
        [string]$StorageKey,
        [string]$Dir
    )

    process
    {
        $azcopy = "c:\Program Files (x86)\Microsoft SDKs\azure\AzCopy\AzCopy.exe"

        Write-Host "Downloading Table $Name"

        $sourceTable = "https://$StorageAcct.table.core.windows.net/$Name"

        # copy from Azure to the local directory
        & $azcopy /Source:$sourceTable /Dest:$Dir /SourceKey:$StorageKey /Z:$Dir\journal /V:"$Dir\${Name}-export.log"

        $manifestPrefix = "${StorageAcct}_${Name}"
        $matchingFiles = Get-ChildItem $Dir -Filter "${manifestPrefix}_*.manifest" | Where-Object { $_.Attributes -ne "Directory" }

        if (($matchingfiles).Count -ne 1) 
        {
            Write-Host "Wrong number of manifest files: count = $count"
        } 

        Write-Host "Download complete"
    }
}

