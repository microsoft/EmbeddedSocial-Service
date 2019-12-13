#
# Provision-AzureBatch.ps1
#
# This script creates and provisions Azure Batch Account and a Batch pool of 2 Large VMs for the specified environment in the specified location 
# The environment name without hyphens is used to prefix the names of the resources/assets created as part of Azure Batch 
# 1. Create Azure Batch Account
# 2. Create Azure Batch Pool
# Example Usage
# Provision-AzureBatch -Name "sp-dev-test1" -Location "NorthCentralUS" -NumberOfNodes 1
#

function Provision-AzureBatch
{
    <#
    .NOTES
       Name: Provision-AzureBatch.ps1
       Requires: Azure PowerShell version 2.1 or higher with Azure subscription loaded.
    .SYNOPSIS
       Creates and provisions Azure Batch Account and a Batch pool of 2 Large VMs for the specified environment in the specified location.
    .DESCRIPTION
       This script creates and provisions Azure Batch Account and a Batch pool of 2 Large VMs for the specified environment in the specified location.
       The environment name without hyphens is used to prefix the names of the resources/assets created as part of Azure Batch.
    .PARAMETER Name
       Name of the Environment.    
    .PARAMETER Location
       Location of Azure Batch. This is generally same as the location of the Environment.
     .PARAMETER NumberOfNodes
       Number of dedicated nodes in the pool.
    #>
    
    param(
        [parameter(Mandatory=$true, HelpMessage='Name of the Environment.')]
        [Alias("Name")]
        [string]$EnvironmentName,
        [parameter(Mandatory=$true, HelpMessage='Location of Azure Batch. This is generally same as the location of the Environment.')]
        [string]$Location,
        [parameter(HelpMessage='Number of dedicated nodes in the pool.')]
        [int]$NumberOfNodes = 2
    )

    process
    {
        $ErrorActionPreference = "Stop"

        $resourceGroup = $EnvironmentName
        $deploymentPrefix = $EnvironmentName.Replace("-","")
        # Names cannot be more than 24 characters
        $azureBatchAccountName = $deploymentPrefix + "batch"
        $azureBatchPoolName = $deploymentPrefix + "pool"

        # Create Batch Account 
        New-AzureRmBatchAccount -AccountName $azureBatchAccountName -ResourceGroupName $resourceGroup -Location $Location

        # Create Batch Pool
        $azureBatchContext = Get-AzureRmBatchAccountKeys -AccountName $azureBatchAccountName

        $autoScaleFormula = "$" + "TargetDedicated=" + $NumberOfNodes + ";"
        $configuration = New-Object -TypeName "Microsoft.Azure.Commands.Batch.Models.PSCloudServiceConfiguration" -ArgumentList @(3,"*")
        New-AzureBatchPool -Id $azureBatchPoolName -VirtualMachineSize "Large" -CloudServiceConfiguration $configuration -AutoScaleFormula $autoScaleFormula -BatchContext $azureBatchContext
    }
}
