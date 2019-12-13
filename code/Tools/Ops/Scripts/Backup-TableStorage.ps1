#
# Backup-TableStorage.ps1
#
# This script backs up all the tables from the Azure storage account of specified source environment to the blob storage of the specified destination environment. 
# The blob storage name is of the format tablestorebackupyyyyMMddHHmmss (Ex. tablestorebackup20160816093429) 
# It uses the local file system as an intermediary because AzCopy does not support table-to-table copies.
# This script requires AzCopy v4.0 or higher installed in C:\Program Files (x86)\Microsoft SDKs\azure\AzCopy\AzCopy.exe (older versions of AzCopy cannot copy tables)
# This script requires Azure PowerShell version 2.1 or higher with Azure subscription loaded.
#
# Example Usage
# Backup-TableStorage -Source "sp-dev-test1" -Destination "sp-dev-test1"
#

function Backup-TableStorage
{
	<#
    .NOTES
       Name: Backup-TableStorage.ps1
       Requires: AzCopy v4.0 or higher installed in C:\Program Files (x86)\Microsoft SDKs\azure\AzCopy\AzCopy.exe. Azure PowerShell version 2.1 or higher with Azure subscription loaded.
    .SYNOPSIS
       Backs up all the tables from the Azure storage account of specified source environment to the blob storage of the specified destination environment.
    .DESCRIPTION
       This script backs up all the tables from the Azure storage account of specified source environment to the blob storage of the specified destination environment. 
       The blob storage name is of the format tablestorebackupyyyyMMddHHmmss (Ex. tablestorebackup20160816093429) .
       It uses the local file system as an intermediary because AzCopy does not support table-to-table copies.	   
    .PARAMETER Source
       Name of the Source Environment.
    .PARAMETER Destination
       Name of the Destination Environment. When not specified, the value is same as Source Environment.    
    #>

	param(
		[parameter(Mandatory=$true, HelpMessage='Name of the Source Environment.')]
		[Alias("Source")]
		[string]$SourceEnvironmentName,
		[parameter(Mandatory=$false, HelpMessage='Name of the Destination Environment. When not specified, the value is same as Source Environment.')]
		[Alias("Destination")]
		[string]$DestinationEnvironmentName
	)

	process
    {
		$ErrorActionPreference = "Stop"

		if (-not (Test-Path "${env:ProgramFiles(x86)}\Microsoft SDKs\Azure\AzCopy\AzCopy.exe"))
		{
			throw "Azcopy is not installed - get it from here: https://azure.microsoft.com/en-gb/documentation/articles/storage-use-azcopy/"
		}

		if (!($DestinationEnvironmentName)) 
		{
			Write-host "Destination environment is not specified. Setting destination environment same as source." -ForegroundColor Yellow
			$DestinationEnvironmentName = $SourceEnvironmentName
		}

		$sourceResourceGroup = $SourceEnvironmentName
		$sourceStorageAccountName = $SourceEnvironmentName.Replace("-","")
		$sourceStorageAccountContext = (Get-AzureRmStorageAccount -ResourceGroupName $sourceResourceGroup -Name $sourceStorageAccountName).Context
		# Breaking change in latest Azure Powershell version where Key1 is replaced with Value[0]
		$sourceStorageAccountKey = (Get-AzureRmStorageAccountKey -ResourceGroupName $sourceResourceGroup -Name $sourceStorageAccountName).Value[0]

		$destinationResourceGroup = $DestinationEnvironmentName
		$destinationStorageAccountName = $DestinationEnvironmentName.Replace("-","")
		# Breaking change in latest Azure Powershell version where Key1 is replaced with Value[0]
		$destinationStorageAccountKey = (Get-AzureRmStorageAccountKey -ResourceGroupName $destinationResourceGroup -Name $destinationStorageAccountName).Value[0]

		$arrayProdStorageAccounts = "spapi", "spmobisys", "spppe", "spprodbeihai", "spppebeihai", "spdevbeihai"
		if($arrayProdStorageAccounts -contains $sourceStorageAccountName)
		{
			$confirmation = Read-Host "The account $sourceStorageAccountName is a production storage account. Are you sure you want to proceed? [y/n]"
			if ($confirmation -ne 'y') 
			{
				exit;
			}
		}

		$azcopy = ${env:programfiles(x86)} + "\Microsoft SDKs\azure\AzCopy\AzCopy.exe"
		$timestamp = Get-Date -Format yyyyMMddHHmmss
		$destinationBlobContainer = "https://$destinationStorageAccountName.blob.core.windows.net/tablestorebackup" + $timestamp
		Write-host "Blob Container Location is $destinationBlobContainer"

		Get-AzureStorageTable -context $sourceStorageAccountContext | Select Name | % `
		{ 
			foreach ($property in $_.PSObject.Properties) 
			{ 
				$table = $($_.PSObject.properties[$property.Name].Value)
				$sourceTable = "https://$sourceStorageAccountName.table.core.windows.net/$table"

				Write-host ""
				Write-Host "Backing up Table: $sourceTable"-ForegroundColor Yellow
				Write-host "--------------------------" -ForegroundColor Yellow
				Write-host ""

				& $azcopy /source:$sourceTable /dest:$destinationBlobContainer /sourcekey:$sourceStorageAccountKey /destkey:$destinationStorageAccountKey

				Write-host ""
				Write-host "Backup completed: $sourceTable" -ForegroundColor Green
				Write-host ""
				Write-host ""
			} 
		}

		Write-host "Table storage is backed up at $destinationBlobContainer"
	}
}
