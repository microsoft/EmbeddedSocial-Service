#
# GenerateBuildInfo.ps1
#

param($ProjectDir)

$found_git = $false
$ResourceDir = "${ProjectDir}Resources"

# find git.exe
$possibleGitLocations = `
    "${env:programfiles(x86)}\Git\bin\git.exe",` 
    "${env:programfiles(x86)}\Git\cmd\git.exe",` 
    "${env:programfiles}\Git\bin\git.exe",` 
    "${env:programfiles}\Git\cmd\git.exe",` 
    "${env:programW6432}\Git\bin\git.exe",` 
    "${env:programW6432}\Git\cmd\git.exe" 

# check each location
foreach ($gitLocation in $possibleGitLocations) {
    If (Test-Path $gitLocation -PathType Leaf) {
        $git = $gitLocation
        $found_git = $true
        break
    }
}

# If still not found, check if in path
If (!$found_git) {
    if (Get-Command "git.exe" -ErrorAction SilentlyContinue) {
    $git = "git.exe"
    $found_git = $true
    }
    # otherwise, we were unable to find git. throw error
    else {
        Write-Error "Unable to locate git.exe on your system. Fix this by adding git.exe to your path."
        exit 1
    }
}

# Print the current branch name
$bname = & $git rev-parse --abbrev-ref HEAD
$bname | Out-File -FilePath ${ResourceDir}\BuildInfoBranchName.txt

# Print hash of current commit
$info = & $git rev-parse HEAD
$info | Out-File -FilePath ${ResourceDir}\BuildInfoCommitHash.txt

# Print the current date
Get-Date -Format G > ${ResourceDir}\BuildInfoDate.txt

# Print the dirty files in a format called "porcelain format"
$dirty = & $git status -z --porcelain
$dirty | Out-File -FilePath ${ResourceDir}\BuildInfoDirtyFiles.txt

# Print the current user and hostname
$username = [environment]::UserName
$hostname = ([environment]::MachineName).ToLower()
Write-Output "$username@$hostname" > ${ResourceDir}\BuildInfoHostname.txt

# Print the current tag name
# If commit does not correspond to any tags, this file will be empty
($tag = & $git describe --exact-match --tags) 2> $null
$tag | Out-File -FilePath ${ResourceDir}\BuildInfoTag.txt
