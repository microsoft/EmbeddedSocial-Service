#
# Get-TableStorageStatistics.ps1
#
# This script gets statistics like Row count for all the tables from the storage account of specified environment. 
# This script requires Azure PowerShell version 2.1 or higher with Azure subscription loaded.
#
# Example Usage
# Get-TableStorageStatistics -Name "sp-dev-test1"
#

function Get-TableStorageStatistics
{
	<#
    .NOTES
       Name: Get-TableStorageStatistics.ps1
       Requires: Azure PowerShell version 2.1 or higher with Azure subscription loaded.
    .SYNOPSIS
       Gets statistics like Row count for all the tables from the Azure storage account of specified environment
    .DESCRIPTION
       This script gets statistics like Row count for all the tables from the storage account of specified environment. 
    .PARAMETER Name
       Name of the Environment.    
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

		# Get the count of rows from the specified storage table
		function GetRowCount($table)
		{
			# Create a table query.
			$query = New-Object Microsoft.WindowsAzure.Storage.Table.TableQuery
			# Define columns to select. 
			$list = New-Object System.Collections.Generic.List[string] 
			$list.Add("PartitionKey")
			# Set query details.
			$query.SelectColumns = $list
			# Execute the query.
			$entities = $table.CloudTable.ExecuteQuery($query)
			($entities | measure).Count
		}

		# Ignore WAD* tables if any
		Get-AzureStorageTable -context $storageAccountContext | Select Name | where Name -notlike "WAD*" | % `
		{ 
			foreach ($property in $_.PSObject.Properties) 
			{ 
				$tableName = $($_.PSObject.properties[$property.Name].Value)
				$tableReference = Get-AzureStorageTable $tableName -Context $storageAccountContext
				$tableRowCount = GetRowCount $tableReference  
				Write-host "Row count for $tableName table:  $tableRowCount" -ForegroundColor Green
			}
		} 
	}
}



