#
# this script does the equivalent of the Visual Studio publish operation for a cloud service
#

function Set-SocialPlusDeployment {
    <#
    .SYNOPSIS
        Creates or updates a SocialPlus deployment.
    .DESCRIPTION
        Implements creation or update of a SocialPlus deployment.  Assumes that the user is already 
        logged in to Azure.
    .PARAMETER Name
        Name of the SocialPlus instance to deploy.
    .PARAMETER ConfigFile
        Name of the configuration file to deploy
    .PARAMETER ExtensionsDir
        Name of the directory where the diagnostics extensions are stored
    .PARAMETER PackageFile
        Name of the package file to deploy
    .PARAMETER SlotName
        Slot for the deployment.  Slot names are Staging or Production.
    #>
    param(
        [Parameter(
             Mandatory=$true,
             HelpMessage='Name of the config file to deploy'
         )]
        [string] $ConfigFile,

        [Parameter(
            Mandatory=$true,
            HelpMessage='Name of the environment to deploy'
        )]
        [Alias("Name")]
        [string] $EnvironmentName,    

        [Parameter(
            Mandatory=$true,
            HelpMessage='Name of the directory where the diagnostics extensions are located'
        )]
        [string] $ExtensionsDir,    

        [Parameter(
             Mandatory=$true,
             HelpMessage='Name of the package file to deploy'
         )]
        [string] $PackageFile,

        [Parameter(
            Mandatory=$true,
            HelpMessage='Deployment slot (Staging or Production)'
        )]
        [string] $SlotName
    )

    process {
        if (! (Test-Path $PackageFile)) {
            Write-Output "Cannot find package $PackageFile"
            return
        }

        if (! (Test-Path $ConfigFile)) {
            Write-Output "Cannot find configuration file $ConfigFile"
            return
        }

        $containerName = "deployments"
        $cwd = Get-Location
        $workerDiagnosticsConfig = $cwd.Path + $ExtensionsDir + "\" + "PaaSDiagnostics.SocialPlus.Server.WorkerRole.PubConfig.xml"
        $webDiagnosticsConfig = $cwd.Path + $ExtensionsDir + "\" + "PaaSDiagnostics.SocialPlus.Server.WebRole.PubConfig.xml"
        $configFullPath = $cwd.Path + "\" + $ConfigFile

        # determine name of blob storage account where we upload the package
        $blobStorageAccount =  $EnvironmentName.Replace("-","") + "blob"

        # get the storage key for the blob account
        $storageKey = (Get-AzureRmStorageAccountKey -ResourceGroupName $EnvironmentName -Name $blobStorageAccount)[0].Value

        # get the context
        $ctx = New-AzureStorageContext -StorageAccountName $blobStorageAccount -StorageAccountKey $storageKey

        # get the container (-EA 0 = -ErrorAction SilentlyContinue)
        $containerState = Get-AzureStorageContainer -Name $containerName -Context $ctx -EA 0

        # create new container if necessary
        if ($containerState -eq $null)
        {
            New-AzureStorageContainer -Name $containerName -Context $ctx | out-null
        }

        $blobName = "$EnvironmentName.package.$(get-date -f yyyy_MM_dd_HH_mm_ss).cspkg"
        # do the upload
        Set-AzureStorageBlobContent -File $PackageFile -Container $containerName -Blob $blobName -Context $ctx -Force | Out-Null
        # get the resulting Url
        $blobState = Get-AzureStorageBlob -blob $blobName -Container $containerName -Context $ctx
        $packageUrl = $blobState.ICloudBlob.uri.AbsoluteUri

        Write-Output "Package $blobName uploaded to $blobStorageAccount storage."

        # get the current deployment if it exists
        $deployment = Get-AzureDeployment -ServiceName $EnvironmentName -Slot $SlotName -EA 0

        # create a new deployment if none exists
        if ($deployment.Name -eq $null) {
            Write-Output "No deployment found in $SlotName slot for $EnvironmentName"
            $workerConfig = New-AzureServiceDiagnosticsExtensionConfig -Role "SocialPlus.Server.WorkerRole" `
              -StorageAccountName $blobStorageAccount -StorageAccountKey $storageKey `
              -DiagnosticsConfigurationPath $WorkerDiagnosticsConfig
            $webConfig = New-AzureServiceDiagnosticsExtensionConfig -Role "SocialPlus.Server.WebRole" `
              -StorageAccountName $blobStorageAccount -StorageAccountKey $storageKey `
              -DiagnosticsConfigurationPath $WebDiagnosticsConfig
            Write-Output "Creating deployment..."
            $result = New-AzureDeployment -Slot $SlotName -Package $packageUrl -Configuration $configFullPath `
              -ServiceName $EnvironmentName -ExtensionConfiguration @($workerConfig,$webConfig)
            Write-Output "Deployed to $SlotName slot for $EnvironmentName"
        } else {
            Write-Output "Deployment exists in $SlotName slot for $EnvironmentName."
            $workerConfig = Set-AzureServiceDiagnosticsExtensionConfig -Role "SocialPlus.Server.WorkerRole" `
              -StorageAccountName $blobStorageAccount -StorageAccountKey $storageKey `
              -DiagnosticsConfigurationPath $WorkerDiagnosticsConfig
            $webConfig = Set-AzureServiceDiagnosticsExtensionConfig -Role "SocialPlus.Server.WebRole" `
              -StorageAccountName $blobStorageAccount -StorageAccountKey $storageKey `
              -DiagnosticsConfigurationPath $WebDiagnosticsConfig
            Write-Output "Upgrading deployment..."
            $result = Set-AzureDeployment -Upgrade -Slot $SlotName -Package $packageUrl -Configuration $configFullPath `
              -ServiceName $EnvironmentName -ExtensionConfiguration @($workerConfig,$webConfig) -Force
            Write-Output "Upgraded deployment in $SlotName slot for $EnvironmentName"
        }

        # check on the deployment
        $deploymentId = Get-AzureDeployment -service $EnvironmentName -slot $SlotName
        Write-Output "Deployed to $SlotName slot for  $EnvironmentName with deployment id $deploymentId"
    }
    end {
        Write-Verbose "Finished."
    }
}

Set-Alias -Name Deploy-SocialPlus -Value Set-SocialPlusDeployment

