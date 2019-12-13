#
# Provision-DataFactoryActivities.ps1
#
# This script provisions the datafactory activities required for data migration in a blob storage location of Azure storage account of specified environment.
# The Azure storage account is same as the account used by Data factory storage LinkedService
# 1. Create storage container if not exists
# 2. Upload activities file as blob
# Example Usage
# Provision-DataFactoryActivities -Name "sp-dev-test1" -ContainerName "datafactoryactivities" -ActivitiesFilePath "D:\SocialPlus\MyRepo\code\Server\DataFactory\bin\Release\SocialPlus.Server.DataFactory.zip"
#

function Provision-DataFactoryActivities
{
    <#
    .NOTES
       Name: Provision-DataFactoryActivities.ps1
       Requires: Azure PowerShell version 2.1 or higher with Azure subscription loaded.
    .SYNOPSIS
       Provisions the datafactory activities required for data migration in a blob storage location of storage account of specified environment.
    .DESCRIPTION
       This script provisions the datafactory activities required for data migration in a blob storage location of Azure storage account of specified environment.
       The Azure storage account is same as the account used by Data factory storage LinkedService.
    .PARAMETER Name
       Name of the Environment.    
    .PARAMETER ContainerName
       Name of the azure storage container.
     .PARAMETER ActivitiesFilePath
       Path to datafactory activities zip file.	 
    #>
    
    param(
        [parameter(Mandatory=$true, HelpMessage='Name of the Environment.')]
        [Alias("Name")]
        [string]$EnvironmentName,
        [parameter(Mandatory=$true, HelpMessage='Name of the azure storage container.')]
        [string]$ContainerName,
        [parameter(Mandatory=$true, HelpMessage='Path to datafactory activities zip file.')]
        [string]$ActivitiesFilePath	
    )

    process
    {
        $ErrorActionPreference = "Stop"

        $resourceGroup = $EnvironmentName
        $storageAccountName = $EnvironmentName.Replace("-","")
        $storageAccountContext = (Get-AzureRmStorageAccount -ResourceGroupName $resourceGroup -Name $storageAccountName).Context

        $container = Get-AzureStorageContainer -Context $storageAccountContext | Where-Object {$_.Name -eq $ContainerName }
        if (!$container)
        {
           Write-Host "Storage Container not found! Creating it...."
           New-AzureStorageContainer -Name $ContainerName -Context $storageAccountContext
        }

        Set-AzureStorageBlobContent -Container $ContainerName -File $ActivitiesFilePath -Context $storageAccountContext -Force
    }
}
