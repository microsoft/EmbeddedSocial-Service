<#
.NOTES
	Name: kv-ls.ps1
	Requires: Azure Powershell with Azure subscription loaded.
	Version History:
	0.1 - 11/4/2015 - Initial script finished.
	0.2 - 1/27/2016 - Added support for Azure Resource Manager
.SYNOPSIS
    Lists keys and secrets in a key vault.
.DESCRIPTION
    Lists all keys and secrets found in a key vault under the current
    Azure account. Supports single key vault or all key vaults found under the
    current account. No additional error checks are performed.
.PARAMETER KVName
	This optional value designates the name of the key vault whose keys are listed.
    If parameter not set, all key vaults are listed.
.EXAMPLE
	[PS] C:\>.\kv-ls -KVName kvName 
	Lists all keys and secrets found in fromName key vault
.EXAMPLE
	[PS] C:\>.\kv-ls
	Enumerates all key vaults under current Azure accounts and lists all their keys 
    and secrets.
#>

param(
    # Read in the optional -KVName value from the command line or object
    # pipeline. Even if -KVName isn't used on the command line, the first value
    # after the script name is used for this variable
    [Parameter(Mandatory=$False,Position=0,ValueFromPipeline=$True)]
    [ValidateNotNullOrEmpty()]
    [string]$KVName
)

# Array storing the key vault names passed in
$kvNames = @()

# Check that Azure cmdlets are loaded
Get-AzureRMKeyVault > $null 2>&1
if(!($?))
{
    Write-Host -ForegroundColor Red "Azure cmdlets not loaded. Perhaps you need to call 'Switch-AzureMode AzureResourceManager'. Exiting."
    EXIT
}

#
# If key vault name passed not passed in, go discover all key vaults under current account
#
if(!$KVName)
{
    foreach ($kv in (Get-AzureRMKeyVault).VaultName)
    {
        $kvNames += $kv
    }    
}
else
{
    # Check that the key vault name passed in exists
    Get-AzureRMKeyVault -VaultName $KVName > $null 2>&1
    if(!($?))
    {
        Write-Host -ForegroundColor Red "Key vault $KVName not present or not accessible. Exiting."
        EXIT
    }

    $kvNames += $KVName
}

foreach ($kv in $kvNames)
{
  write-host =============================
  write-host Vault Name: $kv -foreground green
  write-host =============================

  foreach($key in (Get-AzureKeyVaultKey -VaultName $kv).Name)
  {
    write-host Key: $key -foreground yellow
  }
  foreach($secret in (Get-AzureKeyVaultSecret -VaultName $kv).Name)
  {
    $secretValue = (Get-AzureKeyVaultSecret -VaultName $kv -Name $secret).SecretValueText
    $versionValue = (Get-AzureKeyVaultSecret -VaultName $kv -Name $secret).Version
    write-host "Secret: ${secret}; Value: ${secretValue}; Version: ${versionValue}" -foreground yellow
  }
  write-host 
}
