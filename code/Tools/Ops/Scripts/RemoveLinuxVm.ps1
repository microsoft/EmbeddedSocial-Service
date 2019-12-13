#
# RemoveLinuxVm.ps1
#
# WARNING: this script removes a VM. Use with CARE !!!!
#
# Usage: RemoveLinuxVm.ps1 -name <vmname>

param(
    [Parameter(Mandatory=$True)]
    [string]$name
)

function SafeDelete([string]$target)
{
   if (Test-Path "$target") {
      Remove-Item "$target"
   } else {
      write-host "Cannot delete $target"
   }
}

$vmname = $name
$subscription = "Microsoft Azure Internal Consumption"
$storageacct = "spprod1"
$key_dir = "d:\home\alecw\research\social-plus\git\docs\DeploymentConfig\vm-keys"

# select azure subscription and storage account
Select-AzureSubscription -SubscriptionName $subscription
Set-AzureSubscription -SubscriptionName $subscription -CurrentStorageAccountName $storageacct

# delete the VM and the VHD
Remove-AzureVM -Name $vmname -ServiceName $vmname -DeleteVHD

# remove the azure cloud service
Remove-AzureService -ServiceName $vmname -Force

# delete the ssh keys
SafeDelete "$key_dir\$vmname.pub"
SafeDelete "$key_dir\$vmname.ppk"
SafeDelete "$key_dir\$vmname.pem"
SafeDelete "$key_dir\$vmname.key"



