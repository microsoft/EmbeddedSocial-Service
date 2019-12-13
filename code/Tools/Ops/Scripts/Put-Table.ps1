#
# Put-Table.ps1
#
# This script uploads a table from the local directory to an Azure table.
# This script requires AzCopy v4.0 or higher (older versions of AzCopy cannot copy tables)
#

function Put-Table 
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

        Write-Host "Uploading Table $Name"

        $destTable = "https://$StorageAcct.table.core.windows.net/$Name"

        $manifestPrefix = "${StorageAcct}_${Name}"
        $matchingFiles = Get-ChildItem $Dir -Filter "${manifestPrefix}_*.manifest" | Where-Object { $_.Attributes -ne "Directory" }
        if (($matchingfiles).Count -ne 1) 
        {
            Write-Host "Cannot upload table: Expected only 1 manifest file, found $count"
            return
        } 
        
        $manifestName = ${matchingFiles}.Name

        # copy from the local directory to Azure
        & $azcopy /Source:$Dir /Dest:$destTable /DestKey:$StorageKey /Manifest:$manifestName /EntityOperation:InsertOrReplace /Z:$Dir\journal /V:"$Dir\${Name}-import.log"

        Write-Host "Upload complete"
    }
}

