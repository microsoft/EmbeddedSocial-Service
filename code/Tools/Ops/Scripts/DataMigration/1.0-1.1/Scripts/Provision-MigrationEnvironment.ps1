#
# Provision-MigrationEnvironment.ps1
#
# This script provisions all the assets required to execute the pipelines for 1.0->1.1 store migration
# The environment name without hyphens is used to identify the names of the resources/assets created as part of Data Migration
# 1. Create Azure Batch
# 2. Create Data Factory
# 3. Upload activities zip file
#
# Note that this is a container script that in turn calls Provision-AzureBatch, Provision-DataFactory, Provision-DataFactoryActivities
# Any change in parameters/convention for any of these scripts may need a change Provision-MigrationEnvironment.ps1
#
# Example Usage
# Provision-MigrationEnvironment -Name "sp-dev-test1" -Location "NorthCentralUS" -DataFactoryLocation "WestUS" -DataFactoryActivitiesContainerName "datafactoryactivities" -DataFactoryActivitiesFilePath "D:\SocialPlus\MyRepo\code\Server\DataFactory\bin\Release\SocialPlus.Server.DataFactory.zip" -NumberOfNodes 2
#

function Provision-MigrationEnvironment
{
    <#
    .NOTES
       Name: Provision-MigrationEnvironment.ps1
       Requires: Azure PowerShell version 2.1 or higher with Azure subscription loaded.
    .SYNOPSIS
       Provisions all the assets required to execute the pipelines for 1.0->1.1 store migration.
    .DESCRIPTION
       This script provisions all the assets required to execute the pipelines for 1.0->1.1 store migration.
       The environment name without hyphens is used to identify the names of the resources/assets created as part of Data Migration.
    .PARAMETER Name
       Name of the Environment.    
    .PARAMETER Location
       Location of the Environment.
     .PARAMETER DataFactoryLocation
       Location of the Data Factory. Check the latest list of available regions for Data Factory.
     .PARAMETER DataFactoryActivitiesContainerName
       Name of the azure storage container for datafactory activities.
     .PARAMETER DataFactoryActivitiesFilePath
       Path to datafactory activities zip file.
     .PARAMETER NumberOfNodes
       Number of dedicated nodes in the batch pool.
    #>
    
    param(
        [parameter(Mandatory=$true, HelpMessage='Name of the Environment.')]
        [Alias("Name")]
        [string]$EnvironmentName,
        [parameter(Mandatory=$true, HelpMessage='Location of the Environment.')]
        [string]$Location,
        [parameter(Mandatory=$true, HelpMessage='Location of the Data Factory. Check the latest list of available regions for Data Factory.')]
        [string]$DataFactoryLocation,
        [parameter(Mandatory=$true, HelpMessage='Name of the azure storage container for datafactory activities.')]
        [string]$DataFactoryActivitiesContainerName,
        [parameter(Mandatory=$true, HelpMessage='Path to datafactory activities zip file.')]
        [string]$DataFactoryActivitiesFilePath,	
        [parameter(HelpMessage='Number of dedicated nodes in the batch pool.')]
        [int]$NumberOfNodes = 2	
    )
    
    process
    {
        $ErrorActionPreference = "Stop"

        $resourceGroup = $EnvironmentName
        $deploymentPrefix = $EnvironmentName.Replace("-","")

        # Provision Azure batch 
        Provision-AzureBatch -Name $EnvironmentName -Location $Location -NumberOfNodes $NumberOfNodes

        # The Azure batch account and pool are named using the following convention as part of Provision-AzureBatch.ps1 script
        $azureBatchAccountName = $deploymentPrefix + "batch"
        $azureBatchPoolName = $deploymentPrefix + "pool"

        # Provision data factory 
        Provision-DataFactory -Name $EnvironmentName -Location $DataFactoryLocation -BatchAccountName $azureBatchAccountName -BatchPoolName $azureBatchPoolName -BatchLocation $Location

        # Provision data factory activities
        Provision-DataFactoryActivities -Name $EnvironmentName -ContainerName $DataFactoryActivitiesContainerName -ActivitiesFilePath $DataFactoryActivitiesFilePath	
    }
}