#
# CopyTables.ps1
#
# This script copies all the SocialPlus tables from one storage account to another.  It uses the local filesystem
# as an intermediary because AzCopy does not support table-to-table copies.
#
# This script requires AzCopy v4.0 or higher (older versions of AzCopy cannot copy tables)
#

# name and storage key for storage account you are copying the tables from
$sourceAcctName = "socialplusprod"
$sourceKey = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"

# name and storage key for storage account you are copying the tables to
$destAcctName = "spprod1"
$destKey = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"

$localDir = "d:\home\alecw\research\social-plus\azcopy"

$azcopy = "c:\Program Files (x86)\Microsoft SDKs\azure\AzCopy\AzCopy.exe"

$tableNames = @(
    "TopicLookupTable", 
    "TopicContentTable", 
    "ChildContentTable", 
    "LikeLookupAndFeedTable", 
    "PinLookupAndFeedTable",

    "FollowerLookupAndFeedTable", 
    "FollowingLookupAndFeedTable", 
    
    "TopicRecentCommentFeedTable", 
    "CommentRecentReplyFeedTable",
    "UserRecentTopicFeedTable",
    "UserRecentCommentFeedTable",
    "UserRecentLikeFeedTable",
    "EveryoneRecentTopicFeedTable",
    "EveryoneRecentCommentFeedTable",

    "UserAccountTable",        
    "UsernameLookupTable",
    "UserEmailLookupTable",
    "UserPhoneNumberLookupTable",
    "UserThirdPartyAccountLookupTable",
    "UserSessionTable",

    "AppMetadataTable",
    "DeveloperAppFeedTable",

    "DeveloperAccountTable",
    "DeveloperSessionTable",
    "DeveloperEmailLookupTable",
      
    "UserActiveAppFeedTable",

    "NotificationLookupAndFeedTable",
    "UserActivityFeedTable",
    "UserMentionFeedTable",

    "ImageListTable",
     
    "PushNotificationRegistrationTable",

    "ReportContentLookupAndFeedTable",
    "ReportUserLookupAndFeedTable",
    "CVSSubmissionTable",

    "AppValidationConfigurationTable"
    )

foreach ($table in $tableNames) {

    Write-Host
    Write-Host
    Write-Host "*************************************************************"
    Write-Host "Copying Table $table"
    Write-Host "*************************************************************"

    $destTable = "https://$destAcctName.table.core.windows.net/$table"
    $sourceTable = "https://$sourceAcctName.table.core.windows.net/$table"

    # copy from Azure to the local directory
    & $azcopy /Source:$sourceTable /Dest:$localDir /SourceKey:$sourceKey /Z:$localDir\journal /V:"$localDir\${table}-export.log"

    $manifestPrefix = "${sourceAcctName}_${table}"
    $matchingFiles = Get-ChildItem $localDir -Filter "${manifestPrefix}_*.manifest" | Where-Object { $_.Attributes -ne "Directory" }

    if (($matchingfiles).Count -ne 1) {
        Write-Host "Wrong number of manifest files: count = $count"
        exit
    }

    $manifestName = ${matchingFiles}.Name

    write-host "manifest name is $manifestName"

    # copy from the local directory into new Azure location
    & $azcopy /Source:$localDir /Dest:$destTable /DestKey:$destKey /Manifest:$manifestName /EntityOperation:InsertOrReplace /Z:$localDir\journal /V:"$localDir\${table}-import.log"
}

