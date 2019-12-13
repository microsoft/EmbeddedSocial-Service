#!/bin/bash

#
# Simple script that inserts new entries into an .sln file for a new
# dev. The script should be much less tedious to use than copying and
# pasting new entries for 40-something projects.
#
# To maintain alphabetical order, choose the existing dev whose name
# comes just before yours. For example, to insert entries for new dev
# "landon" between entries for existing devs "eduardo" and "sharad"
# use "eduardo" as the existing dev and "landon" as the new dev as
# command line arguments.
#
# It's best to be cautious when using the script and perform several
# sanity checks before overwriting the actual .sln file. Here's a
# typical sequence of commands from a bash shell:
#
# $> cd code
# $> cp SocialPlus.sln SocialPlus.sln.bak
# $> Tools/Ops/Scripts/insert-new-config-to-sln.sh landon eduardo ./SocialPlus.sln.bak
#
# Eyeball the output to make sure that it looks ok. If it looks ok,
# then redirect the output to a new file and run unix2dos on the new
# file:
#
# $> Tools/Ops/Scripts/insert-new-config-to-sln.sh landon eduardo ./SocialPlus.sln.bak > ./SocialPlus.sln.new
# $> unix2dos ./SocialPlus.sln.new
#
# Take a look at the new file to make sure that it looks ok. If it
# looks ok, then exit VisualStudio and replace the existing .sln with
# the new one:
#
# $> cp ./SocialPlus.sln.new ./SocialPlus.sln
#

if [ $# -lt 3 ]; then 
    echo "usage: $0 <new dev name> <existing dev name> <.sln file>"
    exit
fi

# new dev we want to add to the solution file
NEW_DEV=$1
# existing dev we'll use a template for inserting our new dev
EXISTING_DEV=$2
# name of VS solution file
SOLUTION_FILE=$3

if [ ! -f $SOLUTION_FILE ]; then
    echo "Could not find $SOLUTION_FILE."
    exit
fi

# array where we'll buffer new-dev lines until they're ready to print
declare -a new_dev_lines=()

# loop through the lines in the solution file
while IFS='' read -r line || [[ -n $line ]]; do
    # Check if the line contains our existing dev and buffer a
    # corresponding line that includes the new dev. Otherwise, print
    # any buffered lines and then clear the buffered lines.    
    if [[ $line == *$EXISTING_DEV* ]]; then
	new_dev_line=`echo $line | sed -e "s/$EXISTING_DEV/$NEW_DEV/g"`
	index=${#new_dev_lines[*]}
	new_dev_lines[$index]="$new_dev_line"
    else
	# dump any buffered lines for the new dev
	# tabs are an ugly indentation hack
	for ((i=0; i<${#new_dev_lines[@]}; i++)) do
	    printf '\t\t%s\n' "${new_dev_lines[$i]}"
	done
	# clear the array of buffered lines
	unset new_dev_lines
    fi
    # print the current line
    echo "$line"
done < "$SOLUTION_FILE"
