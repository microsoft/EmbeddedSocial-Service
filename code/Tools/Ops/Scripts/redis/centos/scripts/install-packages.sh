#!/bin/sh

log_file_path="./install-packages.log"

{
echo ====================================================================
echo Output from install-packages script
date
echo ====================================================================

# install tcsh
echo ----------------------------------
echo Installing tcsh
echo ----------------------------------
sudo yum -y install tcsh
sudo chsh -s /bin/tcsh spadmin

# make epel packages available
echo ----------------------------------
echo Installing epel-release
echo ----------------------------------
sudo yum -y install epel-release

# install apg
echo ----------------------------------
echo Installing apg
echo ----------------------------------
sudo yum -y install apg

# install mg editor
echo ----------------------------------
echo Installing mg
echo ----------------------------------
sudo yum -y install mg

# install redis
echo ----------------------------------
echo Installing redis
echo ----------------------------------
sudo yum -y install redis

# install policycoreutils-python
echo ----------------------------------
echo Installing policycoreutils-python
echo ----------------------------------
sudo yum -y install policycoreutils-python

# install patch
echo ----------------------------------
echo Installing patch
echo ----------------------------------
sudo yum -y install patch

# install dos2unix
echo ----------------------------------
echo Installing dos2unix
echo ----------------------------------
sudo yum -y install dos2unix

# make sure packages are up to date
echo ----------------------------------
echo Installing package updates
echo ----------------------------------
sudo yum -y update
} >> $log_file_path 2>& 1

