#!/bin/bash

set -ev

cp travis-build/.npmrc.template $HOME/.npmrc

for pkg in $PACKAGES
do
	echo "NPM Publish $pkg"
	pushd "$UNITY_PROJECT_PATH/$pkg"
	npm publish --access public
	popd
	echo "Done publishing $pkg"
done
