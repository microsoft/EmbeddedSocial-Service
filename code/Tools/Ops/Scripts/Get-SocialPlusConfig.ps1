function Get-SocialPlusConfig {
    <#
    .NOTES
    	Name: Get-SocialPlusConfig.ps1
    	Requires: Azure Powershell with Azure subscription loaded
    .SYNOPSIS
        Gets the configuration data for SocialPlus Azure instance.
    .DESCRIPTION
        Gets the Azure storage and blob storage keys, 
        Redis connection strings, the ServiceBus key, the push notifications key, and the
        application insights instrumentation keys.
    .PARAMETER Name
        This mandatory value designated the name of the environment whose
        config should be provisioned into the KeyVault.
    .PARAMETER Provision
        This is a switch. If turned on, the config should be provisioned into the KeyVault.
    .PARAMETER Production
        This is a switch which indicates if we are getting the configuration for a production environment.
    .PARAMETER RedisDnsName
        For dev environments, we cannot automatically figure out the DNS name for Redis.  Instead, we require
        the user to specify this name.
    .EXAMPLE
    	[PS] C:\>Get-SocialPlusConfig -Name sp-dev-restapi -Verbose
    	Get configuration settings for sp-dev-restapi SocialPlus environment
    	[PS] C:\>Get-SocialPlusConfig -Name sp-dev-restapi -Verbose -Provision
    	Provision configuration settings for sp-dev-restapi SocialPlus environment into KV
    #>
	
    param (
        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='Environment Name'
        )]
        [Alias("Name")]
        [string] $EnvironmentName,

        [Parameter(
             Mandatory=$false
        )]
        [switch] $Provision,

        [Parameter(
             Mandatory=$false
        )]
        [switch] $Production,

        [Parameter(
             Mandatory=$false,
             HelpMessage='Dns Name of the Redis VM.  This value is ignored when the -Production switch is enabled. For example, try splusdev-redis.cloudapp.net'
        )]
        [string] $RedisDnsName           
    )

    begin {
    }

    process {
        $ResourceGroup = $EnvironmentName
        $bearerTokenName = "bearerTokenSigningKey"
        
        # Beihai uses nic suffix. Restapi does not.
        #$redisDnsName = (Get-AzureRmPublicIpAddress -ResourceGroupName $ResourceGroup -Name "$EnvironmentName").DnsSettings.Fqdn
        if ($Production) {
            $RedisDnsName = (Get-AzureRmPublicIpAddress -ResourceGroupName $ResourceGroup -Name "$EnvironmentName-nic").DnsSettings.Fqdn
        } else {
            if (!($RedisDnsName)) {
                Write-Output "For dev environments, you must specify the DNS name of the Redis VM using the -RedisDnsName parameter"
                Return
            }
        }
        
        $storageName = $EnvironmentName.Replace("-","")
        $blobName = "${storageName}blob"
        $pushName  = "push-$EnvironmentName"

        $storageKey = (Get-AzureRmStorageAccountKey -ResourceGroupName $ResourceGroup -Name $storageName)[0].Value
        $storageKey = "DefaultEndpointsProtocol=https;AccountName=${storageName};AccountKey=${storageKey}"
        if($Provision) {
            $storageKey = Provision-KV -VaultName $EnvironmentName -Name "AzureStorageConnectionString" -Value $storageKey
            $storageKey = "kv:${storageKey}"
        }
        Write-Output "<Setting name=`"AzureStorageConnectionString`" value=`"${storageKey}`" />"

        $blobStorageKey = (Get-AzureStorageKey -StorageAccountName $blobName).Primary
        $blobStorageKey = "DefaultEndpointsProtocol=https;AccountName=${blobName};AccountKey=${blobStorageKey}"
        if($Provision) {
            $blobStorageKey = Provision-KV -VaultName $EnvironmentName -Name "AzureBlobStorageConnectionString" -Value $blobStorageKey
            $blobStorageKey = "kv:${blobStorageKey}"
        }
        Write-Output "<Setting name=`"AzureBlobStorageConnectionString`" value=`"${blobStorageKey}`" />"
        Write-Output "<Setting name=`"Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString`" value=`"${blobStorageKey}`" />"

        # redis
        $redisPersistConf = "redis\centos\dev\redis-persistent-${storageName}.conf"
        $results = Get-Content $redisPersistConf | Select-String -Pattern '^requirepass'
        $items = ($results)[0].Line.Split(' ')
        $pw = $items[1]
        $results = Get-Content $redisPersistConf | Select-String -Pattern '^port'
        $items = ($results)[0].Line.Split(' ')
        $portnum = $items[1]
        $value = "${redisDnsName}:${portnum},abortConnect=false,password=${pw}"
        if($Provision) {
            $value = Provision-KV -VaultName $EnvironmentName -Name "SocPlus-Persistent-RedisConnectionString" -Value $value
            $value = "kv:${value}"
        }
        Write-Output "<Setting name=`"SocPlus-Persistent-RedisConnectionString`" value=`"${value}`" />"

        $redisVolatileConf = "redis\centos\dev\redis-volatile-${storageName}.conf"
        $results = Get-Content $redisVolatileConf | Select-String -Pattern '^requirepass'
        $items = ($results)[0].Line.Split(' ')
        $pw = $items[1]
        $results = Get-Content $redisVolatileConf | Select-String -Pattern '^port'
        $items = ($results)[0].Line.Split(' ')
        $portnum = $items[1]
        $value = "${redisDnsName}:${portnum},abortConnect=false,password=${pw}"
        if($Provision) {
            $value = Provision-KV -VaultName $EnvironmentName -Name "SocPlus-Volatile-RedisConnectionString" -Value $value
            $value = "kv:${value}"
        }
        Write-Output "<Setting name=`"SocPlus-Volatile-RedisConnectionString`" value=`"${value}`" />"

        # Some legacy SocialPlus deployments (e.g. Restapi) use the -premium suffix.  This is obsolete.
        # $serviceConnStr = (Get-AzureSbAuthorizationRule -Namespace "${name}-premium").ConnectionString
        $serviceConnStr = (Get-AzureSbAuthorizationRule -Namespace $EnvironmentName).ConnectionString
        if($Provision) {
            $serviceConnStr = Provision-KV -VaultName $EnvironmentName -Name "ServiceBusConnectionString" -Value $serviceConnStr
            $serviceConnStr = "kv:${serviceConnStr}"
        }
        Write-Output "<Setting name=`"ServiceBusConnectionString`" value=`"${serviceConnStr}`" />"

        $pushConnStr = (Get-AzureSbAuthorizationRule -Namespace $pushName).ConnectionString
        if($Provision) {
            $pushConnStr = Provision-KV -VaultName $EnvironmentName -Name "PushNotificationsConnectionString" -Value $pushConnStr
            $pushConnStr = "kv:${pushConnStr}"
        }
        Write-Output "<Setting name=`"PushNotificationsConnectionString`" value=`"${pushConnStr}`" />"

        #CDN
        $cdnURL = "https://${EnvironmentName}.azureedge.net/"
        Write-Output  "<Setting name=`"CDNUrl`" value=`"${cdnURL}`" />"

        #Search
        Write-Output "<Setting name=`"SearchServiceName`" value=`"${EnvironmentName}`" />"
        $searchResource = Get-AzureRmResource -ResourceGroupName $ResourceGroup -ResourceName $EnvironmentName -ResourceType Microsoft.Search/searchServices
        $searchKey = (Invoke-AzureRmResourceAction -Action listAdminKeys -ResourceId ($searchResource.ResourceId) -Force).PrimaryKey
        if($Provision) {
            $searchKey = Provision-KV -VaultName $EnvironmentName -Name "SearchServiceAdminKey" -Value $searchKey
            $searchKey = "kv:${searchKey}"
        }
        Write-Output "<Setting name=`"SearchServiceAdminKey`" value=`"${searchKey}`" />"

        # Get the KV URL
        $kvURI = (Get-AzureRmKeyVault -VaultName $EnvironmentName).VaultUri
        Write-Output "<Setting name=`"SocPlus-KV-vaultURL`" value=`"${kvURI}`" />"

        # Get the bearer Token URI
        $bearerToken = (Get-AzureKeyVaultKey -VaultName $EnvironmentName -Name $bearerTokenName).Id
        Write-Output "<Setting name=`"SocPlus-KV-bearerTokenSigningKey`" value=`"${bearerToken}`" />"
    }

    end {
    }
}

function Provision-KV {
    param (
        [Parameter(
            Mandatory=$true
        )]
        [string] $VaultName,

        [Parameter(
            Mandatory=$true
        )]
        [string] $Name,

        [Parameter(
            Mandatory=$true
        )]
        [string] $Value
    )

    process {
        # Convert it from string to secure string
        $secretValue = ConvertTo-SecureString -String $Value -AsPlainText -Force

        # Insert the secure string in the KV
        $KVId = (Set-AzureKeyVaultSecret -VaultName $VaultName -Name $Name  -SecretValue $secretValue).Id;

        return $KVId
    }
}

