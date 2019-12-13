<#
.NOTES
	Name: kv-key-copy.ps1
	Requires: Azure Powershell with Azure subscription loaded.
	Version History:
	0.1 - 11/3/2015 - Initial script finished.
	0.2 - 1/27/2016 - Added support for Azure Resource Manager
.SYNOPSIS
    Copies keys from source key vault to destination key vault. No secrets other
    than keys are copied.
.DESCRIPTION
    Copies keys stored in a source key vault to a destination key vault. Supports single
    key copy or all keys present in the source key vault.
    The script assumes that it has the right permissions to read from source
    key vault and write to the destination key vault. This script does not guarantee 
    that the destination key vault will contain the same keys as the source.
.PARAMETER FromKVName
	This mandatory value designates the name of the source key vault.
.PARAMETER ToKVName
	This mandatory value designates the name of the destination key vault.
.PARAMETER KeyName
	This optional value designates the name of the key to be copied from source
    to destination key vault. 
.PARAMETER All
	This optional switch tells the script to copy all keys found in the source
    key vault.
.EXAMPLE
	[PS] C:\>.\kv-key-copy -FromKVName fromName -ToKVName toName -KeyName 'signingKey'
	The destination key vault will have a signingKey whose value is idential to the
    value of the signingKey in the source vault.
.EXAMPLE
	[PS] C:\>.\kv-key-copy -FromKVName fromName -ToKVName toName -All 
	All keys from the source key vault are copied to the destination key vault
#>

param(
    # Read in the mandatory -FromKVName value from the command line or object
    # pipeline. Even if -FromKVName isn't used on the command line, the first value
    # after the script name is used for this variable
    [Parameter(Mandatory=$True,Position=0,ValueFromPipeline=$True)]
    [ValidateNotNullOrEmpty()]
    [string]$FromKVName, 
 
    # Read in the mandatory -ToKVName value from the command line or object
    # pipeline. Evenif -ToKVName isn't used on the command line, the second value
    # after the script name is used for this variable
    [Parameter(Mandatory=$True,Position=1,ValueFromPipeline=$True)]
    [ValidateNotNullOrEmpty()]
    [string]$ToKVName,

    # Read in the optional -KeyName value from the command line 
    [Parameter(Mandatory=$False)]
    [ValidateNotNullOrEmpty()]
    [string]$KeyName,

    # Read in the optional -All switch from the command line
    [Parameter(Mandatory=$False)]
    [Switch]$All
)

# Check that $KeyName or $All are present. If none present, output error and terminate
if((!$KeyName) -and (!$All))
{
    Write-Host -ForegroundColor Red "No keys passed to the script. Exiting."
    EXIT
}

# Check to see whether KV authorization is currently loaded
Get-AzureRMKeyVault > $null 2>&1
if(!($?))
{
    Write-Host -ForegroundColor Red "KV authorization not currently loaded. Exiting."
    EXIT
}

# The script uses yellow font color in its output for all material relevant to 
# source KV and green for destination KV
$srcColor = "yellow"
$dstColor = "green"

# Array storing the keyNames passed in
$keyNames = @()

#
# If $All switch turned on, go discover all key names in the source 
# key vault
#
if($All)
{
    #
    # Output source key vault keys
    # 
    write-host
    write-host Keys found in ($FromKVName): -foreground $srcColor
    foreach ($keyNameSrc in (Get-AzureKeyVaultKey -VaultName $FromKVName).Name)
    {
      write-host "  " $keyNameSrc -foreground $srcColor
      $keyNames += $keyNameSrc
    }    
    write-host
}
else
{
    # Checks that the key name passed in exists in the source
    Get-AzureKeyVaultKey -VaultName $FromKVName -Name $KeyName > $null 2>&1
    if(!($?))
    {
        Write-Host -ForegroundColor Red "Key name $keyName not present in source key vault. Exiting."
        EXIT
    }

    $keyNames += $KeyName;
}

#
# Check that none of the key names already exist in the destination key vault.
# If any do, stop.
#
foreach ($keyName in $keyNames)
{
    Get-AzureKeyVaultKey -VaultName $ToKVName -Name $keyName > $null 2>&1
    if($?)
    {
        Write-Host -ForegroundColor Red "Key name $keyName already present in the destination key vault. Exiting."
        EXIT
    }
}

#
# Start copying
# 

foreach ($keyName in $keyNames)
{
    Write-Host -ForegroundColor $srcColor "Starting to copy key name: " + $keyName

    $backupFileName = ".\" + $keyName.ToString() + ".backup"
    Backup-AzureKeyVaultKey -VaultName $FromKVName -Name $keyName -OutputFile $backupFileName
    Restore-AzureKeyVaultKey -VaultName $ToKVName -InputFile $backupFileName
    del $backupFileName

    Write-Host -ForegroundColor $dstColor "Finished copy of key name: " + $keyName
}
