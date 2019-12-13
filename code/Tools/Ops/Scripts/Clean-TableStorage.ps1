#
# Clean-TableStorage.ps1
#
# This script removes all the tables from Azure storage account of the specified Environment. 
# This script requires Azure PowerShell version 2.1 or higher with Azure subscription loaded.
#
# Example Usage
# Clean-TableStorage -Name "sp-dev-test1"
#

function Clean-TableStorage
{
	<#
    .NOTES
       Name: Clean-TableStorage.ps1
       Requires: Azure PowerShell version 2.1 or higher with Azure subscription loaded.
    .SYNOPSIS
       Removes all the tables from Azure storage account of the specified Environment
    .DESCRIPTION
       This script removes all the tables from Azure storage account of the specified Environment.	   
    .PARAMETER Name
       Name of the Source Environment.    
    #>
	
	param(
		[parameter(Mandatory=$true, HelpMessage='Name of the Environment.')]
		[Alias("Name")]
		[string]$EnvironmentName
	)

	process
    {
		$ErrorActionPreference = "Stop"

		$resourceGroup = $EnvironmentName
		$storageAccountName = $EnvironmentName.Replace("-","")
		$storageAccountContext = (Get-AzureRmStorageAccount -ResourceGroupName $resourceGroup -Name $storageAccountName).Context

		$arrayProdStorageAccounts = "spapi", "spmobisys", "spppe", "spprodbeihai", "spppebeihai", "spdevbeihai"
		if($arrayProdStorageAccounts -contains $storageAccountName)
		{
			$confirmation = Read-Host "The account $storageAccountName is a production storage account. Are you sure you want to proceed? [y/n]"
			if ($confirmation -ne 'y') 
			{
				exit;
			}
		}

		Get-AzureStorageTable -context $storageAccountContext | Select Name | % `
		{ 
			foreach ($property in $_.PSObject.Properties) 
			{ 
				$table = $($_.PSObject.properties[$property.Name].Value)
				
				Write-host ""
				Write-Host "Deleting Table: $table"-ForegroundColor Yellow
				Write-host "--------------------------" -ForegroundColor Yellow
				Write-host ""

				& Remove-AzureStorageTable -Name $table -Context $storageAccountContext -Force

				Write-host ""
				Write-host "Deletion completed: $table" -ForegroundColor Green
				Write-host ""
				Write-host ""
			}
		}

		Write-host "Table storage is deleted"
	}
}
