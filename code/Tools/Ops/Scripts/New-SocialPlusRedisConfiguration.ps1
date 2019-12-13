function New-SocialPlusRedisConfiguration {
    <#
    .NOTES
        Name: New-SocialPlusRedisConfiguration.ps1
        Requires: Azure Powershell version 1.1 or higher.
    .SYNOPSIS
        Creates a new SocialPlus Redis VM configuration.
    .DESCRIPTION
        Sets up a the configuration of a remote Redis VM server.
        Assumes that the user is already logged in the Azure RM.
    .PARAMETER ManifestFileName
        Name of the SocialPlus environment manifest
    .EXAMPLE
    	[PS] C:\>New-SocialPlusRedisConfiguration -name manifest.2016_03_24_15_03_31.json -Verbose	
    #>

    param (
        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='Manifest Name'
        )]
        [Alias("Name")]
        [string] $ManifestFileName
    )

    begin {    
        # Read the json input file into an object
        $manifest = (Get-Content $ManifestFileName) -join "`n" | ConvertFrom-Json

        # Variables used for Redis config
        $redisServer = $manifest.PublicIpAddress.DnsSettings.Fqdn
        $userName = $manifest.LinuxAdminUsername

        # Make temporary file with private key from manifest
        $Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding($False)
        $filePrivateKey = Join-Path $Pwd -ChildPath ".\redis.privatekey"
        [System.IO.File]::WriteAllLines($filePrivateKey, $manifest.PrivateKey, $Utf8NoBomEncoding)

        # Construct the Redis configuration file paths
        $nameNoDashes = $manifest.TableStorageAccount.ResourceGroupName.Replace("-","")
        $baseDir = "redis\centos"
        $baseDevDir = "redis\centos\dev"
        $baseScriptsDir = "redis\centos\scripts"
        $outputPersist = "redis-persistent-${nameNoDashes}.conf"
        $outputVolatile = "redis-volatile-${nameNoDashes}.conf"
        $outputServicePersist = "redis-persistent-${nameNoDashes}.service"
        $outputServiceVolatile = "redis-volatile-${nameNoDashes}.service"
        $thpDisableService = "thpdisable.service"
        $installScriptName = "install-packages.sh"
        $configureScriptName = "system-configure.sh" 

        $redisPersistentPort = $manifest.RedisPersistPort
        $redisVolatilePort = $manifest.RedisVolatilePort
    }

    process {
        # Copy all needed configuration files to Redis server
        Write-Verbose "Copying files to remote Redis server..."

        # Using ssh to copy. scp.exe has the path to ssh hardcoded and we cannot get it to work.
        # We also turn off checking the host key against our authorized_hosts file
        type "${baseDevDir}\${outputPersist}" | .\bin\ssh.exe -o StrictHostKeyChecking=no  -i .\redis.privatekey "${userName}@${redisServer}" "cat > /tmp/${outputPersist}"
        type "${baseDevDir}\${outputVolatile}" | .\bin\ssh.exe -o StrictHostKeyChecking=no  -i .\redis.privatekey "${userName}@${redisServer}" "cat > /tmp/${outputVolatile}"
        type "${baseDevDir}\${outputServicePersist}" | .\bin\ssh.exe -o StrictHostKeyChecking=no  -i .\redis.privatekey "${userName}@${redisServer}" "cat > /tmp/${outputServicePersist}"
        type "${baseDevDir}\${outputServiceVolatile}" | .\bin\ssh.exe -o StrictHostKeyChecking=no  -i .\redis.privatekey "${userName}@${redisServer}" "cat > /tmp/${outputServiceVolatile}"
        type "${baseDir}\${thpDisableService}" | .\bin\ssh.exe -o StrictHostKeyChecking=no  -i .\redis.privatekey "${userName}@${redisServer}" "cat > /tmp/${thpDisableService}"
        type "${baseScriptsDir}\${installScriptName}" | .\bin\ssh.exe -o StrictHostKeyChecking=no  -i .\redis.privatekey "${userName}@${redisServer}" "cat > /tmp/${installScriptName}; chmod a+x /tmp/${installScriptName}; perl -pi -e 's/\cM//g' /tmp/${installScriptName}"
        type "${baseScriptsDir}\${configureScriptName}" | .\bin\ssh.exe -o StrictHostKeyChecking=no  -i .\redis.privatekey "${userName}@${redisServer}" "cat > /tmp/${configureScriptName}; chmod a+x /tmp/${configureScriptName}; perl -pi -e 's/\cM//g' /tmp/${configureScriptName}"
        Write-Verbose "Copied files to remote Redis server."

        Write-Verbose "Running Redis server configuration scripts (takes several minutes)..."
        .\bin\ssh.exe -t -o StrictHostKeyChecking=no -i .\redis.privatekey "${userName}@${redisServer}" "cd /tmp; ./${installScriptName}; ./${configureScriptName} -pp ${redisPersistentPort} -vp ${redisVolatilePort} -n ${nameNoDashes}"
        Write-Verbose "Redis server configuration scripts done."

        Write-Verbose "Rebooting Redis server..."
        .\bin\ssh.exe -t -o StrictHostKeyChecking=no -i .\redis.privatekey "${userName}@${redisServer}" "sudo reboot"
        Write-Verbose "Redis server ready."
    }

    end {
        # remove private key
        del ".\redis.privatekey"
    }

}