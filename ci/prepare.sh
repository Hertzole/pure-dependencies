#!/bin/bash

# Check if nextVersion is provided
if [ -z "$1" ]; then
    echo "Usage: $0 <nextVersion> <branch>"
    exit 1
fi

# Check if branch is provided
if [ -z "$2" ]; then
    echo "Usage: $0 <nextVersion> <branch>"
    exit 1
fi

nextVersion=$1
branch=$2

echo "Updating version to $nextVersion"

# Echo all found files
echo "Found .csproj files:"
find . -type f -name "*.csproj"

# Check if branch is develop
if [ "$branch" == "develop" ]; then
    # Find and replace Version only
    # AssemblyVersion won't support the format x.x.x-develop.x
    find . -type f -name "*.csproj" -exec sed -i -e "s/<Version>.*<\/Version>/<Version>${nextVersion}<\/Version>/;" {} +
else
    # Find and replace Version, AssemblyVersion, and ProductVersion in all .csproj files
    find . -type f -name "*.csproj" -exec sed -i -e "s/<Version>.*<\/Version>/<Version>${nextVersion}<\/Version>/; s/<AssemblyVersion>.*<\/AssemblyVersion>/<AssemblyVersion>${nextVersion}<\/AssemblyVersion>/; s/<ProductVersion>.*<\/ProductVersion>/<ProductVersion>${nextVersion}<\/ProductVersion>/" {} +
fi

echo "Version updated to $nextVersion in all .csproj files."