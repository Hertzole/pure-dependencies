#!/bin/bash

version=$1
branch=$2

if [ "$branch" == "develop" ]; then
  # Convert version to the desired format
  version=$(echo $version | sed 's/-develop\././')
fi

echo $version