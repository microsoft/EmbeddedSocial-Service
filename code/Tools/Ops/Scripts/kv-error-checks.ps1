<#
.NOTES
	Name: kv-errror-checks.ps1
	Requires: Azure Powershell.
	Version History:
	0.1 - 11/4/2015 - Initial script finished.
	0.2 - 1/27/2016 - Added support for Azure Resource Manager
.SYNOPSIS
    Does all sorts of error checking relevant to the Azure Key Vault service
.DESCRIPTION
    This script checks parameters relevant to the Azure Key Vault service for errors.
    It checks whether the Azure subscription is loaded, whether a key vault name is present
    or accessible, or whether a secret is set in the key vault.
.PARAMETER KVName
	This optional value designates the name of the key vault to check its presence or whether 
    is accesible.
.EXAMPLE
	[PS] C:\>.\kv-error-checks
    Checks whether Azure KV cmdlets are loaded
.EXAMPLE
	[PS] C:\>.\kv-error-checks -KVName kvName 
	Lists all keys and secrets found in fromName key vault
#>
{
param(
    # Read in the optional -KVName value from the command line or object
    # piepline. Even if -KVName isn't used on the command line, the first value
    # after the script name is used for this variable
    [Parameter(Mandatory=$False,Position=0,ValueFromPipeline=$True)]
    [ValidateNotNullOrEmpty()]
    [string]$KVName
)

# Check that Azure KV cmdlets are loaded
Get-AzureRMKeyVault > $null 2>&1
if(!($?))
{
    Write-Host -ForegroundColor Red "Azure cmdlets not loaded. Perhaps you need to call 'Switch-AzureMode AzureResourceManager'. Exiting."
    EXIT -1
}

if($KVName) 
{
    # Check whether the key vault is present or accessible
    Get-AzureRMKeyVault -KVName $KVName > $null 2>&1
    if(!($?))
    {
        Write-Host -ForegroundColor Red "Azure key vault $KVName is not present or accessible. Exiting."
        EXIT -2
    }
}

# All error checking passed
EXIT 0
}
