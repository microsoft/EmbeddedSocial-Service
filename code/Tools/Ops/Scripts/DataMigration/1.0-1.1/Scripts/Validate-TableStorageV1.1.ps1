#
# Validate-TableStorageV1.1.ps1
#
# This script validates the tables from the Azure storage account of specified Environment for V 1.1 schema & data changes. 
# This script requires Azure PowerShell version 2.1 or higher with Azure subscription loaded.
#
# Example Usage
# Validate-TableStorageV1.1 -Name "sp-dev-test1"
#

function Validate-TableStorageV1.1
{
    <#
    .NOTES
       Name: Validate-TableStorageV1.1.ps1
       Requires: Azure PowerShell version 2.1 or higher with Azure subscription loaded.
    .SYNOPSIS
       Validates the tables from the Azure storage account of specified Environment for V 1.1 schema & data changes.
    .DESCRIPTION
       This script validates the tables from the Azure storage account of specified Environment for V 1.1 schema & data changes.        
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

        $resourceGroup = $EnvironmentName
        $storageAccountName = $EnvironmentName.Replace("-","")
        $storageAccountContext = (Get-AzureRmStorageAccount -ResourceGroupName $resourceGroup -Name $storageAccountName).Context

        $arrayProdStorageAccounts = "spapi", "spmobisys", "spppe", "spprodbeihai", "spppebeihai", "spdevbeihai"
        if($arrayProdStorageAccounts -contains $storageAccountName)
        {
            $confirmation = Read-Host "The account $storageAccountName is a production storage account. Are you sure you want to proceed? [y/n]"
            if ($confirmation -ne 'y') 
            {
                exit;
            }
        }

        # Get the table object from the specified context and table name
        function GetTable($context, $tableName)
        {   
            $azureStorageTable = Get-AzureStorageTable $tableName -Context $context -ErrorAction Ignore
            $azureStorageTable
        }

        # Get the count of rows with the specified filter string
        function GetRowKeyCount($table, $filterString)
        {
            # Create a table query.
            $query = New-Object Microsoft.WindowsAzure.Storage.Table.TableQuery
            $query.FilterString = $filterString
            # Execute the query.
            $entities = $table.CloudTable.ExecuteQuery($query)			
            ($entities | measure).Count
        }

        # Get the count of rows with the specified filter string and containing the specified column
        function GetTableColumnCount($table, $filterString, $columnName)
        {
            # Create a table query.
            $query = New-Object Microsoft.WindowsAzure.Storage.Table.TableQuery
            $query.FilterString = $filterString
            # Execute the query.
            $entities = $table.CloudTable.ExecuteQuery($query)			
            $count = 0
            foreach ($entity in $entities) 
            { 
                foreach ($property in $entity.Properties) 
                { 	
                    # Check if the properties contain the specified column 		
                    if ($property.Keys.Contains($columnName))
                    {
                        $count++
                    }	
                }
            }

            return $count
        }

        Write-Host "Checking added tables"
        $userFollowersTable = GetTable $storageAccountContext 'UserFollowers'
        $userFollowingTable = GetTable $storageAccountContext 'UserFollowing'
        $topicFollowersTable = GetTable $storageAccountContext 'TopicFollowers'
        $topicFollowingTable = GetTable $storageAccountContext 'TopicFollowing'
        if ($userFollowersTable) { Write-Host "UserFollowers table is found" -ForegroundColor Green} else {Write-Host "UserFollowers table is not found" -ForegroundColor Yellow}
        if ($userFollowingTable) { Write-Host "UserFollowing table is found" -ForegroundColor Green} else {Write-Host "UserFollowing table is not found" -ForegroundColor Yellow}
        if ($topicFollowersTable) { Write-Host "TopicFollowers table is found" -ForegroundColor Green} else {Write-Host "TopicFollowers table is not found" -ForegroundColor Yellow}
        if ($topicFollowingTable) { Write-Host "TopicFollowing table is found" -ForegroundColor Green} else {Write-Host "TopicFollowing table is not found" -ForegroundColor Yellow}

        Write-Host "Checking removed tables"
        $followersTable = GetTable $storageAccountContext 'Followers'
        $followingTable = GetTable $storageAccountContext 'Following'
        if (!$followersTable) { Write-Host "Followers table is not found" -ForegroundColor Green} else {Write-Host "Followers table is found" -ForegroundColor Yellow}
        if (!$followingTable) { Write-Host "Following table is not found" -ForegroundColor Green} else {Write-Host "Following table is found" -ForegroundColor Yellow}

        $appsTableReference = GetTable $storageAccountContext 'Apps'
        $usersTableReference = GetTable $storageAccountContext 'Users'
        $linkedAccountsTableReference = GetTable $storageAccountContext 'LinkedAccounts'
        $serviceConfigTableReference = GetTable $storageAccountContext 'ServiceConfig'

        Write-Host "Checking added columns in Apps table"
        # Apps Table: DisableHandleValidation column is added in ProfilesObject rows
        # Condition Hack to simulate StartsWith condition on RowKey
        $filterString = "RowKey ge 'ProfilesObject' and RowKey lt 'ProfilesObjecu'" 
        $count = GetTableColumnCount $appsTableReference $filterString "DisableHandleValidation"
        if ($count -gt 0) { Write-Host "DisableHandleValidation column is found in $count records" -ForegroundColor Green} else {Write-Host "DisableHandleValidation column is found in $count records" -ForegroundColor Yellow}

        # Apps Table: ClientId column is added in IdentityCredentialsObject:AADS2S rows
        $filterString = "RowKey eq 'IdentityCredentialsObject:AADS2S'" 
        $count = GetTableColumnCount $appsTableReference $filterString "ClientId"
        if ($count -gt 0) { Write-Host "ClientId column is found in $count records" -ForegroundColor Green} else {Write-Host "ClientId column is found in $count records" -ForegroundColor Yellow}

        # Apps Table: ClientRedirectUri column is added in IdentityCredentialsObject:AADS2S rows
        $filterString = "RowKey eq 'IdentityCredentialsObject:AADS2S'" 
        $count = GetTableColumnCount $appsTableReference $filterString "ClientRedirectUri"
        if ($count -gt 0) { Write-Host "ClientRedirectUri column is found in $count records" -ForegroundColor Green} else {Write-Host "ClientRedirectUri column is found in $count records" -ForegroundColor Yellow}

        # Apps Table: ClientSecret column is added in IdentityCredentialsObject:AADS2S rows
        $filterString = "RowKey eq 'IdentityCredentialsObject:AADS2S'" 
        $count = GetTableColumnCount $appsTableReference $filterString "ClientSecret"
        if ($count -gt 0) { Write-Host "ClientSecret column is found in $count records" -ForegroundColor Green} else {Write-Host "ClientSecret column is found in $count records" -ForegroundColor Yellow}

        Write-Host "Checking removed columns from Apps table"
        # Apps Table: UseDefault column is removed from IdentityCredentialsObject:AADS2S rows
        $filterString = "RowKey ge 'IdentityCredentialsObject' and RowKey lt 'IdentityCredentialsObjecu'"
        $count = GetTableColumnCount $appsTableReference $filterString "UseDefault"
        if ($count -gt  0) { Write-Host "UseDefault column is found in $count records" -ForegroundColor Yellow} else {Write-Host "UseDefault column is found in $count records" -ForegroundColor Green}

        Write-Host "Checking updated rows in Apps table"
        # Apps Table: RowKey is updated to IdentityCredentialsObject:AADS2S
        $filterString = "RowKey eq 'IdentityCredentialsObject:AADS2S'"
        $count = GetRowKeyCount $appsTableReference $filterString
        if ($count -gt  0) { Write-Host "RowKey 'IdentityCredentialsObject:AADS2S' is found in $count records" -ForegroundColor Green} else {Write-Host "RowKey 'IdentityCredentialsObject:AADS2S' is found in $count records" -ForegroundColor Yellow}

        # Apps Table: DisableHandleValidation is updated to true for EndToEndTests Owner
        $filterString = "DisableHandleValidation eq true"
        $count = GetRowKeyCount $appsTableReference $filterString
        if ($count -gt  0) { Write-Host "DisableHandleValidation eq true is found in $count records" -ForegroundColor Green} else {Write-Host "DisableHandleValidation eq true is found in $count records" -ForegroundColor Yellow}

        # Apps Table: ClientRedirectUri is updated to 'http://localhost/beihaiclient' for EndToEndTests Owner
        $filterString = "ClientRedirectUri eq 'http://localhost/beihaiclient'"
        $count = GetRowKeyCount $appsTableReference $filterString
        if ($count -gt  0) { Write-Host "ClientRedirectUri eq 'http://localhost/beihaiclient' is found in $count records" -ForegroundColor Green} else {Write-Host "ClientRedirectUri eq 'http://localhost/beihaiclient' is found in $count records" -ForegroundColor Yellow}

        Write-Host "Checking removed rows from Apps table"
        # Apps Table: RowKey eq IdentityCredentialsObject:Beihai is removed from rows
        $filterString = "RowKey eq 'IdentityCredentialsObject:Beihai'"
        $count = GetRowKeyCount $appsTableReference $filterString
        if ($count -gt  0) { Write-Host "RowKey 'IdentityCredentialsObject:Beihai' is found in $count records" -ForegroundColor Yellow} else {Write-Host "RowKey 'IdentityCredentialsObject:Beihai' is found in $count records" -ForegroundColor Green}

        Write-Host "Checking updated rows in Users table"
        # Users Table: IdentityProviderType is updated to AADS2S 
        $filterString = "IdentityProviderType eq 'AADS2S'"
        $count = GetRowKeyCount $usersTableReference $filterString
        if ($count -gt  0) { Write-Host "IdentityProviderType eq 'AADS2S' is found in $count records" -ForegroundColor Green} else {Write-Host "IdentityProviderType eq 'AADS2S' is found in $count records" -ForegroundColor Yellow}

        Write-Host "Checking removed rows from Users table"
        # Users Table: IdentityProviderType eq Beihai is removed from rows
        $filterString = "IdentityProviderType eq 'Beihai'"
        $count = GetRowKeyCount $usersTableReference $filterString
        if ($count -gt  0) { Write-Host "IdentityProviderType eq 'Beihai' is found in $count records" -ForegroundColor Yellow} else {Write-Host "IdentityProviderType eq 'Beihai' is found in $count records" -ForegroundColor Green}

        Write-Host "Checking updated rows in LinkedAccounts table"
        # LinkedAccounts Table: PartitionKey is updated to AADS2S*
        $filterString = "(PartitionKey ge 'AADS2S') and (PartitionKey lt 'AADS2T')"
        $count = GetRowKeyCount $linkedAccountsTableReference $filterString
        if ($count -gt  0) { Write-Host "PartitionKey 'AADS2S*' is found in $count records" -ForegroundColor Green} else {Write-Host "PartitionKey 'AADS2S*' is found in $count records" -ForegroundColor Yellow}

        Write-Host "Checking removed rows from LinkedAccounts table"
        # LinkedAccounts Table: PartitionKey ge Beihai* is removed from rows
        $filterString = "(PartitionKey ge 'Beihai') and (PartitionKey lt 'Beihaj')"
        $count = GetRowKeyCount $linkedAccountsTableReference $filterString
        if ($count -gt  0) { Write-Host "PartitionKey 'Beihai*' is found in $count records" -ForegroundColor Yellow} else {Write-Host "PartitionKey ge 'Beihai*' is found in $count records" -ForegroundColor Green}

        Write-Host "Checking updated rows in ServiceConfig table"
        # ServiceConfig Table: Version is updated to 1.1
        $filterString = "(Version eq '1.1')"
        $count = GetRowKeyCount $serviceConfigTableReference $filterString
        if ($count -gt  0) { Write-Host "Version eq '1.1' is found in $count records" -ForegroundColor Green} else {Write-Host "Version eq '1.1' is found in $count records" -ForegroundColor Yellow}
    }
}
