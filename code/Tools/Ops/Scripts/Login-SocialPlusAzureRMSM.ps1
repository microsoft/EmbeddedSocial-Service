function Login-SocialPlusAzureRMSM {
    <#
    .NOTES
    	Name: Login-SocialPlusAzureRMSM.ps1
    	Requires: Azure Powershell version 2.1 or higher.
    .SYNOPSIS
        Logs in a user to Azure for both the resource manager (RM) and the service manager (SM).
    .DESCRIPTION
        Given an Azure subscription id, this function logs you in and
        selects the passed-in subscription. This function is used as a preamble
        to many Azure scripts we have to make sure the user is logged in properly.
    .PARAMETER Id
        This optional value designates the id of the subscription to log
        in. If no id passed, the script automatically uses id:
        "f52057db-bff8-493e-84ab-a510fb5ab8f4" which is SocialPlus's id.
    .EXAMPLE
    	[PS] C:\>Login-SocialPlusAzureRMSM -Id f52057db-bff8-493e-84ab-a510fb5ab8f4    	
    #>

    param (
        [Parameter(
            Mandatory=$false,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='Azure subscription id'
        )]
        [Alias("Id")]
        [string] $socPlusId = "f52057db-bff8-493e-84ab-a510fb5ab8f4"
    )

    begin {
    }

    process {
        Write-Verbose "Looking up account information..."

        $sub = Get-AzureSubscription -SubscriptionId $socPlusId
        $sub | Select-AzureSubscription > $null

        if (($sub | Measure-Object).Count -eq 0) {
           Write-Host "Not logged in to the Azure Service Manager"
           Add-AzureAccount

           # after Add-AzureAccount, retry
           $sub = Get-AzureSubscription -SubscriptionId $socPlusId
           $sub | Select-AzureSubscription > $null
           Write-Verbose "Using Azure subscription $($sub.SubscriptionId)"
        } else {
           # if we reach here, the user is already logged in
           Write-Verbose "Using Azure subscription $($sub.SubscriptionId)"
        }

        try {
            $rmSub = Get-AzureRmSubscription -SubscriptionId $socPlusId
        }
        # if Get-AzureRmSubscription generates an invalid operation
        #   exception, then the user needs to login
        catch [System.Management.Automation.PSInvalidOperationException] {
            Write-Host "User not logged in to the Azure Resource Manager"
            Login-AzureRmAccount

            # after Login-AzureRmAccount, retry
            $rmSub = Get-AzureRmSubscription -SubscriptionId $socPlusId
        }

        $rmSub | Select-AzureRmSubscription > $null

        if (($rmSub | Measure-Object).Count -eq 0) {
           throw "Not logged in to the Azure Resource Manager."
        } else {
           Write-Verbose "Using Azure RM subscription $($rmSub.SubscriptionId)"
        }
    }

    end {
    }
}
