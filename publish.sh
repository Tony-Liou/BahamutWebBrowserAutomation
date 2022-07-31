#!/bin/bash
# Purpose: Publish the app to Ubuntu 22.04 in x86-64
# Usage: ./publish.sh
# For more information, see: https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish
dotnet publish -c Release -r ubuntu.22.04-x64 -p:PublishReadyToRun=true