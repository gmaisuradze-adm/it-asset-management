#!/bin/bash

# 🏥 Hospital IT Asset Tracker - ყოველდღიური მონიტორინგი
# Version: 1.0.0
# Created: 2025-06-18

PROJECT_DIR="/home/gadmin/it_system_new_gen"
MONITOR_DIR="$PROJECT_DIR/logs/monitoring"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
DATE_ONLY=$(date +%Y%m%d)

mkdir -p "$MONITOR_DIR"

# ფერების დაყენება
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🏥 Hospital IT Asset Tracker - ყოველდღიური მონიტორინგი${NC}"
echo "=================================================="
echo "📅 თარიღი: $(date '+%Y-%m-%d %H:%M:%S')"
echo ""

# 1. სისტემის ზოგადი მდგომარეობა
echo -e "${BLUE}🖥️  სისტემის ზოგადი მდგომარეობა${NC}"
echo "--------------------"

# CPU და Memory გამოყენება
CPU_USAGE=$(top -bn1 | grep "Cpu(s)" | awk '{print $2}' | cut -d'%' -f1)
MEMORY_USAGE=$(free | grep Mem | awk '{printf("%.2f", $3/$2 * 100.0)}')
DISK_USAGE=$(df -h / | awk 'NR==2 {print $5}' | cut -d'%' -f1)

echo "💻 CPU გამოყენება: ${CPU_USAGE}%"
echo "🧠 RAM გამოყენება: ${MEMORY_USAGE}%"
echo "💾 დისკის გამოყენება: ${DISK_USAGE}%"

if (( $(echo "$CPU_USAGE > 80" | bc -l) )); then
    echo -e "${RED}⚠️  მაღალი CPU გამოყენება!${NC}"
fi

if (( $(echo "$MEMORY_USAGE > 85" | bc -l) )); then
    echo -e "${RED}⚠️  მაღალი RAM გამოყენება!${NC}"
fi

if [ $DISK_USAGE -gt 85 ]; then
    echo -e "${RED}⚠️  მაღალი დისკის გამოყენება!${NC}"
fi

echo ""

# 2. პროექტის სტატუსი
echo -e "${BLUE}🚀 პროექტის სტატუსი${NC}"
echo "--------------------"

cd "$PROJECT_DIR"

# Build სტატუსი
if dotnet build --no-restore > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Build Status: წარმატებული${NC}"
    BUILD_OK=true
else
    echo -e "${RED}❌ Build Status: შეცდომა${NC}"
    BUILD_OK=false
fi

# სერვისის სტატუსი
if pgrep -f "dotnet.*HospitalAssetTracker" > /dev/null; then
    echo -e "${GREEN}✅ Service Status: გაშვებულია${NC}"
    SERVICE_OK=true
    
    # პროცესის ინფორმაცია
    PROCESS_PID=$(pgrep -f "dotnet.*HospitalAssetTracker")
    PROCESS_MEMORY=$(ps -p $PROCESS_PID -o rss= | awk '{print $1/1024}' | xargs printf "%.1f")
    echo "   📊 PID: $PROCESS_PID"
    echo "   🧠 Memory: ${PROCESS_MEMORY}MB"
else
    echo -e "${RED}❌ Service Status: არ მუშაობს${NC}"
    SERVICE_OK=false
fi

# პორტების შემოწმება
PORTS_STATUS=""
for port in 5001 5002 5003; do
    if netstat -tuln | grep ":$port " > /dev/null; then
        PORTS_STATUS="${PORTS_STATUS}${port}:✅ "
    else
        PORTS_STATUS="${PORTS_STATUS}${port}:❌ "
    fi
done
echo "🌐 Ports Status: $PORTS_STATUS"

echo ""

# 3. მონაცემთა ბაზის სტატუსი
echo -e "${BLUE}🗄️  მონაცემთა ბაზის სტატუსი${NC}"
echo "--------------------"

# PostgreSQL კავშირის შემოწმება
if pg_isready > /dev/null 2>&1; then
    echo -e "${GREEN}✅ PostgreSQL: მიერთებულია${NC}"
    DB_OK=true
    
    # Database-ის ზომა
    if command -v psql >/dev/null 2>&1; then
        DB_SIZE=$(psql -d postgres -t -c "SELECT pg_size_pretty(pg_database_size('postgres'));" 2>/dev/null | xargs)
        if [ ! -z "$DB_SIZE" ]; then
            echo "   📊 Database ზომა: $DB_SIZE"
        fi
    fi
else
    echo -e "${RED}❌ PostgreSQL: კავშირის შეცდომა${NC}"
    DB_OK=false
fi

echo ""

# 4. ლოგების ანალიზი
echo -e "${BLUE}📋 დღევანდელი ლოგების ანალიზი${NC}"
echo "--------------------"

# Error ლოგები
ERROR_COUNT=0
WARNING_COUNT=0
INFO_COUNT=0

# Application ლოგების შემოწმება
if pgrep -f "dotnet.*HospitalAssetTracker" > /dev/null; then
    # ვეძებთ დღევანდელ შეცდომებს
    if journalctl --since "today" | grep -i "hospitalassettracker" | grep -i "error" > /dev/null 2>&1; then
        ERROR_COUNT=$(journalctl --since "today" | grep -i "hospitalassettracker" | grep -i "error" | wc -l)
    fi
    
    if journalctl --since "today" | grep -i "hospitalassettracker" | grep -i "warning" > /dev/null 2>&1; then
        WARNING_COUNT=$(journalctl --since "today" | grep -i "hospitalassettracker" | grep -i "warning" | wc -l)
    fi
fi

echo "❌ Errors დღეს: $ERROR_COUNT"
echo "⚠️  Warnings დღეს: $WARNING_COUNT"

