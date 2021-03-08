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
  sed -i '' "s/\"version\": \"[0-9]*\.[0-9]*\.[0-9]*\"/\"version\": \"$VERSION\"/" ./IPTech-Coroutines/Packages/IPTech.Coroutines/package.json
}

function update_readme_settings() {
  echo Updating version in IPTech.Coroutines README
  sed -i '' "s/\"com.iptech.coroutines\": \"[0-9]*\.[0-9]*\.[0-9]*\"/\"com.iptech.coroutines\": \"$VERSION\"/" ./IPTech-Coroutines/Packages/IPTech.Coroutines/README.md

  cp ./IPTech-Coroutines/Packages/IPTech.Coroutines/README.md ./IPTech-Coroutines/Packages/IPTech.Coroutines/Documentation~/README.md
  cp ./IPTech-Coroutines/Packages/IPTech.Coroutines/README.md README.md
}

check_for_valid_arguments
update_project_settings
update_package_settings
update_readme_settings

echo Version Updated
