#!/bin/bash
# Auto Version Increment Script for Hospital Asset Tracker
# This script automatically increments the build version on each build

set -e

PROJECT_FILE="HospitalAssetTracker.csproj"
INCREMENT_TYPE="${1:-build}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

if [ ! -f "$PROJECT_FILE" ]; then
    echo -e "${RED}❌ Project file not found: $PROJECT_FILE${NC}"
    exit 1
fi

echo -e "${CYAN}🔄 Auto-incrementing version...${NC}"

# Extract current version using grep and sed
CURRENT_VERSION=$(grep -oP '<Version>\K[^<]+' "$PROJECT_FILE")

if [ -z "$CURRENT_VERSION" ]; then
    echo -e "${RED}❌ Version element not found in project file${NC}"
    exit 1
fi

echo -e "${YELLOW}📋 Current version: $CURRENT_VERSION${NC}"

# Parse version components
IFS='.' read -ra VERSION_PARTS <<< "$CURRENT_VERSION"
MAJOR=${VERSION_PARTS[0]:-0}
MINOR=${VERSION_PARTS[1]:-0}
PATCH=${VERSION_PARTS[2]:-0}
BUILD=${VERSION_PARTS[3]:-0}

# Increment version based on type
case "$INCREMENT_TYPE" in
    "major")
        MAJOR=$((MAJOR + 1))
        MINOR=0
        PATCH=0
        BUILD=0
        ;;
    "minor")
        MINOR=$((MINOR + 1))
        PATCH=0
        BUILD=0
        ;;
    "patch")
        PATCH=$((PATCH + 1))
        BUILD=0
        ;;
    "build"|*)
        BUILD=$((BUILD + 1))
        ;;
esac

# Format new version
if [ "$BUILD" -eq 0 ]; then
    NEW_VERSION="$MAJOR.$MINOR.$PATCH"
else
    NEW_VERSION="$MAJOR.$MINOR.$PATCH.$BUILD"
fi

ASSEMBLY_VERSION="$MAJOR.$MINOR.$PATCH.0"

echo -e "${GREEN}🆕 New version: $NEW_VERSION${NC}"

# Update version in project file using sed
sed -i.bak \
    -e "s|<Version>.*</Version>|<Version>$NEW_VERSION</Version>|" \
    -e "s|<AssemblyVersion>.*</AssemblyVersion>|<AssemblyVersion>$ASSEMBLY_VERSION</AssemblyVersion>|" \
    -e "s|<FileVersion>.*</FileVersion>|<FileVersion>$ASSEMBLY_VERSION</FileVersion>|" \
    "$PROJECT_FILE"

# Remove backup file
rm -f "${PROJECT_FILE}.bak"

echo -e "${GREEN}✅ Version updated successfully!${NC}"
echo -e "${GRAY}📁 Updated files: $PROJECT_FILE${NC}"

# Create version info file
BUILD_DATE=$(date -u +"%Y-%m-%d %H:%M:%S")
BUILD_MACHINE=$(hostname)
BUILD_USER=$(whoami)
GIT_COMMIT=$(git rev-parse HEAD 2>/dev/null || echo "Unknown")

cat > version.json << EOF
{
  "Version": "$NEW_VERSION",
  "BuildDate": "$BUILD_DATE",
  "BuildMachine": "$BUILD_MACHINE",
  "BuildUser": "$BUILD_USER",
  "GitCommit": "$GIT_COMMIT"
}
EOF

echo -e "${GRAY}📋 Version info saved to version.json${NC}"
