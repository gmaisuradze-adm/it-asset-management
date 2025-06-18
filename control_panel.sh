#!/bin/bash

# 🏥 Hospital IT Asset Tracker - მთავარი კონტროლის პანელი
# Version: 1.0.0
# Created: 2025-06-18

PROJECT_DIR="/home/gadmin/it_system_new_gen"

# ფერების დაყენება
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# ლოგოს ჩვენება
show_logo() {
    echo -e "${BLUE}"
    echo "  ╔══════════════════════════════════════════════════════════╗"
    echo "  ║                                                          ║"
    echo "  ║    🏥 HOSPITAL IT ASSET TRACKER - კონტროლის პანელი      ║"
    echo "  ║                                                          ║"
    echo "  ║    📊 პროექტის მართვა - ტესტირება - მონიტორინგი       ║"
    echo "  ║                                                          ║"
    echo "  ╚══════════════════════════════════════════════════════════╝"
    echo -e "${NC}"
    echo ""
}

# სწრაფი სტატუსი
show_quick_status() {
    echo -e "${CYAN}📋 სწრაფი მდგომარეობა:${NC}"
    echo "----------------------------------------"
    
    # ვერსია
    if [ -f "$PROJECT_DIR/version.json" ]; then
        VERSION=$(cat "$PROJECT_DIR/version.json" | grep -o '"version": *"[^"]*"' | cut -d'"' -f4)
        echo -e "${GREEN}📦 ვერსია: v$VERSION${NC}"
    else
        echo -e "${YELLOW}📦 ვერსია: განუსაზღვრელი${NC}"
    fi
    
    # სერვისის სტატუსი
    if pgrep -f "dotnet.*HospitalAssetTracker" > /dev/null; then
        echo -e "${GREEN}🚀 სერვისი: გაშვებულია${NC}"
    else
        echo -e "${RED}🚀 სერვისი: გაჩერებულია${NC}"
    fi
    
    # ბოლო ბექაპი
    if ls /home/gadmin/backup/*.tar.gz 1> /dev/null 2>&1; then
        LAST_BACKUP=$(ls -t /home/gadmin/backup/*.tar.gz | head -1)
        BACKUP_DATE=$(stat -c %y "$LAST_BACKUP" | cut -d' ' -f1)
        echo -e "${GREEN}💾 ბოლო ბექაპი: $BACKUP_DATE${NC}"
    else
        echo -e "${YELLOW}💾 ბექაპი: არ არსებობს${NC}"
    fi
    
    echo ""
}

# მენიუს ჩვენება
show_menu() {
    echo -e "${PURPLE}🎯 მთავარი მენიუ:${NC}"
    echo "----------------------------------------"
    echo "1. 🧪 ავტომატური ტესტირება"
    echo "2. 📊 ყოველდღიური მონიტორინგი"
    echo "3. 📈 ვერსიების მართვა"
    echo "4. 💾 ბექაპის შექმნა"
    echo "5. 🚀 სერვისის მართვა"
    echo "6. 📋 ლოგების ნახვა"
    echo "7. 🔧 პროექტის Build & Run"
    echo "8. 📊 დეტალური სტატუსი"
    echo "9. 🆘 დახმარება"
    echo "0. 🚪 გასვლა"
    echo ""
}

# ავტომატური ტესტირება
run_automated_test() {
    echo -e "${BLUE}🧪 ავტომატური ტესტირების გაშვება...${NC}"
    echo "======================================"
    
    if [ -f "$PROJECT_DIR/automated_test.sh" ]; then
        "$PROJECT_DIR/automated_test.sh"
    else
        echo -e "${RED}❌ ავტომატური ტესტის სქრიპტი ვერ ვიპოვნე${NC}"
    fi
    
    echo ""
    read -p "Enter-ის დასაწყისად..." -r
}

# ყოველდღიური მონიტორინგი
run_daily_monitoring() {
    echo -e "${BLUE}📊 ყოველდღიური მონიტორინგის გაშვება...${NC}"
    echo "======================================"
    
    if [ -f "$PROJECT_DIR/daily_monitoring.sh" ]; then
        "$PROJECT_DIR/daily_monitoring.sh"
    else
        echo -e "${RED}❌ მონიტორინგის სქრიპტი ვერ ვიპოვნე${NC}"
    fi
    
    echo ""
    read -p "Enter-ის დასაწყისად..." -r
}

# ვერსიების მართვა
manage_versions() {
    echo -e "${BLUE}📈 ვერსიების მართვა${NC}"
    echo "======================================"
    
    if [ -f "$PROJECT_DIR/version_control.sh" ]; then
        echo "1. 📊 მიმდინარე ვერსიის ჩვენება"
        echo "2. 🔧 Patch ვერსიის განახლება"
        echo "3. ⚡ Minor ვერსიის განახლება"
        echo "4. 🚀 Major ვერსიის განახლება"
        echo "5. 🔙 უკან დაბრუნება"
        echo ""
        
        read -p "აირჩიეთ ოფცია (1-5): " version_choice
        
        case $version_choice in
            1)
                "$PROJECT_DIR/version_control.sh" show
                ;;
            2)
                read -p "Patch ვერსიის აღწერა: " description
                "$PROJECT_DIR/version_control.sh" patch "$description"
                ;;
            3)
                read -p "Minor ვერსიის აღწერა: " description
                "$PROJECT_DIR/version_control.sh" minor "$description"
                ;;
            4)
                read -p "Major ვერსიის აღწერა: " description
                "$PROJECT_DIR/version_control.sh" major "$description"
                ;;
            5)
                return
                ;;
            *)
                echo -e "${RED}❌ არასწორი არჩევანი${NC}"
                ;;
        esac
    else
        echo -e "${RED}❌ ვერსიების მართვის სქრიპტი ვერ ვიპოვნე${NC}"
    fi
    
    echo ""
    read -p "Enter-ის დასაწყისად..." -r
}

# ბექაპის შექმნა
create_backup() {
    echo -e "${BLUE}💾 ბექაპის შექმნა...${NC}"
    echo "======================================"
    
    TIMESTAMP=$(date +%Y%m%d_%H%M%S)
    BACKUP_DIR="/home/gadmin/backup"
    BACKUP_FILE="$BACKUP_DIR/manual_backup_$TIMESTAMP.tar.gz"
    
    mkdir -p "$BACKUP_DIR"
    
    echo "📦 ბექაპის შექმნა მიმდინარეობს..."
    
    if tar -czf "$BACKUP_FILE" "$PROJECT_DIR" 2>/dev/null; then
        echo -e "${GREEN}✅ ბექაპი წარმატებით შეიქმნა${NC}"
        echo "📁 ფაილი: $BACKUP_FILE"
        
        # ბექაპის ზომა
        BACKUP_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
        echo "📊 ზომა: $BACKUP_SIZE"
    else
        echo -e "${RED}❌ ბექაპის შექმნის შეცდომა${NC}"
    fi
    
    echo ""
    read -p "Enter-ის დასაწყისად..." -r
}

# სერვისის მართვა
manage_service() {
    echo -e "${BLUE}🚀 სერვისის მართვა${NC}"
    echo "======================================"
    
    if pgrep -f "dotnet.*HospitalAssetTracker" > /dev/null; then
        echo -e "${GREEN}✅ სერვისი ამჟამად გაშვებულია${NC}"
        echo ""
        echo "1. 🛑 სერვისის გაჩერება"
        echo "2. 🔄 სერვისის ხელახალი გაშვება"
        echo "3. 🔙 უკან დაბრუნება"
        
        read -p "აირჩიეთ ოფცია (1-3): " service_choice
        
        case $service_choice in
            1)
                echo "🛑 სერვისის გაჩერება..."
                pkill -f "dotnet.*HospitalAssetTracker"
                sleep 2
                if ! pgrep -f "dotnet.*HospitalAssetTracker" > /dev/null; then
                    echo -e "${GREEN}✅ სერვისი წარმატებით გაჩერდა${NC}"
                else
                    echo -e "${RED}❌ სერვისის გაჩერების შეცდომა${NC}"
                fi
                ;;
            2)
                echo "🔄 სერვისის ხელახალი გაშვება..."
                pkill -f "dotnet.*HospitalAssetTracker"
                sleep 2
                cd "$PROJECT_DIR"
                nohup dotnet run > /dev/null 2>&1 &
                sleep 3
                if pgrep -f "dotnet.*HospitalAssetTracker" > /dev/null; then
                    echo -e "${GREEN}✅ სერვისი წარმატებით ხელახლა გაეშვა${NC}"
                else
                    echo -e "${RED}❌ სერვისის გაშვების შეცდომა${NC}"
                fi
                ;;
            3)
                return
                ;;
        esac
    else
        echo -e "${RED}❌ სერვისი ამჟამად გაჩერებულია${NC}"
        echo ""
        echo "1. 🚀 სერვისის გაშვება"
        echo "2. 🔙 უკან დაბრუნება"
        
        read -p "აირჩიეთ ოფცია (1-2): " service_choice
        
        case $service_choice in
            1)
                echo "🚀 სერვისის გაშვება..."
                cd "$PROJECT_DIR"
                nohup dotnet run > /dev/null 2>&1 &
                sleep 3
                if pgrep -f "dotnet.*HospitalAssetTracker" > /dev/null; then
                    echo -e "${GREEN}✅ სერვისი წარმატებით გაეშვა${NC}"
                    echo "🌐 ხელმისაწვდომია: http://localhost:5001"
                else
                    echo -e "${RED}❌ სერვისის გაშვების შეცდომა${NC}"
                fi
                ;;
            2)
                return
                ;;
        esac
    fi
    
    echo ""
    read -p "Enter-ის დასაწყისად..." -r
}

# ლოგების ნახვა
view_logs() {
    echo -e "${BLUE}📋 ლოგების ნახვა${NC}"
    echo "======================================"
    
    echo "1. 🧪 ტესტირების ლოგები"
    echo "2. 📊 მონიტორინგის ლოგები"
    echo "3. 🔨 Build ლოგები"
    echo "4. 📱 Application ლოგები"
    echo "5. 🔙 უკან დაბრუნება"
    
    read -p "აირჩიეთ ოფცია (1-5): " log_choice
    
    case $log_choice in
        1)
            echo "🧪 ტესტირების ლოგები:"
            if ls "$PROJECT_DIR/logs/test/"*.txt 1> /dev/null 2>&1; then
                echo "📄 ბოლო ტესტის ლოგი:"
                tail -20 "$(ls -t "$PROJECT_DIR/logs/test/"*.txt | head -1)"
            else
                echo "ლოგები ვერ ვიპოვნე"
            fi
            ;;
        2)
            echo "📊 მონიტორინგის ლოგები:"
            if ls "$PROJECT_DIR/logs/monitoring/"*.json 1> /dev/null 2>&1; then
                echo "📄 ბოლო მონიტორინგის რეპორტი:"
                cat "$(ls -t "$PROJECT_DIR/logs/monitoring/"*.json | head -1)" | jq '.' 2>/dev/null || cat "$(ls -t "$PROJECT_DIR/logs/monitoring/"*.json | head -1)"
            else
                echo "ლოგები ვერ ვიპოვნე"
            fi
            ;;
        3)
            echo "🔨 Build ლოგები:"
            if ls "$PROJECT_DIR/logs/test/build_"*.log 1> /dev/null 2>&1; then
                tail -20 "$(ls -t "$PROJECT_DIR/logs/test/build_"*.log | head -1)"
            else
                echo "ლოგები ვერ ვიპოვნე"
            fi
            ;;
        4)
            echo "📱 Application ლოგები:"
            journalctl --since "today" | grep -i "hospitalassettracker" | tail -10
            ;;
        5)
            return
            ;;
    esac
    
    echo ""
    read -p "Enter-ის დასაწყისად..." -r
}

# Build & Run
build_and_run() {
    echo -e "${BLUE}🔧 პროექტის Build & Run${NC}"
    echo "======================================"
    
    cd "$PROJECT_DIR"
    
    echo "🔨 Build-ის შესრულება..."
    if dotnet build; then
        echo -e "${GREEN}✅ Build წარმატებული${NC}"
        echo ""
        echo "🚀 სერვისის გაშვება..."
        
        # ძველი პროცესის გაჩერება
        pkill -f "dotnet.*HospitalAssetTracker" 2>/dev/null
        sleep 2
        
        # ახალი პროცესის გაშვება
        dotnet run &
        sleep 3
        
        if pgrep -f "dotnet.*HospitalAssetTracker" > /dev/null; then
            echo -e "${GREEN}✅ სერვისი წარმატებით გაეშვა${NC}"
            echo "🌐 ხელმისაწვდომია:"
            echo "   - http://localhost:5001"
            echo "   - http://localhost:5002"
            echo "   - http://localhost:5003"
        else
            echo -e "${RED}❌ სერვისის გაშვების შეცდომა${NC}"
        fi
    else
        echo -e "${RED}❌ Build-ის შეცდომა${NC}"
    fi
    
    echo ""
    read -p "Enter-ის დასაწყისად..." -r
}

# დეტალური სტატუსი
detailed_status() {
    echo -e "${BLUE}📊 დეტალური სტატუსი${NC}"
    echo "======================================"
    
    # პროექტის ინფორმაცია
    echo -e "${CYAN}📦 პროექტის ინფორმაცია:${NC}"
    echo "  📁 დირექტორია: $PROJECT_DIR"
    if [ -f "$PROJECT_DIR/version.json" ]; then
        VERSION=$(cat "$PROJECT_DIR/version.json" | grep -o '"version": *"[^"]*"' | cut -d'"' -f4)
        echo "  📦 ვერსია: v$VERSION"
    fi
    echo ""
    
    # ფაილების რაოდენობა
    echo -e "${CYAN}📄 ფაილების სტატისტიკა:${NC}"
    echo "  📋 .cs ფაილები: $(find "$PROJECT_DIR" -name "*.cs" | wc -l)"
    echo "  🌐 .cshtml ფაილები: $(find "$PROJECT_DIR" -name "*.cshtml" | wc -l)"
    echo "  📄 ჯამური ფაილები: $(find "$PROJECT_DIR" -type f | wc -l)"
    echo ""
    
    # ბექაპების ინფორმაცია
    echo -e "${CYAN}💾 ბექაპების ინფორმაცია:${NC}"
    BACKUP_COUNT=$(ls -1 /home/gadmin/backup/*.tar.gz 2>/dev/null | wc -l)
    echo "  📊 ბექაპების რაოდენობა: $BACKUP_COUNT"
    if [ $BACKUP_COUNT -gt 0 ]; then
        TOTAL_BACKUP_SIZE=$(du -sh /home/gadmin/backup/ | cut -f1)
        echo "  📦 ჯამური ზომა: $TOTAL_BACKUP_SIZE"
    fi
    echo ""
    
    # სისტემის რესურსები
    echo -e "${CYAN}🖥️  სისტემის რესურსები:${NC}"
    echo "  💻 CPU: $(top -bn1 | grep "Cpu(s)" | awk '{print $2}')"
    echo "  🧠 RAM: $(free -h | grep Mem | awk '{print $3 "/" $2}')"
    echo "  💾 Disk: $(df -h / | awk 'NR==2 {print $3 "/" $2 " (" $5 ")"}')"
    
    echo ""
    read -p "Enter-ის დასაწყისად..." -r
}

# დახმარება
show_help() {
    echo -e "${BLUE}🆘 დახმარება${NC}"
    echo "======================================"
    echo ""
    echo -e "${CYAN}🎯 ამ კონტროლის პანელის შესაძლებლობები:${NC}"
    echo ""
    echo "1. 🧪 ავტომატური ტესტირება - სისტემის ჯანმრთელობის შემოწმება"
    echo "2. 📊 ყოველდღიური მონიტორინგი - დეტალური მონიტორინგი"
    echo "3. 📈 ვერსიების მართვა - ვერსიების განახლება და ისტორია"
    echo "4. 💾 ბექაპის შექმნა - პროექტის ბექაპი"
    echo "5. 🚀 სერვისის მართვა - ASP.NET Core სერვისის მართვა"
    echo "6. 📋 ლოგების ნახვა - სხვადასხვა ლოგების ნახვა"
    echo "7. 🔧 Build & Run - პროექტის აწყობა და გაშვება"
    echo "8. 📊 დეტალური სტატუსი - სრული ინფორმაცია"
    echo ""
    echo -e "${CYAN}🔧 ბრძანებები ტერმინალში:${NC}"
    echo "  ./automated_test.sh - ავტომატური ტესტი"
    echo "  ./daily_monitoring.sh - მონიტორინგი"
    echo "  ./version_control.sh show - ვერსიის ჩვენება"
    echo "  ./version_control.sh patch \"აღწერა\" - ვერსიის განახლება"
    echo ""
    echo -e "${CYAN}📁 მნიშვნელოვანი ფაილები:${NC}"
    echo "  PROJECT_TESTING_FRAMEWORK.md - ტესტირების ჩარჩო"
    echo "  logs/test/ - ტესტირების ლოგები"
    echo "  logs/monitoring/ - მონიტორინგის ლოგები"
    echo "  /home/gadmin/backup/ - ბექაპები"
    echo ""
    
    read -p "Enter-ის დასაწყისად..." -r
}

# მთავარი ფუნქცია
main() {
    while true; do
        clear
        show_logo
        show_quick_status
        show_menu
        
        read -p "აირჩიეთ ოფცია (0-9): " choice
        
        case $choice in
            1)
                run_automated_test
                ;;
            2)
                run_daily_monitoring
                ;;
            3)
                manage_versions
                ;;
            4)
                create_backup
                ;;
            5)
                manage_service
                ;;
            6)
                view_logs
                ;;
            7)
                build_and_run
                ;;
            8)
                detailed_status
                ;;
            9)
                show_help
                ;;
            0)
                echo -e "${GREEN}👋 ნახვამდის!${NC}"
                exit 0
                ;;
            *)
                echo -e "${RED}❌ არასწორი არჩევანი. სცადეთ თავიდან.${NC}"
                sleep 2
                ;;
        esac
    done
}

# პროგრამის გაშვება
main
