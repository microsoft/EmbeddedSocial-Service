function Format-SocialPlusManifest {
    <#
    .NOTES
    	Name: Format-SocialPlusManifest.ps1
    	Requires: Azure Powershell with Azure subscription loaded
    .SYNOPSIS
        Prints the configuration data of an environment from the manifest
    .DESCRIPTION
        Prints out various forms of configuration data read from the manifest.
    .PARAMETER ManifestFileName
        Name of the SocialPlus environment manifest
    .PARAMETER AzureConfig
        This is a switch. If turned on, we print the environment configuration in an Azure-specific format.
        The format is easily ingestible by CloudConfigManager
    .PARAMETER RedisConfig
        This is a switch which indicates whether to create the Redis configuration files to be copied to
        the remote Redis VM machine
    .PARAMETER AppConfig
        This is a switch. If turned on, we print the environment configuration in an App-specific format.
        The format is easily ingestible by ConfigManager
    .PARAMETER Provision
        This is a switch. If turned on, the config should be provisioned into the KeyVault.
        Note that not all settings are put in the key vault. Only the secret ones.
    .EXAMPLE
    	[PS] C:\>Format-SocialPlusManifest -name manifest.2016_03_24_15_03_31.json -AzureConfig -Verbose
    	Prints the configuration settings from the manifest needed in the cscfg files
    	[PS] C:\>Format-SocialPlusManifest -name manifest.2016_03_24_15_03_31.json -AppConfig -Verbose
    	Prints the configuration settings from the manifest needed in the .config files
    #>
	
    param (
        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='Manifest Name'
        )]
        [Alias("Name")]
        [string] $ManifestFileName,

        [switch] $AzureConfig,

        [switch] $RedisConfig,

        [switch] $AppConfig,

        [Parameter(
             Mandatory=$false
        )]
        [switch] $Provision
    )

    begin {
        $config = @{}
        
        # Read the json input file into an object
        $manifest = (Get-Content $ManifestFileName) -join "`n" | ConvertFrom-Json
    
        $EnvironmentName = $manifest.TableStorageAccount.ResourceGroupName

        # Construct the Redis configuration file paths
        $nameNoDashes = $manifest.TableStorageAccount.ResourceGroupName.Replace("-","")
        $baseDir = "redis\centos\dev"
        $templatePersist = "${baseDir}\redis-persistent-template.conf"
        $outputPersist = "${baseDir}\redis-persistent-${nameNoDashes}.conf"
        $templateVolatile = "${baseDir}\redis-volatile-template.conf"
        $outputVolatile = "${baseDir}\redis-volatile-${nameNoDashes}.conf"
        $templateServicePersist = "${baseDir}\redis-persistent-template.service"
        $outputServicePersist = "${baseDir}\redis-persistent-${nameNoDashes}.service"
        $templateServiceVolatile = "${baseDir}\redis-volatile-template.service"
        $outputServiceVolatile = "${baseDir}\redis-volatile-${nameNoDashes}.service"
    }

    process {
        # Construct AzureSqlConnectionString
        $serverName = $manifest.PortalSqlDatabase.ServerName
        $dbName = $manifest.PortalSqlDatabase.DatabaseName
        $userId = $manifest.PortalSqlAdminUsername
        $userPassword = $manifest.PortalSqlAdminPassword
        $key = "AzureSqlConnectionString"
        $value = "Server=tcp:${serverName}.database.windows.net,1433;Database=${dbName};User ID=${userId}@${serverName};Password=${userPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)

        # Construct AzureStorageConnectionString
        $storageName = $manifest.TableStorageAccount.StorageAccountName
        $storageKey = $manifest.TableStorageAccountKey1
        $key = "AzureStorageConnectionString"
        $value = "DefaultEndpointsProtocol=https;AccountName=${storageName};AccountKey=${storageKey}"
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)
        
        # Construct AzureBlobStorageConnectionString
        $blobName = $manifest.BlobStorageAccount.Name
        $blobKey = $manifest.BlobStorageAccountPrimary
        $key = "AzureBlobStorageConnectionString"
        $value = "DefaultEndpointsProtocol=https;AccountName=${blobName};AccountKey=${blobKey}"
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)

        # Construct PersistentRedisConnectionString
        $redisDnsName = $manifest.PublicIpAddress.DnsSettings.Fqdn
        $redisPersistentPort = $manifest.RedisPersistPort
        $redisPersistentPassword = $manifest.RedisPersistPassword
        $key = "PersistentRedisConnectionString"
        $value = "${redisDnsName}:${redisPersistentPort},abortConnect=false,password=${redisPersistentPassword}"
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)

        # Construct VolatileRedisConnectionString
        $redisVolatilePort = $manifest.RedisVolatilePort
        $redisVolatilePassword = $manifest.RedisVolatilePassword
        $key = "VolatileRedisConnectionString"
        $value = "${redisDnsName}:${redisVolatilePort},abortConnect=false,password=${redisVolatilePassword}"
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)     

        # Construct Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString
        $key = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"
        $value = "UseDevelopmentStorage=true"
        $config.Add($key, $value)     
        
        # Construct ServiceBusConnectionString
        $key = "ServiceBusConnectionString"
        $value = $manifest.ServiceBusConnectionString
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)

        # Construct PushNotificationsConnectionString
        $key = "PushNotificationsConnectionString"
        $value = $manifest.PushNotificationHubConnectionString
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)
        
        # Construct SearchServiceName
        $key = "SearchServiceName"
        $value = $manifest.Search.Name
        $config.Add($key, $value)
        
        # Construct SearchServiceAdminKey
        $key = "SearchServiceAdminKey"
        $value = $manifest.SearchPrimaryKey
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)
        
        # Construct CDNUrl
        $key = "CDNUrl"
        $value = "https://" + $manifest.CDNEndpoint.Properties.HostName + "/"
        $config.Add($key, $value)
        
        # Construct KeyVaultUri
        $key = "KeyVaultUri"
        $value = $manifest.KeyVault.VaultUri
        $config.Add($key, $value)

        # Construct SigningKey
        $key = "SigningKey"
        $value = $manifest.SigningKey.Id
        $config.Add($key, $value)
        
        # Construct HashingKey
        $key = "HashingKey"
        $value = $manifest.HashingKey.Id
        $config.Add($key, $value)

        # Construct AADEmbeddedSocialClientId
        $key = "AADEmbeddedSocialClientId"
        $value = $manifest.AADEmbeddedSocialClientId
        $config.Add($key, $value)

        # Construct SocialPlusCertThumbprint
        $key = "SocialPlusCertThumbprint"
        $value = $manifest.ServiceCertificate.Thumbprint
        $config.Add($key, $value)

        # Construct AADMicrosoftClientId
        $key = "AADMicrosoftClientId"
        $value = $manifest.AADMicrosoftClientId
        $config.Add($key, $value)

        # Construct SendGridInstrumentationKey
        $key = "SendGridInstrumentationKey"
        $value = $manifest.SendGridKey
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)
        
        # Construct EnableMDM
        $key = "EnableMdm"
        $value = $true
        $config.Add($key, $value)

        # Construct EnableAzureSettingsReaderTracing flag
        $key = "EnableAzureSettingsReaderTracing"
        $value = $false
        $config.Add($key, $value)

        # Construct the AVERTUrl
        $key = "AVERTUrl"
        $value = $manifest.AVERTUrl
        $config.Add($key, $value)

        # Construct the AVERTCertThumbprint
        $key = "AVERTCertThumbprint"
        $value = $manifest.AVERTCertThumbprint
        $config.Add($key, $value)

        # Construct the AVERTKey
        $key = "AVERTKey"
        $value = $manifest.AVERTKey
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }
        $config.Add($key, $value)

        # Construct ServiceBusBatchIntervalMs
        $key = "ServiceBusBatchIntervalMs"
        $value = "0"
        $config.Add($key, $value)

        # Construct the CVSKey
        $key = "CVSKey"
        $value = $manifest.CVSKey
        if ($Provision) {
            $value = Put-KV -VaultName "${EnvironmentName}" -Name "${key}" -Value "${value}"
            $value = "kv:${value}"
        }   
        $config.Add($key, $value)

        # Construct the CVSUrl
        $key = "CVSUrl"
        $value = $manifest.CVSUrl
        $config.Add($key, $value)

        # Construct the CVSX509Thumbprint
        $key = "CVSX509Thumbprint"
        $value = $manifest.CVSX509Thumbprint
        $config.Add($key, $value)

        # Construct the RateLimit
        $key = "RateLimitPerMinute"
        $value = $manifest.RateLimitPerMinute;
        $config.Add($key, $value);
    }

    end {
        if ($AzureConfig) {
            $config.Keys | Sort-Object | Foreach-Object { $key = $_; "<Setting name=`"$key`" value=`"" + $config.Item($_) + "`" />" }
        }

        if ($AppConfig) {
            $config.Keys | Sort-Object | Foreach-Object { $key = $_; "<add key=`"$key`" value=`"" + $config.Item($_) + "`" />" }
        }

        if ($RedisConfig) {
            # read in redis-persistent-template.conf
            $text = (Get-Content $templatePersist) -Join "`n" 
            (($text.replace('%%portnum%%',$manifest.RedisPersistPort)).replace('%%name%%',$nameNoDashes)).replace('%%password%%', $manifest.RedisPersistPassword) | `
                Set-Content -Encoding UTF8 $outputPersist
            Write-Verbose "Created config file $outputPersist"

            # read in redis-volatile-template.conf
            $text = (Get-Content $templateVolatile) -Join "`n"
            (($text.replace('%%portnum%%',$manifest.RedisVolatilePort)).replace('%%name%%',$nameNoDashes)).replace('%%password%%', $manifest.RedisVolatilePassword) | `
              Set-Content -Encoding UTF8 $outputVolatile
            Write-Verbose "Created config file $outputVolatile"

            # read in redis-persistent-template.service
            $text = (Get-Content $templateServicePersist) -Join "`n"
            $text.replace('%%name%%',$nameNoDashes) | Set-Content -Encoding UTF8 $outputServicePersist
            Write-Verbose "Created file $outputServicePersist"

            # read in redis-volatile-template.service
            $text = (Get-Content $templateServiceVolatile) -Join "`n"
            $text.replace('%%name%%',$nameNoDashes) | Set-Content -Encoding UTF8 $outputServiceVolatile
            Write-Verbose "Created file $outputServiceVolatile"
        }
    }
}

function Put-KV {
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

