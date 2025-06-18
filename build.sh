#!/bin/bash
# Build script with automatic version increment
# Usage: ./build.sh [increment_type] [configuration]
# increment_type: major, minor, patch, build (default: build)
# configuration: Debug, Release (default: Release)

set -e

INCREMENT_TYPE="${1:-build}"
CONFIGURATION="${2:-Release}"

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

echo -e "${CYAN}🏗️  Building Hospital Asset Tracker...${NC}"
echo -e "${YELLOW}📋 Configuration: $CONFIGURATION${NC}"
echo -e "${YELLOW}📈 Version increment: $INCREMENT_TYPE${NC}"

# Clean previous builds
echo -e "${CYAN}🧹 Cleaning previous builds...${NC}"
/home/gadmin/.dotnet/dotnet clean it_system_new_gen.sln --configuration $CONFIGURATION

# Increment version (only for Release builds or if explicitly requested)
if [ "$CONFIGURATION" = "Release" ] || [ "$3" = "--increment-version" ]; then
    echo -e "${CYAN}🔢 Incrementing version...${NC}"
    ./increment-version.sh $INCREMENT_TYPE
fi

# Restore packages
echo -e "${CYAN}📦 Restoring packages...${NC}"
/home/gadmin/.dotnet/dotnet restore it_system_new_gen.sln

# Build the application
echo -e "${CYAN}🔨 Building application...${NC}"
/home/gadmin/.dotnet/dotnet build it_system_new_gen.sln --configuration $CONFIGURATION --no-restore

# Run tests if they exist
if [ -d "Tests" ] || [ -f "*.Test.csproj" ]; then
    echo -e "${CYAN}🧪 Running tests...${NC}"
    /home/gadmin/.dotnet/dotnet test it_system_new_gen.sln --configuration $CONFIGURATION --no-build
fi

echo -e "${GREEN}✅ Build completed successfully!${NC}"

# Show version information
if [ -f "version.json" ]; then
    echo -e "${YELLOW}📋 Build Information:${NC}"
    echo -e "${YELLOW}$(cat version.json)${NC}"
fi
