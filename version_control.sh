#!/bin/bash

# 🏥 Hospital IT Asset Tracker - ვერსიების მართვის სისტემა
# Version: 1.0.0
# Created: 2025-06-18

PROJECT_DIR="/home/gadmin/it_system_new_gen"
VERSION_FILE="$PROJECT_DIR/version.json"
CHANGELOG_FILE="$PROJECT_DIR/CHANGELOG.md"

# მიმდინარე ვერსიის წაკითხვა
get_current_version() {
    if [ -f "$VERSION_FILE" ]; then
        cat "$VERSION_FILE" | grep -o '"version": *"[^"]*"' | cut -d'"' -f4
    else
        echo "1.2.3"
    fi
}

# ვერსიის განახლება
increment_version() {
    local version_type=$1
    local current_version=$(get_current_version)
    
    IFS='.' read -ra VERSION_PARTS <<< "$current_version"
    local major=${VERSION_PARTS[0]}
    local minor=${VERSION_PARTS[1]}
    local patch=${VERSION_PARTS[2]}
    
    case $version_type in
        "major")
            major=$((major + 1))
            minor=0
            patch=0
            ;;
        "minor")
            minor=$((minor + 1))
            patch=0
            ;;
        "patch"|*)
            patch=$((patch + 1))
            ;;
    esac
    
    echo "$major.$minor.$patch"
}

# ვერსიის ფაილის განახლება
update_version_file() {
    local new_version=$1
    local change_description=$2
    
    cat > "$VERSION_FILE" << EOF
{
    "version": "$new_version",
    "updated": "$(date -Iseconds)",
    "build": $(date +%s),
    "description": "$change_description",
    "previous_versions": [
        "1.2.3 - Asset Creation Guidelines UI Update",
        "1.2.2 - Dashboard Charts Implementation", 
        "1.2.1 - PostgreSQL Integration Complete",
        "1.2.0 - Core Asset Management Complete"
    ]
}
EOF
}

# CHANGELOG-ის განახლება
update_changelog() {
    local new_version=$1
    local change_description=$2
    
    # ახალი ჩანაწერის დამატება CHANGELOG-ის დასაწყისში
    if [ -f "$CHANGELOG_FILE" ]; then
        # ბექაპი
        cp "$CHANGELOG_FILE" "$CHANGELOG_FILE.bak"
        # ახალი ჩანაწერი
        {
            echo "# Hospital IT Asset Tracker - ცვლილებების ისტორია"
            echo ""
            echo "## v$new_version - $(date '+%Y-%m-%d')"
            echo "- $change_description"
            echo ""
            tail -n +4 "$CHANGELOG_FILE.bak"
        } > "$CHANGELOG_FILE"
    else
        # ახალი CHANGELOG ფაილი
        cat > "$CHANGELOG_FILE" << EOF
# Hospital IT Asset Tracker - ცვლილებების ისტორია

## v$new_version - $(date '+%Y-%m-%d')
- $change_description

## v1.2.3 - 2025-06-18
- Asset Creation Guidelines-ის მინიმალისტური UI განახლება
- ავტომატური progress tracking დამატება
- Bootstrap 5 კომპონენტების გაუმჯობესება

## v1.2.2 - 2025-06-17
- Dashboard-ზე Charts.js ინტეგრაცია
- Real-time მეტრიკების ვიზუალიზაცია
- Asset კატეგორიების ანალიტიკა

## v1.2.1 - 2025-06-16
- PostgreSQL მონაცემთა ბაზის სრული ინტეგრაცია
- Entity Framework Core მიგრაციები
- მონაცემთა უსაფრთხოების გაუმჯობესება

## v1.2.0 - 2025-06-15
- Asset Management-ის ძირითადი CRUD ოპერაციები
- ავტორიზაციისა და ავთენტიფიკაციის სისტემა
- QR კოდების გენერაცია Asset-ებისთვის
EOF
    fi
}

