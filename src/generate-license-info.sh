#!/bin/bash
SRC_DIR=$(dirname "$0")
ASSETS_DIR="$SRC_DIR/Nonius/Assets"
JSON_PATH="$ASSETS_DIR/licenses.json"
CSPROJ_PATH="$SRC_DIR/Nonius/Nonius.csproj"

dotnet tool run nuget-license -- -i $CSPROJ_PATH -o JsonPretty > $JSON_PATH
