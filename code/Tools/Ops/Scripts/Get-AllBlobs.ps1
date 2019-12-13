#
# Get-AllBlobs.ps1
#

function Get-AllBlobs
{
    <#
    .NOTES
       Name: Get-AllBlobs.ps1
       Requires: AzCopy installed in C:\Program Files (x86)\Microsoft SDKs\azure\AzCopy\AzCopy.exe
    .SYNOPSIS
       Downloads all blobs from an Azure storage account to the local filesystem.
    .DESCRIPTION
       This script uses AzCopy to download all the blobs from an Azure storage account into the specified local directory.
       It creates subdirectories for each container, and the blobs in a container are stored in files whose
       name corresponds to the blob name.
       It skips over the following containers: "vsdeploy", "wad-crashdumps", and "wad-iis-logfiles".
    .PARAMETER Acct
       Name of the Azure Storage Account.
    .PARAMETER Key
       Key to access the Azure Storage Account.
    .PARAMETER Dir       
       Path to the local directory where the blobs will be downloaded to.
    #>

    param (
        [Parameter(
             HelpMessage='Name of the Azure Storage Account.'
        )]
        [string]$Acct,

        [Parameter(
             HelpMessage='Key to access the Azure Storage Account.'
        )]
        [string]$Key,

        [Parameter(
             HelpMessage='Path to the local directory where the blobs will be downloaded to.'
        )]
        [string]$Dir
    )

    process
    {
        $azcopy = ${env:programfiles(x86)} + "\Microsoft SDKs\azure\AzCopy\AzCopy.exe"

        $ctx = New-AzureStorageContext -StorageAccountName $Acct -StorageAccountKey $Key

        if ($ctx.StorageAccountName -ne "$Acct") {
             Write-Host "Failed to access storage account $Acct"
             return
        }
        
        Get-AzureStorageContainer -Context $ctx | ForEach { 
            $containerName = $_.Name

            if ($containerName -ne "vsdeploy" -and $containerName -ne "wad-iis-logfiles" -and $containerName -ne "wad-crashdumps") {
                $containerUri = $ctx.BlobEndPoint +  $containerName
                
                $destDir = Join-Path $Dir $containerName

                Write-Host "Copying blobs from $containerUri to $destDir"

                # check that the local directory does not already exist
                if (!(Test-Path -PathType Container $destDir)) {
                    New-Item -ItemType Directory -Path $destDir
                }

                # download each item from the container
                & $azcopy /Source:$containerUri /Dest:$destDir /SourceKey:$Key /S /MT /V:"$Dir\${containerName}-export.log"
            }
        }

        Write-Host "Downloads complete"
    }
}

