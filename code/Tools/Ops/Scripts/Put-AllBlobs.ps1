#
# Put-AllBlobs.ps1
#

function Put-AllBlobs
{
    <#
    .NOTES
       Name: Put-AllBlobs.ps1
       Requires: AzCopy installed in C:\Program Files (x86)\Microsoft SDKs\azure\AzCopy\AzCopy.exe
    .SYNOPSIS
       Uploads all blobs from the local filesystem to an Azure storage account.
    .DESCRIPTION
       This script uses AzCopy to upload all the blobs from the specified local directory into an Azure storage account.
       It creates containers for each subdirectory in the specified local directory, if those container names
       do not already exist.
    .PARAMETER Acct
       Name of the Azure Storage Account.
    .PARAMETER Key
       Key to access the Azure Storage Account.
    .PARAMETER Dir       
       Path to the local directory where the blobs will be uploaded from.
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
             HelpMessage='Path to the local directory where the blobs will be uploaded from.'
        )]
        [string]$Dir
    )

    process
    {
        $azcopy = ${env:programfiles(x86)} + "\Microsoft SDKs\azure\AzCopy\AzCopy.exe"

        $ctx = New-AzureStorageContext -StorageAccountName $Acct -StorageAccountKey $Key

        if ($ctx.StorageAccountName -ne "$Acct") 
        {
            Write-Host "Failed to access storage account $Acct"
            return
        }

        $containerNames = Get-ChildItem $Dir | Where-Object { $_.Attributes -eq "Directory" }

        $containerNames | ForEach {
            $container = $_.Name
            Write-Host "container is $container"

            # check if container already exists, and create it if necessary
            $azContainer = Get-AzureStorageContainer -Name $container -Context $ctx -ErrorAction "SilentlyContinue"

            if (($azContainer | Measure-Object).Count -eq 0) {
                Write-Host "Creating container $container"
                $azContainer = New-AzureStorageContainer -Name $container -Context $ctx
            }

            $containerUri = $ctx.BlobEndPoint +  $container
            $srcDir = Join-Path $Dir $container

            Write-Host "Copying blobs from $srcDir to $containerUri"

            # upload all the blobs
            & $azcopy /Source:$srcDir /Dest:$containerUri /DestKey:$Key /S /Y /V:"$Dir\$container-import.log"
        }

        Write-Host "Uploads complete"
    }
}

