#
# Execute-RenameFollowingTable.ps1
#
# This script executes RenameFollowingTable pipeline using the specified Environment name
# The environment name without hyphens is used to identify the names of the resources/assets created as part of data factory 
#
# Example Usage
# Execute-RenameFollowingTable -Name "sp-dev-test1"
#

function Execute-RenameFollowingTable
{
    <#
    .NOTES
       Name: Execute-RenameFollowingTable.ps1
       Requires: Azure PowerShell version 2.1 or higher with Azure subscription loaded.
    .SYNOPSIS
       Executes RenameFollowingTable pipeline using the specified Environment name.
    .DESCRIPTION
       This script executes RenameFollowingTable pipeline using the specified Environment name. 
       The environment name without hyphens is used to identify the names of the resources/assets created as part of data factory.	   
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
        $deploymentPrefix = $EnvironmentName.Replace("-","")

        $azureDataFactoryName = $deploymentPrefix + "datafactory"
        $azureStorageLinkedServiceName = $deploymentPrefix + "storagelinkedservice"
        $azureBatchLinkedServiceName = $deploymentPrefix + "batchlinkedservice"

        # Get directory for deployment related files
        $directory = ".\" + $deploymentPrefix
        if(!(Test-Path -Path $directory))
        {
            New-Item -ItemType directory -Path $directory
        }

        # Create Pipeline
        $relativePath = Get-Item "..\pipelines" | Resolve-Path -Relative
        $file = Get-Item $relativePath"\RenameFollowingTable.json"
        $pipelineName = $deploymentPrefix + $file.BaseName
        $pipelineFilePath = $directory + "\" + $file.Name
        Write-host $pipelineName
        Write-host $pipelineFilePath
        $json = Get-Content $file.FullName -Raw | ConvertFrom-Json 
        $json."properties".activities[0].linkedServiceName = $json."properties".activities[0].linkedServiceName -replace "placeholder-batch-name" , $azureBatchLinkedServiceName 
        $json."properties".activities[0].typeProperties.packageLinkedService = $json."properties".activities[0].typeProperties.packageLinkedService -replace "placeholder-storage-name" , $azureStorageLinkedServiceName 
        $json."properties".activities[1].linkedServiceName = $json."properties".activities[1].linkedServiceName -replace "placeholder-batch-name" , $azureBatchLinkedServiceName 
        $json."properties".activities[1].typeProperties.packageLinkedService = $json."properties".activities[1].typeProperties.packageLinkedService -replace "placeholder-storage-name" , $azureStorageLinkedServiceName 
        $json | ConvertTo-Json -Depth 10 | Out-File -FilePath $pipelineFilePath
                
        New-AzureRmDataFactoryPipeline -ResourceGroupName $resourceGroup -DataFactoryName $azureDataFactoryName -Name $pipelineName -File $pipelineFilePath -Force
    }
}