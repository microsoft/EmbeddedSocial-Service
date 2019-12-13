This directory contains Azure PowerShell scripts for managing Azure Key
Vaults. To learn more about each script, type "help" followed by the script
name.

Example:

help .\kv-key-copy.ps1

============================

Before running any scripts, make sure you have Windows PowerShell version
1.10 or higher. Visit this site if you don't:
https://azure.microsoft.com/en-us/documentation/articles/powershell-install-configure/

Then, make sure to
# Import AzureRM modules for the given version manifest in the AzureRM module
Import-AzureRM

# Import Azure Service Management module
Import-Module Azure

Then, you need to setup the Azure RM:

Login for Azure Resource Manager:
> Login-AzureRmAccount

List the available subscriptions
> Get-AzureRmSubscription

Select the SocialPlus subscription:
> Get-AzureRmSubscription –SubscriptionId f52057db-bff8-493e-84ab-a510fb5ab8f4 | Select-AzureRmSubscription
