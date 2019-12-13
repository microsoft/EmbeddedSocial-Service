<#
.NOTES
	Name: kv-secret-delete.ps1
	Requires: Azure Powershell with Azure subscription loaded.
	Version History:
	0.1 - 11/4/2015 - Initial script finished.
	0.2 - 1/27/2016 - Added support for Azure Resource Manager
.SYNOPSIS
    Deletes secret in specified key vault.
.DESCRIPTION
    Deletes a secret in a specified key vault under current Azure account. 
.PARAMETER KVName
	This mandatory value designates the name of the key vault where the secret must be deleted.
.PARAMETER SecretName
	This mandatory value designates the name of the secret to be deleted.
.EXAMPLE
	[PS] C:\>.\kv-secret-delete -KVName kvName -SecretName secretName
    TBD.
#>
param(
    # Read in the mandatory -KVName value from the command line or object
    # piepline. Even if -KVName isn't used on the command line, the first value
    # after the script name is used for this variable
    [Parameter(Mandatory=$True,Position=0,ValueFromPipeline=$True)]
    [ValidateNotNullOrEmpty()]
    [string]$KVName,
 
    # Read in the mandatory -SecretName value from the command line or object
    # piepline. Even if -SecretName isn't used on the command line, the second value
    # after the script name is used for this variable
    [Parameter(Mandatory=$True,Position=1,ValueFromPipeline=$True)]
    [ValidateNotNullOrEmpty()]
    [string]$SecretName
)

# Check that Azure cmdlets are loaded
$allVaults = Get-AzureRMKeyVault
if (($allVaults | Measure-Object).Count -eq 0)
{
    Write-Host -ForegroundColor Red "Azure cmdlets not loaded. Perhaps you need to call 'Switch-AzureMode AzureResourceManager'. Exiting."
    EXIT
}

# Check that Azure key vault exists and is accessible
$vault = Get-AzureRMKeyVault -VaultName $KVName
if (($vault | Measure-Object).Count -eq 0)
{
    Write-Host -ForegroundColor Red "Azure key vault $KVName is not present or not accessible. Exiting."
    EXIT
}

# Check that secret name exists in the key vault
$secret = Get-AzureKeyVaultSecret -VaultName $KVName -Name $SecretName
if (($secret | Measure-Object).Count -eq 0)
{
    Write-Host -ForegroundColor Red "Secret name $SecretName is not present in key vault $KVName. Exiting."
    EXIT
}

Remove-AzureKeyVaultSecret -VaultName $KVName -Name $SecretName
