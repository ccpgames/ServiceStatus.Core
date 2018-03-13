#!/bin/sh
set -e

NUGET_API_KEY=$1
NUGET_SOURCE=$2

PACKAGE_FILES=$(find ./nuget -type f -name "*.nupkg")
NUPKG_FILES=$(echo "$PACKAGE_FILES" | grep -Po '(?i)\K([a-zA-Z0-9-.]+).nupkg')

# Loop through the different packages that have been updated
# and create a nuget package to be deployed
echo "$NUPKG_FILES" | while read -r packageName; do
    echo "Package: $packageName"
    nuget push $packageName -Verbosity detailed -ApiKey $NUGET_API_KEY -Source $NUGET_SOURCE
done