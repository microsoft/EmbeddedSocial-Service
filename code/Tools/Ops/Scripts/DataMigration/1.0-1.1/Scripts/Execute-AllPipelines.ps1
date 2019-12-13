#
# Execute-AllPipelines.ps1
#
# This script executes all the pipelines for 1.0->1.1 store migration using the specified Environment name
# The environment name without hyphens is used to identify the names of the resources/assets created as part of data factory 
#
# Example Usage
# Execute-AllPipelines -Name "sp-dev-test1"
#

function Execute-AllPipelines
{
    <#
    .NOTES
       Name: Execute-AllPipelines.ps1
       Requires: Azure PowerShell version 2.1 or higher with Azure subscription loaded.
    .SYNOPSIS
       Executes all the pipelines for 1.0->1.1 store migration using the specified Environment name.
    .DESCRIPTION
       This script executes all the pipelines for 1.0->1.1 store migration using the specified Environment name
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

        Execute-ProcessAppsTable -Name $EnvironmentName
         
        Execute-RenameFollowersTable -Name $EnvironmentName

        Execute-RenameFollowingTable -Name $EnvironmentName

        Execute-UpdateLinkedAccountsTable -Name $EnvironmentName

        Execute-UpdateUsersTable -Name $EnvironmentName
    }
}
