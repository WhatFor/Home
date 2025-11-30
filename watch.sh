#!/bin/bash

WATCH_DIR="./src"
RUN_CMD="dotnet run build.cs"

checksum=$(find "$WATCH_DIR" -type f -exec md5sum {} \; | md5sum)

echo "Watching $WATCH_DIR for changes..."
while true; do
    new_checksum=$(find "$WATCH_DIR" -type f -exec md5sum {} \; | md5sum)
    if [ "$new_checksum" != "$checksum" ]; then
        $RUN_CMD
        checksum=$new_checksum
    fi
    sleep 1
done