# ბექაპის შექმნა ვერსიის განახლებამდე
create_version_backup() {
    local version=$1
    local backup_dir="/home/gadmin/backup/versions"
    
    mkdir -p "$backup_dir"
    
    local backup_file="$backup_dir/version_v${version}_$(date +%Y%m%d_%H%M%S).tar.gz"
    
    cd "$PROJECT_DIR/.."
    tar -czf "$backup_file" "it_system_new_gen" 2>/dev/null
    
    echo "$backup_file"
}

# Git commit (თუ Git repository არსებობს)
git_commit_version() {
    local version=$1
    local change_description=$2
    
    cd "$PROJECT_DIR"
    
    if [ -d ".git" ]; then
        git add .
        git commit -m "v$version: $change_description" 2>/dev/null
        git tag "v$version" 2>/dev/null
        echo "Git commit შეიქმნა tag-ით v$version"
    else
        echo "Git repository არ ვიპოვნე - commit გამოტოვებულია"
    fi
}

# მთავარი ფუნქცია
main() {
    echo "🏥 Hospital IT Asset Tracker - ვერსიების მართვა"
    echo "=================================================="
    
    local action=$1
    local change_description=$2
    
    if [ -z "$action" ]; then
        echo "გამოყენება: $0 [patch|minor|major|show] [აღწერა]"
        echo ""
        echo "მაგალითები:"
        echo "  $0 patch 'Bug fixes და პატარა გაუმჯობესებები'"
        echo "  $0 minor 'ახალი ფუნქციონალობის დამატება'"
        echo "  $0 major 'დიდი ცვლილებები და API განახლებები'"
        echo "  $0 show"
        exit 1
    fi
    
    local current_version=$(get_current_version)
    
    case $action in
        "show")
            echo "📊 მიმდინარე ვერსია: v$current_version"
            if [ -f "$VERSION_FILE" ]; then
                echo "📅 განახლების თარიღი: $(cat "$VERSION_FILE" | grep -o '"updated": *"[^"]*"' | cut -d'"' -f4)"
                echo "📝 აღწერა: $(cat "$VERSION_FILE" | grep -o '"description": *"[^"]*"' | cut -d'"' -f4)"
            fi
            ;;
        "patch"|"minor"|"major")
            if [ -z "$change_description" ]; then
                echo "❌ ცვლილების აღწერა საჭიროა"
                exit 1
            fi
            
            local new_version=$(increment_version "$action")
            
            echo "📈 ვერსიის განახლება: v$current_version → v$new_version"
            echo "📝 ცვლილება: $change_description"
            echo ""
            
            # ბექაპის შექმნა
            echo "💾 ბექაპის შექმნა..."
            local backup_file=$(create_version_backup "$current_version")
            echo "✅ ბექაპი შეიქმნა: $backup_file"
            
            # ვერსიის ფაილის განახლება
            echo "📄 ვერსიის ფაილის განახლება..."
            update_version_file "$new_version" "$change_description"
            echo "✅ version.json განახლდა"
            
            # CHANGELOG-ის განახლება
            echo "📋 CHANGELOG-ის განახლება..."
            update_changelog "$new_version" "$change_description"
            echo "✅ CHANGELOG.md განახლდა"
            
            # Git commit
            echo "🔄 Git commit..."
            git_commit_version "$new_version" "$change_description"
            
            echo ""
            echo "🎉 ვერსია წარმატებით განახლდა v$new_version-ზე!"
            echo "📁 ფაილები განახლდა:"
            echo "   - $VERSION_FILE"
            echo "   - $CHANGELOG_FILE"
            echo "   - $backup_file"
            ;;
        *)
            echo "❌ უცნობი ოპერაცია: $action"
            echo "გამოყენება: $0 [patch|minor|major|show] [აღწერა]"
            exit 1
            ;;
    esac
}

# სქრიპტის გაშვება
main "$@"
