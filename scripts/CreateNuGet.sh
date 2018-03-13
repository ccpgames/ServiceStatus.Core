#!/bin/sh
set -e

# Find all the GIT changes
# GIT_CHANGES=$(git diff --name-only HEAD HEAD~10)
GIT_CHANGES=$(git diff --name-only $TRAVIS_COMMIT_RANGE)

# Get the folder names for the changes that have occurred
LIST_CHANGES=$(echo "$GIT_CHANGES" | grep -Po '(?i)src/\K([a-zA-Z0-9-.]+)(?=/.*)')

# Reduce the list to unique changes
LIST_UNIQUE=$(echo "$LIST_CHANGES" | uniq)

# Enter the source folder
cd "src"

# Create a default version number
PROJECT_VERSION=$(echo 1.0.$(date +"%y%m").$(date +"%d")$(echo -n $TRAVIS_BUILD_NUMBER | tail -c 2))

# Look for version tags
while getopts :v: option
do 
    case "$option" in
    v)
        # Validate that inserted value is a valid version string
        VERSION_VALIDATOR_REGEX='^([0-9]+\.){0,3}(\*|[0-9]+)(-[a-zA-Z0-9]+){0,1}$'
        if [[ $OPTARG =~ $VERSION_VALIDATOR_REGEX ]]
        then
            PROJECT_VERSION=$OPTARG
        fi
        ;;
    *)
        echo "Received invalid option as an argument: "
        echo $OPTARG
        PROCEED="false"
        ;;
        esac
done

# Loop through the different packages that have been updated
# and create a nuget package to be deployed
echo "$LIST_UNIQUE" | while read -r dirName; do
    cd "$dirName"
    dotnet pack -o ../../nuget -c Release //p:Version=$PROJECT_VERSION
    cd ..
done
