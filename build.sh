#!/usr/bin/env bash

# this script is introduced to collect sdk source file from each engine implementation
# so that you don't need to copy the folder manually.
cp -r cocos2dx-js/sdk/ sdk/js/
cp -r Unity3D/sdk/ sdk/csharp/
