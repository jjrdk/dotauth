#!/usr/bin/env bash

export PATH="$PATH:~/.dotnet/tools"

dotnet tool install -g Cake.Tool

dotnet cake build.cake
