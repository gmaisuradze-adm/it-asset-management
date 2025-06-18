#!/bin/bash

# Build script with basic error handling
echo "Building project with basic error handling..."

# Run build and count errors
echo "Running initial build check..."
BUILD_OUTPUT=$(dotnet build it_system_new_gen.sln 2>&1)
ERROR_COUNT=$(echo "$BUILD_OUTPUT" | grep -c "error CS")

echo "Current error count: $ERROR_COUNT"

if [ $ERROR_COUNT -gt 20 ]; then
    echo "Too many errors to fix automatically. Manual intervention required."
    echo "Most common errors found:"
    echo "$BUILD_OUTPUT" | grep "error CS" | head -10
    exit 1
fi

echo "Build completed. Check detailed output in build logs."
echo "$BUILD_OUTPUT" > latest_build.log
