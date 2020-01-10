function New-SocialPlusEnvironment {
    <#
    .NOTES
        Name: New-SocialPlusEnvironment.ps1
        Requires: Azure Powershell version 2.1 or higher.
    .SYNOPSIS
        Creates a new SocialPlus Azure instance.
    .DESCRIPTION
        Implements all logic needed to create a new SocialPlus Azure instance.
        Assumes that the user is already logged in the Azure RM.
    .PARAMETER Name
        Name of the SocialPlus instance to create.
    .PARAMETER SendGridInstrumentationKey
        Name of the instrumentation key from SendGrid.
    .PARAMETER EmbeddedSocialClientId
        Value of the AAD Application Id for the Embedded Social AAD application. 
    .PARAMETER Production
        Switch that creates the new instance as a production environment.
    .PARAMETER CreateRedis
        Switch that creates a new Redis VM for the environment.  Redis VMs are created
        by default when using the Production switch, but this switch creates a Redis VM
        for a development environment.
    .PARAMETER RedisPort
        Base port number for the Redis volatile instance running on a Linux Redis VM.
        The Redis persistent instance uses the port number of base port number + 10.
    .PARAMETER DevPortalOnly
        Switch that causes only the dev portal to be created.
    .PARAMETER SocialPlusClientCertThumbprint
        Thumbprint of SocialPlus's client certificate. The cloud service uses this certificate to authenticate
        to Azure KeyVault and to the Geneva monitoring service. Currently, all SocialPlus environmnents 
        use the same certificate.
        Certificate must be installed in local machine "MY" cert store, and in current user's store  
        The default thumbprint value starts with "5C9..."
    .PARAMETER SocialPlusSSLCertThumbprint
        Thumbprint of SocialPlus's root SSL certificate. Use a wildcard cert.  
        The default thumbprint value starts with "06E..."
    .PARAMETER AVERTUrl
        The URL for the AVERT service. 
    .PARAMETER AVERTCertThumbprint
        Thumbprint of the certificate the AVERT service calls us with.
    .PARAMETER AVERTKey
        The key for the AVERT service.
    .PARAMETER CVSKey
        The key for the CVS service.
    .PARAMETER CVSUrl
        The URL for the CVS service. 
    .PARAMETER CVSX509Thumbprint
        Thumbprint of the certificate the CVS service calls us with.
    .PARAMETER Location
        Location specifies the Azure datacenter location where resources are created.  The default is "West US".
    .EXAMPLE
    	[PS] C:\>New-SocialPlusEnvironment -Name sp-dev-stefan -SendGridInstrumentationKey xxxxxx -Verbose
    #>

    param (
        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='Name of the environment to create'
        )]
        [Alias("Name")]
        [string] $EnvironmentName,

        [Parameter(
            Mandatory=$false,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='Embedded Social client id'
        )]
        # defaults to Embedded Social AAD Application Id (aka clientID)
        [string] $EmbeddedSocialClientId = "155f75c0-e6e2-4953-af4e-aaaf7300ba23",

        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='SendGrid instrumentation key'
        )]
        # defaults to hard-coded SendGrid instrumentation key
        [string] $SendGridInstrumentationKey,

        [switch] $Production,

        [switch] $CreateRedis,

        [Parameter(
            Mandatory=$false,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='Redis port number'
        )]
        [int] $RedisPort = 0,

        [switch] $DevPortalOnly,

        [Parameter(
            Mandatory=$false,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='SocialPlus Client Certificate thumbprint'
        )]
        [string] $SocialPlusClientCertThumbprint = "5C9B335BAD911D7CE64263192F65E040779530BC",

        [Parameter(
            Mandatory=$false,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='SocialPlus SSL Certificate thumbprint'
        )]
        [string] $SocialPlusSSLCertThumbprint = "cdd1bb89a99a8a42c487fc816eaefff1493f04c6",

        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='The URL for the AVERT service'
        )]
        [string] $AVERTUrl,

        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='Thumbprint of the certificate the AVERT service calls us with'
        )]
        [string] $AVERTCertThumbprint,

        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='The key for the AVERT service'
        )]
        [string] $AVERTKey,

        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='The key for the CVS service'
        )]
        [string] $CVSKey,

        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='The URL for the CVS service'
        )]
        [string] $CVSUrl,

        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='Thumbprint of the certificate the CVS service calls us with'
        )]
        [string] $CVSX509Thumbprint,

        [Parameter(
            Mandatory=$true,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='The rate limit threshold in reqs/minute'
        )]
        # RateLimit is unitialized, set it to -1
        [int] $RateLimitPerMinute = -1,

        [Parameter(
            Mandatory=$false,
            ValueFromPipeline=$true,
            ValueFromPipelineByPropertyName=$true,
            HelpMessage='Location'
        )]
        [string] $Location = "West US"
    )

    begin {
        # If set to create redis, the port must be set to a sane port number value
        if (($CreateRedis -Or $Production) -And ($RedisPort -le 1024 -Or $RedisPort -gt 65535)) {
           throw "Redis port must be set properly."
        }
        
        # Check the client certificate is present in local store
        $clientCertExists = Test-Path Cert:\LocalMachine\My\${SocialPlusClientCertThumbprint}
        if ([string]::IsNullOrEmpty($SocialPlusClientCertThumbprint) -or !$clientCertExists)
        {
          throw "Certificate with thumbprint ${SocialPlusClientCertThumbprint} not present in my certificate store."
        }
        $clientCert = Get-Item Cert:\LocalMachine\MY\${SocialPlusClientCertThumbprint}

        # Write to screen info about passed in parameters
        if ($Production) {

            # Check the SSL certificate now for production environments
            $sslCertExists = Test-Path Cert:\LocalMachine\My\${SocialPlusSSLCertThumbprint}
            if ([string]::IsNullOrEmpty($SocialPlusSSLCertThumbprint) -or !$sslCertExists)
            {
            throw "Certificate with thumbprint ${SocialPlusSSLCertThumbprint} not present in my certificate store."
            }
            $sslCert = Get-Item Cert:\LocalMachine\MY\${SocialPlusSSLCertThumbprint}

            if ($DevPortalOnly) {
                Write-Host "Creating dev-portal for production environment $EnvironmentName."
            } else {
                Write-Host "Creating production environment $EnvironmentName."
            }
        } else {
            if ($DevPortalOnly) {
                Write-Host "Creating dev-portal for dev environment $EnvironmentName."
            } else {
                Write-Host "Creating dev environment $EnvironmentName."
            }
        }

        # Implement a simple fuse by asking the user to press 'y' to continue
        $message  = 'This script creates Azure resources for a new SocialPlus environment.'
        $question = 'Are you sure you want to proceed?'

        $choices = New-Object Collections.ObjectModel.Collection[Management.Automation.Host.ChoiceDescription]
        $choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&Yes'))
        $choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&No'))

        $decision = $Host.UI.PromptForChoice($message, $question, $choices, 1)
        if ($decision -ne 0) {
          throw 'cancelled'
        }
    }

    process {
        $ResourceGroup = $EnvironmentName

        # Create the manifest as a hashtable
        $manifest = @{}

        # Add EnvironmentName to manifest
        $manifest.EnvironmentName = $EnvironmentName
        
        # Write to the manifest the AAD Embedded Social clientID and appKey (appKey will soon be deprecated)
        $manifest.AADEmbeddedSocialClientId = $EmbeddedSocialClientId
        $manifest.AADSocialPlusAppKey = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"

        # Write to the manifest the AAD Microsoft client ID
        $manifest.AADMicrosoftClientId = "72f988bf-86f1-41af-91ab-2d7cd011db47"

        # write to the manifest SendGrid's instrumentation key
        $manifest.SendGridKey = $SendGridInstrumentationKey

        # write the AVERT values to the manifest
        $manifest.AVERTUrl = $AVERTUrl
        $manifest.AVERTCertThumbprint = $AVERTCertThumbprint
        $manifest.AVERTKey = $AVERTKey

        # If RateLimit is unitialized, set it to 3000 for dev environments and 6000 for prod environments
        if ($RateLimitPerMinute -le 0) {
            if ($Production) {
                $manifest.RateLimitPerMinute = 6000;
            } 
            else {
                $manifest.RateLimitPerMinute = 3000;
            }
        }
        else {
            $manifest.RateLimitPerMinute = $RateLimitPerMinute;
        }

        # write the CVS values to the manifest
        $manifest.CVSKey = $CVSKey
        $manifest.CVSUrl = $CVSUrl
        $manifest.CVSX509Thumbprint = $CVSX509Thumbprint

        # if creating a new environment for the service
        if (!($DevPortalOnly)) {
            # create the resource group and update the manifest
            $manifest.ResourceGroup = New-AzureRmResourceGroup -Name $ResourceGroup -Location $Location
            Write-Verbose ($manifest.ResourceGroup | Out-String)
            Write-Verbose "Created resource group $ResourceGroup."

            # create a key vault in the resource group 
            $manifest.KeyVault = New-AzureRmKeyVault -VaultName $EnvironmentName -ResourceGroupName $ResourceGroup -Location $Location
            Write-Verbose ($manifest.KeyVault | Out-String)
            Write-Verbose "Created key vault $EnvironmentName"
            
            # we postpone provisioning the key vault until later in the script because sometimes we noticed faults 
            # when trying to provision it right-away

            # create a storage account in the resource group
            $storageName =  $EnvironmentName.Replace("-","")

            # use GRS for a production account, and LRS for a dev account
            if ($Production) { $storageType = "Standard_GRS" } else { $storageType = "Standard_LRS" }
            Write-Verbose "Creating storage account $storageName..."
            $manifest.TableStorageAccount = New-AzureRmStorageAccount -ResourceGroupName $ResourceGroup -Name $storageName `
              -Location $Location -SkuName $storageType
            Write-Verbose ($manifest.TableStorageAccount | Out-String)
            $manifest.TableStorageAccountKey1 = (Get-AzureRmStorageAccountKey -ResourceGroupName $ResourceGroup `
              -Name $storageName)[0].Value              
            Write-Verbose ($manifest.TableStorageAccountKey1 | Out-String)
            $manifest.TableStorageAccountKey2 = (Get-AzureRmStorageAccountKey -ResourceGroupName $ResourceGroup `
              -Name $storageName)[1].Value
            Write-Verbose ($manifest.TableStorageAccountKey2 | Out-String)
            Write-Verbose "Created storage account $storageName"
            
            # create a cloud service in the resource group
            $manifest.CloudService = New-AzureRmResource -ResourceGroupName $ResourceGroup -ResourceName $EnvironmentName `
              -ResourceType "Microsoft.ClassicCompute/domainNames" `
              -Properties @{} -Location $Location -Force
            Write-Verbose ($manifest.CloudService | Out-String)
            Write-Verbose "Created cloud service $EnvironmentName"
            
            # Upload client certificate
            $manifest.ServiceCertificate = $clientCert
            Write-Verbose ($manifest.ServiceCertificate | Out-String)
            $manifest.ServiceCertificateAdded = Add-AzureCertificate -ServiceName $EnvironmentName -CertToDeploy $clientCert
            Write-Verbose ($manifest.ServiceCertificateAdded | Out-String)
            Write-Verbose "Uploaded certificate with thumbprint $SocialPlusClientCertThumbprint to $EnvironmentName service"

            # Upload SSL certificate for Production
            if ($Production) {
                $manifest.SSLCertificate = $sslCert
                Write-Verbose ($manifest.ServiceCertificate | Out-String)
                $manifest.SSLCertificateAdded = Add-AzureCertificate -ServiceName $EnvironmentName -CertToDeploy $sslCert
                Write-Verbose ($manifest.SSLCertificateAdded | Out-String)
                Write-Verbose "Uploaded certificate with thumbprint $SocialPlusSSLCertThumbprint to $EnvironmentName service"
            }

            # create a service bus instance
            if ($Production) { $sbType = "Premium" } else { $sbType = "Basic" }
            $manifest.ServiceBus = New-AzureRmServiceBusNamespace -ResourceGroupName $ResourceGroup -Name $EnvironmentName `
                -Location $Location -SkuName $sbType
            Write-Verbose ($manifest.ServiceBus | Out-String)
            $manifest.ServiceBusConnectionString = (Get-AzureRmServiceBusKey -Namespace $EnvironmentName -ResourceGroupName `
                $ResourceGroup -Name RootManageSharedAccessKey).PrimaryConnectionString
            Write-Verbose ($manifest.ServiceBusConnectionString | Out-String)
            Write-Verbose "Created service bus namespace $EnvironmentName"
	
            # create a push notification hub namespace in the resource group
            $hubName = "push-$EnvironmentName"
            $manifest.PushNotificationHubNamespace = New-AzureRmNotificationHubsNamespace -ResourceGroup $ResourceGroup `
                -Namespace $hubName -Location $Location 
            Write-Verbose ($manifest.PushNotificationHubNamespace | Out-String)

            $manifest.PushNotificationHubConnectionString = (Get-AzureRmNotificationHubsNamespaceListKeys -ResourceGroup $ResourceGroup `
                -Namespace $hubName -AuthorizationRule RootManageSharedAccessKey).PrimaryConnectionString
            Write-Verbose ($manifest.PushNotificationHubConnectionString | Out-String)
            Write-Verbose "Created notification hub $hubName"

            # create a classic blob storage account
            # this storage account must be classic so that it can be used for deploying to the cloud service
            # it is also used to back the CDN
            $blobName = "${storageName}blob"
            $manifest.BlobStorageAccount = New-AzureRmResource -ResourceGroupName $ResourceGroup -ResourceName $blobName `
              -ResourceType "Microsoft.ClassicStorage/storageAccounts" -ApiVersion "2015-12-01" `
              -Properties @{ "accountType" = "$storageType" } -Location $Location -Force
            Write-Verbose ($manifest.BlobStorageAccount | Out-String)
            $manifest.BlobStorageAccountPrimary = (Get-AzureStorageKey -StorageAccountName $blobName).Primary
            Write-Verbose ($manifest.BlobStorageAccountPrimary | Out-String)
            $manifest.BlobStorageAccountSecondary = (Get-AzureStorageKey -StorageAccountName $blobName).Secondary
            Write-Verbose ($manifest.BlobStorageAccountSecondary | Out-String)
            Write-Verbose "Created blob storage account $blobName"

            # create a CDN profile
            $profileName = "$EnvironmentName-profile"
            $cdnSku = "Standard_Verizon"
            $manifest.CDNProfile = New-AzureRmCdnProfile -ResourceGroupName $ResourceGroup -ProfileName $profileName `
                -Location $Location -Sku $cdnSku
            Write-Verbose ($manifest.CDNProfile | Out-String)

            # create a CDN endpoint
            $epOriginName = "$blobName"
            $epOriginHostname ="$blobName.blob.core.windows.net"
            $manifest.CDNEndpoint = New-AzureRmCdnEndpoint -EndpointName $EnvironmentName -CdnProfile $manifest.CDNProfile -IsHttpAllowed $true `
                -IsHttpsAllowed $true -OriginName $epOriginName -OriginHostName $epOriginHostname -OriginHostHeader $epOriginHostname
            Write-Verbose ($manifest.CDNEndpoint | Out-String)
            Write-Verbose "Created CDN endpoint $EnvironmentName"

            # Create an azure search instance
            $searchName = "$EnvironmentName"
            if ($Production) {
                # for a production account, use the standard tier.
                $searchSku = "standard"
            } else {
                # for a dev account, use the basic tier.
                $searchSku = "basic"
            }
            $manifest.Search = New-AzureRmResource -Name $searchName -ResourceGroupName $ResourceGroup `
              -ResourceType "Microsoft.Search/searchServices" -Location $Location -Properties @{} `
              -sku @{ "name" = "${searchSku}" } -Force
            Write-Verbose ($manifest.Search | Out-String)
            $manifest.SearchResource = Get-AzureRmResource -ResourceGroupName $ResourceGroup -ResourceName $EnvironmentName `
              -ResourceType Microsoft.Search/searchServices
            Write-Verbose ($manifest.SearchResource | Out-String)
            $manifest.SearchPrimaryKey = (Invoke-AzureRmResourceAction -Action listAdminKeys `
              -ResourceId ($manifest.SearchResource.ResourceId) -Force).PrimaryKey
            Write-Verbose ($manifest.SearchPrimaryKey | Out-String)
            $manifest.SearchSecondaryKey = (Invoke-AzureRmResourceAction -Action listAdminKeys `
              -ResourceId ($manifest.SearchResource.ResourceId) -Force).SecondaryKey
            Write-Verbose ($manifest.SearchSecondaryKey | Out-String)
            Write-Verbose "Created search instance $searchName"

            if ($Production -or $CreateRedis) {
                # create a new network security group
                $manifest.NetworkSecurityRuleConfig1 = New-AzureRmNetworkSecurityRuleConfig -Access Allow -Direction Inbound `
                  -Name "allow-redis" -SourcePortRange "*" -DestinationPortRange "6380-6399" -Priority 1100 `
                  -Protocol Tcp -SourceAddressPrefix "*" -DestinationAddressPrefix "*"
                Write-Verbose ($manifest.NetworkSecurityRuleConfig1 | Out-String)
                Write-Verbose "Created network security rule #1"

                $manifest.NetworkSecurityRuleConfig2 = New-AzureRmNetworkSecurityRuleConfig -Access Allow -Direction Inbound `
                  -Name "allow-ssh" -SourcePortRange "*" -DestinationPortRange "22" -Priority 1000 `
                  -Protocol Tcp -SourceAddressPrefix "*" -DestinationAddressPrefix "*"
                Write-Verbose ($manifest.NetworkSecurityRuleConfig2 | Out-String)
                Write-Verbose "Created network security rule #2"

                $nsgName = "$EnvironmentName-nsg"
                $manifest.NetworkSecurityGroup = New-AzureRmNetworkSecurityGroup -Name $nsgName -Location $Location `
                  -ResourceGroupName $ResourceGroup -SecurityRules $manifest.NetworkSecurityRuleConfig1,$manifest.NetworkSecurityRuleConfig2
                Write-Verbose ($manifest.NetworkSecurityGroup | Out-String)
                Write-Verbose "Created network security group $nsgName"

                # create a subnet in the resource group
                $subnetName = "subnet1"
                $manifest.NetworkSubnetConfig = New-AzureRmVirtualNetworkSubnetConfig -NetworkSecurityGroup $manifest.NetworkSecurityGroup -Name $subnetName -AddressPrefix 10.0.0.0/24
                Write-Verbose ($manifest.NetworkSubnetConfig | Out-String)
                Write-Verbose "Created subnet $subnetName"

                # create a virtual network in the resource group and attach the subnet
                $vnetName = "$EnvironmentName-vnet"
                $manifest.VirtualNetwork = New-AzureRmVirtualNetwork -ResourceGroupName $ResourceGroup -Name $vnetName `
                  -Location $Location -AddressPrefix "10.0.0.0/16" -Subnet $manifest.NetworkSubnetConfig
                Write-Verbose ($manifest.VirtualNetwork | Out-String)
                Write-Verbose "Created vnet $vnetName"

                $nicName = "$EnvironmentName-nic1"

                # create a public ip address
                $vmName = "$EnvironmentName-redis1"
                $manifest.PublicIpAddress = New-AzureRmPublicIpAddress -Name $nicName -ResourceGroupName $ResourceGroup `
                  -DomainNameLabel $vmName -Location $Location -AllocationMethod Dynamic
                Write-Verbose ($manifest.PublicIpAddress | Out-String)
                Write-Verbose "Created public ip addresss for $nicName"

                # TODO: attempt to simplify the code for New-AzureRmNetworkInterface without breaking the script
                # create a nic attached to the subnet, with the previously created public ip address, and with 
                # the previously created network security group
                $pip = Get-AzureRmPublicIpAddress -Name $nicName -ResourceGroupName $ResourceGroup
                $nsg = Get-AzureRmNetworkSecurityGroup -Name $nsgName -ResourceGroupName $ResourceGroup
                $vnet = Get-AzureRmVirtualNetwork -Name $vnetName -ResourceGroupName $ResourceGroup
                $subnet = Get-AzureRmVirtualNetworkSubnetConfig -Name $subnetName -VirtualNetwork $vnet
                $manifest.NetworkInterface = New-AzureRmNetworkInterface -Name $nicName -ResourceGroupName $ResourceGroup `
                  -Location $Location -Subnet $subnet -PublicIpAddress $pip -NetworkSecurityGroup $nsg
                Write-Verbose ($manifest.NetworkInterface | Out-String)
                Write-Verbose "Created a network interface $nicName"

                # create an availability set in the resource group
                $avsetName = "$EnvironmentName-avset"
                $manifest.AvailabilitySet = New-AzureRmAvailabilitySet -ResourceGroupName $ResourceGroup -Name $avsetName `
                  -Location $Location -PlatformUpdateDomainCount 3 -PlatformFaultDomainCount 3
                Write-Verbose ($manifest.AvailabilitySet | Out-String)
                Write-Verbose "Created availability set $avsetName"

                # create a VM config object
                if ($Production) {
                    $vmsize = "Standard_D1"
                } else {
                    $vmsize = "Standard_A0"
                }
                $manifest.VMConfig = New-AzureRmVMConfig -VMName $vmName -VMSize $vmSize -AvailabilitySetId $manifest.AvailabilitySet.Id
                Write-Verbose ($manifest.VMConfig | Out-String)
                Write-Verbose "Created a VM configuration $vmName in availability set $avsetName"

                # configure the VM source image
                $pubName = "OpenLogic"
                $offerName = "CentOS"
                $skuName = "7.4"
                $manifest.VMSourceImage = Set-AzureRmVMSourceImage -VM $manifest.VMConfig -PublisherName $pubName `
                  -Offer $offerName -Skus $skuName -Version "latest"
                Write-Verbose ($manifest.VMSourceImage | Out-String)
                Write-Verbose "Configured the VM source image"

                # attach the Nic to the VM
                $manifest.VMNetworkInterface = Add-AzureRmVMNetworkInterface -VM $manifest.VMSourceImage -Id $manifest.NetworkInterface.Id
                Write-Verbose ($manifest.VMNetworkInterface | Out-String)
                Write-Verbose "Attached the nic to the VM"

                # configure OS disk for the VM
                $diskName = "OSDisk"
                $osDiskUri = "https://${storageName}.blob.core.windows.net/vhds/${vmName}.vhd"
                $manifest.VMNetworkInterface = Set-AzureRmVMOSDisk -VM $manifest.VMConfig -Name $diskName -VhdUri $osDiskUri `
                  -CreateOption "fromImage"
                Write-Verbose ($manifest.VMNetworkInterface | Out-String)
                Write-Verbose "Configured the OS disk for the VM"

                # set the VM Operating System type, and a password
                # Even though this appears to be a hard-coded password here, we disable password authentication.
                $linuxAdminUsername = "spadmin"
                $linuxAdminPassword = "passwordDisabled" 
                $manifest.LinuxAdminUsername = $linuxAdminUsername
                $manifest.LinuxAdminPassword = $linuxAdminPassword
                $linuxAdminPassword = $linuxAdminPassword | ConvertTo-SecureString -asPlainText -Force
                $linuxAdminCredentials = New-Object -typename System.Management.Automation.PSCredential `
                  -argumentList $linuxAdminUsername,$linuxAdminPassword
                $manifest.VMOS = Set-AzureRmVMOperatingSystem -Credential $linuxAdminCredentials -VM $manifest.VMConfig `
                  -Linux -ComputerName $vmName -DisablePasswordAuthentication
                Write-Verbose ($manifest.VMOS | Out-String)
                Write-Verbose "Configured the VM operating system"

                # Create vm-keys directory if missing
                if (!(Test-Path vm-keys)) {
                    New-Item -ItemType Directory vm-keys
                }

                # if the private key doesn't already exist, then run ssh-keygen to generate a keypair
                # Create vm-keys directory if missing
                if (!(Test-Path vm-keys)) {
                    New-Item -ItemType Directory vm-keys
                }
                $privateKey = "vm-keys\${vmName}_id_rsa"
                $publicKey = "vm-keys\${vmName}_id_rsa.pub"
                if (!(Test-Path $privateKey -PathType Leaf)) {
                    & .\bin\ssh-keygen.exe -t rsa -b 2048 -N "''" -f $privateKey
                }
                $manifest.PrivateKey = (Get-Content $privateKey) -join "`n"
                $manifest.PublicKey = (Get-Content $publicKey) -join "`n"

                # provide ssh configuration
                $sshPath = "/home/spadmin/.ssh/authorized_keys"
                $sshKey = Get-Content $publicKey
                $manifest.VMSshPublicKey = Add-AzureRmVMSshPublicKey -VM $manifest.VMOS -KeyData $sshKey -Path $sshPath
                Write-Verbose ($manifest.VMSshPublicKey | Out-String)
                Write-Verbose "Configured SSH"

                # Before we run the New-AzureRmVM command, we must set the admin password to null
                $manifest.VMSshPublicKey.OSProfile.AdminPassword = [NullString]::Value

                $manifest.VM = New-AzureRmVM -ResourceGroupName $ResourceGroup -Location $Location -VM $manifest.VMSshPublicKey
                Write-Verbose ($manifest.VM | Out-String)
                Write-Verbose "Created the VM $vmName"
            }

            # create and provision a signing key
            $manifest.SigningKey = Add-AzureKeyVaultKey -VaultName $EnvironmentName -Name SigningKey `
              -Destination Software
            Write-Verbose ($manifest.SigningKey | Out-String)
            Write-Verbose "Signing key created and provisioned in the key vault $EnvironmentName"
            
            # create and provision a hashing key (32 bytes of randomness)
            $manifest.HashingKeyPlain = & .\bin\wapg.exe -a 1 -m 32 -n 1 -M NCL
            Write-Verbose ($manifest.HashingKeyPlain | Out-String)
            $secretHashingKey = ConvertTo-SecureString -String $manifest.HashingKeyPlain -AsPlainText -Force
            $manifest.HashingKey = Set-AzureKeyVaultSecret -VaultName $EnvironmentName `
              -Name HashingKey -SecretValue $secretHashingKey
            Write-Verbose ($manifest.HashingKey | Out-String)
            Write-Verbose "Hashing key created and provisioned in the key vault $EnvironmentName"
            
            # set permissions on the keyvault for the following accounts: soplazur, ssaroiu, alecw, sagarwal, cuervo
            $keyPerms = @("decrypt","encrypt","verify","sign","get","list","update","create","delete")
            $secretPerms = @("get","list","set","delete","backup","restore","recover","purge")
            Set-AzureRmKeyVaultAccessPolicy -VaultName $EnvironmentName -UserPrincipalName 'soplazur@microsoft.com' `
              -PermissionsToKeys $keyPerms -PermissionsToSecrets $secretPerms
            Set-AzureRmKeyVaultAccessPolicy -VaultName $EnvironmentName -UserPrincipalName 'ssaroiu@microsoft.com' `
              -PermissionsToKeys $keyPerms -PermissionsToSecrets $secretPerms
            Set-AzureRmKeyVaultAccessPolicy -VaultName $EnvironmentName -UserPrincipalName 'alecw@microsoft.com' `
              -PermissionsToKeys $keyPerms -PermissionsToSecrets $secretPerms
            Set-AzureRmKeyVaultAccessPolicy -VaultName $EnvironmentName -UserPrincipalName 'sagarwal@microsoft.com' `
              -PermissionsToKeys $keyPerms -PermissionsToSecrets $secretPerms
            Set-AzureRmKeyVaultAccessPolicy -VaultName $EnvironmentName -UserPrincipalName 'cuervo@microsoft.com' `
              -PermissionsToKeys $keyPerms -PermissionsToSecrets $secretPerms
            Write-Verbose "Permissions to key vault $EnvironmentName granted"

            # set permissions on the keyvault for our SocialPlus service
            Set-AzureRmKeyVaultAccessPolicy -VaultName $EnvironmentName -ServicePrincipalName $EmbeddedSocialClientId `
              -PermissionsToSecrets get,list
            Set-AzureRmKeyVaultAccessPolicy -VaultName $EnvironmentName -ServicePrincipalName $EmbeddedSocialClientId `
              -PermissionsToKeys sign,verify,get
            Write-Verbose "Permissions to key vault $EnvironmentName for SocialPlus application granted"

            # generate redis configuration passwords one for persistent and one for volatile
            $persistPassword = & .\bin\wapg.exe -a 1 -m 80 -n 1 -M NCL
            $persistPort = $RedisPort + 10
            $manifest.RedisPersistPort = $persistPort
            $manifest.RedisPersistPassword = $persistPassword

            $volatilePassword = & .\bin\wapg.exe -a 1 -m 80 -n 1 -M NCL
            $manifest.RedisVolatilePort = $RedisPort
            $manifest.RedisVolatilePassword = $volatilePassword
        }

        # create the portal cloud service
        $portalName = "$EnvironmentName-portal"
        $manifest.PortalCloudService = New-AzureRmResource -ResourceGroupName $ResourceGroup -ResourceName $portalName `
          -ResourceType "Microsoft.ClassicCompute/domainNames" `
          -Properties @{} -Location $Location -Force
        Write-Verbose ($manifest.PortalCloudService | Out-String)
        Write-Verbose "Created cloud service for portal $portalName"
        
        # Upload certificate
        $manifest.SqlCertificate = $clientCert
        Write-Verbose ($manifest.SqlCertificate | Out-String)
        $manifest.SqlCertificateAdded = Add-AzureCertificate -ServiceName $portalName -CertToDeploy $clientCert
        Write-Verbose ($manifest.SqlCertificateAdded | Out-String)
        Write-Verbose "Uploaded certificate with thumbprint $SocialPlusClientCertThumbprint to $portalName service"            

        # create a sql server instance for the portal
        $sqlPortalName = "$EnvironmentName-portal-sql"
        $sqlVersion = "12.0"
        $sqlAdminUsername = "spadmin"
        $manifest.PortalSqlAdminUsername = $sqlAdminUsername
        Write-Verbose ($manifest.PortalSqlAdminUsername | Out-String)

        # use password generator to create sql admin password
        $sqlAdminPassword = & .\bin\wapg.exe -a 1 -m 14 -n 1 -M NCL
        $manifest.PortalSqlAdminPassword = $sqlAdminPassword
        $secureSqlAdminPassword = $sqlAdminPassword | ConvertTo-SecureString -asPlainText -Force 
        $sqlCredentials = New-Object -typename System.Management.Automation.PSCredential `
          -argumentList $sqlAdminUsername,$secureSqlAdminPassword
        $manifest.PortalSqlServer = New-AzureRmSqlServer -ResourceGroupName $ResourceGroup -Location $Location `
          -ServerName $sqlPortalName -ServerVersion $sqlVersion -SqlAdministratorCredentials $sqlCredentials
        Write-Verbose ($manifest.PortalSqlServer | Out-String)
        Write-Verbose "Created sql server $sqlPortalName for portal $portalName"

        Write-Output ""
        Write-Output "Sql admin password for $sqlPortalName is $sqlAdminPassword"
        Write-Output ""

        # sleep for 10 seconds after creating a SQL server before you do anything with it
        Start-Sleep -s 10

        # create firewall rules for sql server
        $manifest.PortalSqlServerFirewallRule1 = New-AzureRmSqlServerFirewallRule -AllowAllAzureIPs -ServerName $sqlPortalName `
          -ResourceGroupName $ResourceGroup
        Write-Verbose ($manifest.PortalSqlServerFirewallRule1 | Out-String)
        $manifest.PortalSqlServerFirewallRule2 = New-AzureRmSqlServerFirewallRule -FirewallRuleName "ms1" `
          -ServerName $sqlPortalName -ResourceGroupName $ResourceGroup `
          -StartIpAddress "131.107.0.0" -EndIpAddress "131.107.255.255"
        Write-Verbose ($manifest.PortalSqlServerFirewallRule2 | Out-String)
        $manifest.PortalSqlServerFirewallRule3 = New-AzureRmSqlServerFirewallRule -FirewallRuleName "ms2" `
          -ServerName $sqlPortalName -ResourceGroupName $ResourceGroup `
          -StartIpAddress "167.220.0.0" -EndIpAddress "167.220.127.255"
        Write-Verbose ($manifest.PortalSqlServerFirewallRule3 | Out-String)
        Write-Verbose "Created corpnet firewall rules for sql server $sqlPortalName"

        # create a sql database
        $dbName = "portalDb"
        if ($Production) {
            $dbEdition = "Standard"
            $dbPerformanceLevel = "S1"
            $manifest.PortalSqlDatabase = New-AzureRmSqlDatabase -ResourceGroupName $ResourceGroup -ServerName $sqlPortalName `
              -DatabaseName $dbName -Edition $dbEdition -RequestedServiceObjectiveName $dbPerformanceLevel
        } else {
            $dbEdition = "Basic"
            $manifest.PortalSqlDatabase = New-AzureRmSqlDatabase -ResourceGroupName $ResourceGroup -ServerName $sqlPortalName `
              -DatabaseName $dbName -Edition $dbEdition
        }
        Write-Verbose ($manifest.PortalSqlDatabase | Out-String)
        Write-Verbose "Created sql server database $dbName on $sqlPortalName"
    }

    end {
        # Write the manifest to local file with a format of manifest.timestamp.txt and manifest.timestamp.json 
        $manifestObject = New-Object -TypeName PSObject -Property $manifest

        $timestamp = get-date -f yyyy_MM_dd_HH_MM_ss
        $filename = "manifest.$EnvironmentName.$timestamp"
        $txtFilename = $filename + ".txt"
        $jsonFilename = $filename + ".json"        
        $manifestObject | Out-File $txtFilename
        $manifestObject | ConvertTo-Json | Out-File $jsonFilename

        Write-Verbose "Finished."
    }
}
