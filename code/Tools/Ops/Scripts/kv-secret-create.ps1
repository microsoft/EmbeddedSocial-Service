<#
.NOTES
	Name: kv-secret-create.ps1
	Requires: Azure Powershell with Azure subscription loaded.
	Version History:
        0.1 - 11/4/2015 - Initial script finished.
        0.2 - 1/27/2016 - Added support for Azure Resource Manager
        0.3 - 9/15/2016 - Make it work with string rather than SecureString
.SYNOPSIS
    Creates secret in specified key vault and loads it with passed in value.
.DESCRIPTION
    Creates a secret in a specified key vault under current Azure account. The script
    also takes the value of the secret as a parameter; the secret in the key vault is set
    to this passed in value.
.PARAMETER KVName
    This mandatory value designates the name of the key vault where the secret must be created.
.PARAMETER SecretName
    This mandatory value designates the name of the secret to be created.
.PARAMETER SecretValue
    This mandatory value designates the value of the secret to be created. This parameter must be
    a PowerShell SecureString.
.EXAMPLE
    [PS] C:\>.\kv-ls -KVName kvName -SecretName secretName -SecretValue secretValue
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
    [string]$SecretName,

    # Read in the mandatory -SecretValue value from the command line or object
    # piepline. Even if -SecretValue isn't used on the command line, the third value
    # after the script name is used for this variable
    [Parameter(Mandatory=$True,Position=2,ValueFromPipeline=$True)]
    [ValidateNotNullOrEmpty()]
    [string]$SecretValue
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

# Check that secret name does not exist in the key vault
$secret = Get-AzureKeyVaultSecret -VaultName $KVName -Name $SecretName
if (($secret | Measure-Object).Count -gt 0)
{
    Write-Host -ForegroundColor Red "Secret name $SecretName already exists in key vault $KVName. Exiting."
    EXIT
}

# Convert secret value to secure string
$secureSecretValue = ConvertTo-SecureString -String $SecretValue -AsPlainText -Force

# Provision the secure string
Set-AzureKeyVaultSecret -VaultName $KVName -Name $SecretName  -SecretValue $secureSecretValue
