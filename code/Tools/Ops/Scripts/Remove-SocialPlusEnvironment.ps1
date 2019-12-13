function Remove-SocialPlusEnvironment {
    <#
    .NOTES
        Name: Remove-SocialPlusEnvironment.ps1
        Requires: Azure Powershell version 2.1 or higher.
    .SYNOPSIS
        Removes a SocialPlus Azure instance.
    .DESCRIPTION
        Implements all logic needed to delete a SocialPlus Azure instance.
        Assumes that the user is already logged in the Azure RM.
    .PARAMETER Name
        Name of the SocialPlus environment to remove.
    .EXAMPLE
    	[PS] C:\>Remove-SocialPlusEnvironment -Name sp-dev-stefan -Verbose
    #>

    param (
        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='Environment name suffix'
        )]
        [Alias("Name")]
        [string] $EnvironmentName
    )

    begin {
    }

    process {
        Write-Host "Deleting all resources for account $EnvironmentName..."

        $rg = $EnvironmentName

        $count = (Get-AzureRmResourceGroup -Name $rg | Measure-Object).Count

        Write-Verbose "Get resource group returned count = $count."

        # check that the resource group exists before we try to delete it
        if ($count -eq 1) {
            # Implement a simple fuse by asking the user to press 'y' to continue
            $message  = 'This script deletes all Azure resources of a SocialPlus environment.'
            $question = 'Are you sure you want to proceed?'

            $choices = New-Object Collections.ObjectModel.Collection[Management.Automation.Host.ChoiceDescription]
            $choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&Yes'))
            $choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&No'))

            $decision = $Host.UI.PromptForChoice($message, $question, $choices, 1)
            if ($decision -ne 0) {
              throw 'cancelled'
            }

            Write-Verbose "Removing resource group $rg"
            Remove-AzureRmResourceGroup -Name $rg -Force
        }
    }

    end {
        Write-Verbose "Finished."
    }
}
