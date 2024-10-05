#!/bin/bash
SRC_DIR=$(dirname "$0")
DIST_DIR=$(dirname "$SRC_DIR")/dist
APP_NAME="$DIST_DIR/Nonius.app"
PUBLISH_OUTPUT_DIRECTORY="$SRC_DIR/Nonius/bin/osx-arm64/Release/net8.0/osx-arm64/publish"
# PUBLISH_OUTPUT_DIRECTORY should point to the output directory of your dotnet publish command.
# One example is /path/to/your/csproj/bin/Release/netcoreapp3.1/osx-x64/publish/.
# If you want to change output directories, add `--output /my/directory/path` to your `dotnet publish` command.
INFO_PLIST="$SRC_DIR/Info.plist"
ICON_FILE="nonius.icns"

if [ -d "$APP_NAME" ]
then
    rm -rf "$APP_NAME"
fi

mkdir -p "$DIST_DIR"
mkdir "$APP_NAME"

mkdir "$APP_NAME/Contents"
mkdir "$APP_NAME/Contents/MacOS"
mkdir "$APP_NAME/Contents/Resources"

cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "$SRC_DIR/Nonius/Assets/$ICON_FILE" "$APP_NAME/Contents/Resources/$ICON_FILE"
cp -a "$PUBLISH_OUTPUT_DIRECTORY/." "$APP_NAME/Contents/MacOS/"
