#
# Get-AllTables.ps1
#

function Get-AllTables
{
    <#
    .NOTES
       Name: Get-AllTables.ps1
       Requires: AzCopy v4.0 or higher installed in C:\Program Files (x86)\Microsoft SDKs\azure\AzCopy\AzCopy.exe
    .SYNOPSIS
       Downloads all tables from an Azure storage account to the local filesystem.
    .DESCRIPTION
       This script uses AzCopy to download all the tables from an Azure storage account into the specified local 
       directory. AzCopy creates a manifest file for each table that is downloaded, and this manifest file is required
       if you want to upload the table at any future point in time.
    .PARAMETER Acct
       Name of the Azure Storage Account.
    .PARAMETER Key
       Key to access the Azure Storage Account.
    .PARAMETER Dir       
       Path to the local directory where the tables will be downloaded to.
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
             HelpMessage='Path to the local directory where the tables will be downloaded to.'
        )]
        [string]$Dir
    )

    process
    {
        $azcopy = ${env:programfiles(x86)} + "\Microsoft SDKs\azure\AzCopy\AzCopy.exe"

        $ctx = New-AzureStorageContext -StorageAccountName $Acct -StorageAccountKey $Key

        if ($ctx.StorageAccountName -ne $Acct) {
             Write-Host "Failed to access storage account $Acct"
             return
        }
        
        Get-AzureStorageTable -Context $ctx | ForEach { 
            $sourceTable = $_.Uri.AbsoluteUri
            $Name = $_.Name
            Write-Host "Downloading $Name from $sourceTable to $Dir"

            # copy from Azure to the local directory
            & $azcopy /Source:$sourceTable /Dest:$Dir /SourceKey:$Key /Z:$Dir\journal /V:"$Dir\${Name}-export.log"
        }

        Write-Host "Downloads complete"
    }
}

