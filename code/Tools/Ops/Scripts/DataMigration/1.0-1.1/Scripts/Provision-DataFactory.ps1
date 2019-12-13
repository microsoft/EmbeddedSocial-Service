#
# Provision-DataFactory.ps1
#
# This script creates and provisions the datafactory assets required for data migration in the specified location for the specified environment 
# The environment name without hyphens is used to prefix the names of the assets created as part of datafactory
# 1. Create Data Factory
# 2. Create AzureStorageLinkedService
# 3. Create AzureBatchLinkedService
# 4. Create Datasets
# Example Usage
# Provision-DataFactory -Name "sp-dev-test1" -Location "WestUS" -BatchAccountName "spdevtest1batch" -BatchPoolName "spdevtest1pool" -BatchLocation "NorthCentralUS"
#

function Provision-DataFactory
{
    <#
    .NOTES
       Name: Provision-DataFactory.ps1
       Requires: Azure PowerShell version 2.1 or higher with Azure subscription loaded.
    .SYNOPSIS
       Creates and provisions the datafactory assets required for data migration in the specified location for the specified environment.
    .DESCRIPTION
       This script creates and provisions the datafactory assets required for data migration in the specified location for the specified environment.
       The environment name without hyphens is used to prefix the names of the assets created as part of datafactory.
    .PARAMETER Name
       Name of the Environment.    
    .PARAMETER Location
       Location of Data Factory. Check the latest list of available regions for Data Factory.
     .PARAMETER BatchAccountName
       Account Name of Azure Batch.
     .PARAMETER BatchPoolName
       Pool Name of Azure Batch.
     .PARAMETER BatchLocation
       Location of Azure Batch.
    #>

    param(
        [parameter(Mandatory=$true, HelpMessage='Name of the Environment.')]
        [Alias("Name")]
        [string]$EnvironmentName,
        [parameter(Mandatory=$true, HelpMessage='Location of Data Factory. Check the latest list of available regions for Data Factory.')]
        [string]$Location,
        [parameter(Mandatory=$true, HelpMessage='Account Name of Azure Batch.')]
        [string]$BatchAccountName,
        [parameter(Mandatory=$true, HelpMessage='Pool Name of Azure Batch.')]
        [string]$BatchPoolName,
        [parameter(Mandatory=$true, HelpMessage='Location of Azure Batch.')]
        [string]$BatchLocation	
    )

    process
    {
        $ErrorActionPreference = "Stop"

        $resourceGroup = $EnvironmentName
        $deploymentPrefix = $EnvironmentName.Replace("-","")
        $storageAccountName = $EnvironmentName.Replace("-","")
        # Breaking change in latest Azure Powershell version where Key1 is replaced with Value[0]
        $storageAccountKey = (Get-AzureRmStorageAccountKey -ResourceGroupName $resourceGroup -Name $storageAccountName).Value[0]

        $azureStorageLinkedServiceTemplateFile = "AzureStorageLinkedServiceTemplate.json"
        $azureBatchLinkedServiceTemplateFile = "AzureBatchLinkedServiceTemplate.json"

        $azureDataFactoryName = $deploymentPrefix + "datafactory"
        $azureStorageLinkedServiceName = $deploymentPrefix + "storagelinkedservice"
        $azureBatchLinkedServiceName = $deploymentPrefix + "batchlinkedservice"

        $azureStorageLinkedServiceFileName = "storagelinkedservice.json"
        $azureBatchLinkedServiceFileName = "batchlinkedservice.json"

        $BatchUri = "https://" + $BatchLocation + ".batch.azure.com"

        # Create Data Factory
        New-AzureRmDataFactory -ResourceGroupName $resourceGroup -Name $azureDataFactoryName -Location $Location -Force

        # Create directory for deployment related filed
        $directory = ".\" + $deploymentPrefix
        if(!(Test-Path -Path $directory))
        {
            New-Item -Path $directory -ItemType directory
        }

        # Create AzureStorageLinkedService
        $azureStorageLinkedServiceFilePath = $directory + "\" + $azureStorageLinkedServiceFileName
        $json = Get-Content $azureStorageLinkedServiceTemplateFile -Raw | ConvertFrom-Json 
        $json."name" = $json."name" -replace "AzureStorageLinkedService" , $azureStorageLinkedServiceName
        $json."properties".typeProperties.connectionString = $json."properties".typeProperties.connectionString -replace "<accountname>" , $storageAccountName 
        $json."properties".typeProperties.connectionString = $json."properties".typeProperties.connectionString -replace "<accountkey>" , $storageAccountKey
        $json | ConvertTo-Json -Depth 10 | Out-File -FilePath $azureStorageLinkedServiceFilePath

        New-AzureRmDataFactoryLinkedService -ResourceGroupName $resourceGroup -DataFactoryName $azureDataFactoryName -Name $azureStorageLinkedServiceName -File ($directory + "\" + $azureStorageLinkedServiceFileName) -Force

        # Create AzureBatchLinkedService
        $azureBatchLinkedServiceFilePath = $directory + "\" + $azureBatchLinkedServiceFileName
        $azureBatchContext = Get-AzureRmBatchAccountKeys -AccountName $BatchAccountName

        $json = Get-Content $azureBatchLinkedServiceTemplateFile -Raw | ConvertFrom-Json 
        $json."name" = $json."name" -replace "AzureBatchLinkedService" , $azureBatchLinkedServiceName
        $json."properties".typeProperties.accountName = $json."properties".typeProperties.accountName -replace "<Azure Batch account name>" , $BatchAccountName 
        $json."properties".typeProperties.accessKey = $json."properties".typeProperties.accessKey -replace "<Azure Batch account key>" , $azureBatchContext.PrimaryAccountKey 
        $json."properties".typeProperties.poolName = $json."properties".typeProperties.poolName -replace "<Azure Batch pool name>" , $BatchPoolName 
        $json."properties".typeProperties.batchUri = $json."properties".typeProperties.batchUri -replace "<Azure Batch uri>" , $BatchUri 
        $json."properties".typeProperties.linkedServiceName = $json."properties".typeProperties.linkedServiceName -replace "<Specify associated storage linked service reference here>" , $azureStorageLinkedServiceName 
        $json | ConvertTo-Json -Depth 10 | Out-File -FilePath $azureBatchLinkedServiceFilePath

        New-AzureRmDataFactoryLinkedService -ResourceGroupName $resourceGroup -DataFactoryName $azureDataFactoryName -Name $azureBatchLinkedServiceName -File $azureBatchLinkedServiceFilePath -Force

        # Create Datasets
        $relativePath = Get-Item "..\datasets" | Resolve-Path -Relative
        foreach($file in Get-ChildItem $relativePath)
        {
            $datasetPath = $directory + "\" + $file
            $json = Get-Content $file.FullName -Raw | ConvertFrom-Json 
            $json."properties".linkedServiceName = $json."properties".linkedServiceName -replace "placeholder-service-name" , $azureStorageLinkedServiceName 
            $json | ConvertTo-Json -Depth 10 | Out-File -FilePath $datasetPath
            
            New-AzureRmDataFactoryDataset -ResourceGroupName $resourceGroup -DataFactoryName $azureDataFactoryName -Name $file.BaseName -File $datasetPath -Force
        }
    }
}