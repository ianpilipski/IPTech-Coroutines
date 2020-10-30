#!/usr/bin/env bash

set -e

VERSION="$1"

function check_for_valid_arguments() {
  rx='^([0-9]+\.){2}([0-9]+)$'
  if [[ $VERSION =~ $rx ]]; then
    echo Changing version to $VERSION
  else
    echo ERROR: must supply a valid version in the form of x.x.x as the first argument.
    exit 1
  fi
}

function update_project_settings() {
  echo Updating version in ProjectSettings
  sed -i '' "s/bundleVersion:.*/bundleVersion: $VERSION/" ./IPTech-Coroutines/ProjectSettings/ProjectSettings.asset
}

function update_package_settings() {
  echo Updating version in IPTech.Coroutines package
  sed -i '' "s/\"version\": \"1\.1\.1\"/\"version\": \"$VERSION\"/" ./IPTech-Coroutines/Assets/IPTech.Coroutines/package.json

  echo Updating version in IPTech.Coroutines.Examples package
  sed -i '' "s/\"version\": \"1\.1\.1\"/\"version\": \"$VERSION\"/" ./IPTech-Coroutines/Assets/IPTech.Coroutines.Examples/package.json
}

check_for_valid_arguments
update_project_settings
update_package_settings

echo Version Updated