if [ $ERROR_COUNT -gt 0 ]; then
    echo -e "${RED}⚠️  შეცდომები ნაპოვნია - შეამოწმეთ ლოგები!${NC}"
fi

echo ""

# 5. ფაილების სისტემის შემოწმება
echo -e "${BLUE}📁 ფაილების სისტემის შემოწმება${NC}"
echo "--------------------"

# wwwroot ფაილები
WWWROOT_SIZE=$(du -sh "$PROJECT_DIR/wwwroot" 2>/dev/null | cut -f1 || echo "N/A")
echo "🌐 wwwroot ზომა: $WWWROOT_SIZE"

# Log ფაილები
LOG_SIZE=$(du -sh "$PROJECT_DIR/logs" 2>/dev/null | cut -f1 || echo "N/A")
echo "📄 ლოგების ზომა: $LOG_SIZE"

# ბექაპების რაოდენობა
BACKUP_COUNT=$(ls -1 /home/gadmin/backup/*.tar.gz 2>/dev/null | wc -l || echo "0")
echo "💾 ბექაპების რაოდენობა: $BACKUP_COUNT"

# ბოლო ბექაპი
if [ $BACKUP_COUNT -gt 0 ]; then
    LAST_BACKUP=$(ls -t /home/gadmin/backup/*.tar.gz 2>/dev/null | head -1)
    LAST_BACKUP_DATE=$(stat -c %y "$LAST_BACKUP" 2>/dev/null | cut -d' ' -f1)
    echo "📅 ბოლო ბექაპი: $LAST_BACKUP_DATE"
fi

echo ""

# 6. რეკომენდაციები
echo -e "${BLUE}💡 რეკომენდაციები${NC}"
echo "--------------------"

RECOMMENDATIONS=()

if [ "$BUILD_OK" = false ]; then
    RECOMMENDATIONS+=("🔧 გაუმჯობესების შესაძლებლობა: Build შეცდომების გასწორება")
fi

if [ "$SERVICE_OK" = false ]; then
    RECOMMENDATIONS+=("🚀 გაუმჯობესების შესაძლებლობა: სერვისის ხელახალი გაშვება")
fi

if [ "$DB_OK" = false ]; then
    RECOMMENDATIONS+=("🗄️  გაუმჯობესების შესაძლებლობა: PostgreSQL სერვისის შემოწმება")
fi

if [ $ERROR_COUNT -gt 5 ]; then
    RECOMMENDATIONS+=("📋 გაუმჯობესების შესაძლებლობა: მრავალი შეცდომა - ლოგების გაანალიზება")
fi

if [ $BACKUP_COUNT -lt 3 ]; then
    RECOMMENDATIONS+=("💾 გაუმჯობესების შესაძლებლობა: ბექაპების შექმნა")
fi

if [ ${#RECOMMENDATIONS[@]} -eq 0 ]; then
    echo -e "${GREEN}🎉 ყველაფერი კარგადაა! სისტემა სრულად ფუნქციონირებს.${NC}"
else
    for rec in "${RECOMMENDATIONS[@]}"; do
        echo -e "${YELLOW}$rec${NC}"
    done
fi

echo ""

# 7. ჯამური შეფასება
echo -e "${BLUE}🏥 ჯამური შეფასება${NC}"
echo "--------------------"

SCORE=0
MAX_SCORE=5

[ "$BUILD_OK" = true ] && ((SCORE++))
[ "$SERVICE_OK" = true ] && ((SCORE++))
[ "$DB_OK" = true ] && ((SCORE++))
[ $ERROR_COUNT -lt 5 ] && ((SCORE++))
[ $BACKUP_COUNT -gt 0 ] && ((SCORE++))

HEALTH_PERCENTAGE=$(( (SCORE * 100) / MAX_SCORE ))

echo "📊 სისტემის ქულა: $SCORE/$MAX_SCORE ($HEALTH_PERCENTAGE%)"

if [ $HEALTH_PERCENTAGE -ge 80 ]; then
    echo -e "${GREEN}🟢 სისტემის მუშაობა: შესანიშნავი${NC}"
    HEALTH_STATUS="EXCELLENT"
elif [ $HEALTH_PERCENTAGE -ge 60 ]; then
    echo -e "${YELLOW}🟡 სისტემის მუშაობა: კარგი${NC}"
    HEALTH_STATUS="GOOD"
else
    echo -e "${RED}🔴 სისტემის მუშაობა: მოითხოვს ყურადღებას${NC}"
    HEALTH_STATUS="NEEDS_ATTENTION"
fi

# ინფორმაციის შენახვა JSON ფორმატში
cat > "$MONITOR_DIR/daily_report_$DATE_ONLY.json" << EOF
{
    "date": "$(date -Iseconds)",
    "system": {
        "cpu_usage": "$CPU_USAGE",
        "memory_usage": "$MEMORY_USAGE",
        "disk_usage": "$DISK_USAGE"
    },
    "project": {
        "build_ok": $BUILD_OK,
        "service_ok": $SERVICE_OK,
        "db_ok": $DB_OK
    },
    "logs": {
        "errors": $ERROR_COUNT,
        "warnings": $WARNING_COUNT
    },
    "health": {
        "score": $SCORE,
        "max_score": $MAX_SCORE,
        "percentage": $HEALTH_PERCENTAGE,
        "status": "$HEALTH_STATUS"
    },
    "backups": {
        "count": $BACKUP_COUNT,
        "last_backup": "$LAST_BACKUP_DATE"
    }
}
EOF

echo ""
echo "📄 დეტალური რეპორტი შენახულია: $MONITOR_DIR/daily_report_$DATE_ONLY.json"
echo "🏁 მონიტორინგი დასრულდა: $(date '+%Y-%m-%d %H:%M:%S')"
echo "=================================================="
