#
# Generate-RedisConfig -Name <name> -Port <port#>
#
# Note: port is specified for the volatile configuration, and port + 10 is used for the persistent configuration
#

function Generate-RedisConfig {
    param (
        [Parameter(Mandatory=$True)]
        [string]$Name,

        [Parameter(Mandatory=$True)]
        [int]$Port
    )

    begin {
        $Name = $Name.Replace("-","")
        $baseDir = "redis\centos\dev"
        $templatePersist = "${baseDir}\redis-persistent-template.conf"
        $outputPersist = "${baseDir}\redis-persistent-${Name}.conf"
        $templateVolatile = "${baseDir}\redis-volatile-template.conf"
        $outputVolatile = "${baseDir}\redis-volatile-${Name}.conf"
        $templateServicePersist = "${baseDir}\redis-persistent-template.service"
        $outputServicePersist = "${baseDir}\redis-persistent-${Name}.service"
        $templateServiceVolatile = "${baseDir}\redis-volatile-template.service"
        $outputServiceVolatile = "${baseDir}\redis-volatile-${Name}.service"
    }

    process {
        # generate a new password
        # TODO: This script should be moved to the top-level directory and the location of wapg should be fixed
        $persistPassword = & .\bin\wapg.exe -a 1 -m 80 -n 1 -M NCL
        $persistPort = $Port + 10
        Write-Verbose "new-password = $persistPassword"

        Write-Verbose "Debug: Name is $Name"

        # read in redis-persistent-template.conf
        $text = (Get-Content $templatePersist) -Join "`n" 
        (($text.replace('%%portnum%%',$persistPort)).replace('%%name%%',$Name)).replace('%%password%%', $persistPassword) | `
          Set-Content $outputPersist
        Write-Verbose "Created config file $outputPersist"

        $volPassword = & c:\bin\wapg.exe -a 1 -m 80 -n 1 -M NCL
        Write-Verbose "new-password = $volPassword"

        # read in redis-volatile-template.conf
        $text = (Get-Content $templateVolatile) -Join "`n"
        (($text.replace('%%portnum%%',$Port)).replace('%%name%%',$Name)).replace('%%password%%', $volPassword) | `
          Set-Content $outputVolatile
        Write-Verbose "Created config file $outputVolatile"

        # read in redis-persistent-template.service
        $text = (Get-Content $templateServicePersist) -Join "`n"
        $text.replace('%%name%%',$Name) | Set-Content $outputServicePersist
        Write-Verbose "Created file $outputServicePersist"

        # read in redis-volatile-template.service
        $text = (Get-Content $templateServiceVolatile) -Join "`n"
        $text.replace('%%name%%',$Name) | Set-Content $outputServiceVolatile
        Write-Verbose "Created file $outputServiceVolatile"
   }

    end {
        Write-Verbose "Finished."
    }
}
