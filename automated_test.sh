#!/bin/bash

# 🏥 Hospital IT Asset Tracker - ავტომატური ტესტირების სისტემა
# Version: 1.0.0
# Created: 2025-06-18

echo "🏥 Hospital IT Asset Tracker - ავტომატური ტესტი დაიწყო..."
echo "📅 დრო: $(date '+%Y-%m-%d %H:%M:%S')"
echo "=================================================="

# ცვლადების განსაზღვრა
PROJECT_DIR="/home/gadmin/it_system_new_gen"
LOG_DIR="$PROJECT_DIR/logs/test"
BACKUP_DIR="/home/gadmin/backup"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

# ლოგების დირექტორიის შექმნა
mkdir -p "$LOG_DIR"
mkdir -p "$BACKUP_DIR"

LOG_FILE="$LOG_DIR/test_log_$TIMESTAMP.txt"

# ლოგირების ფუნქცია
log_message() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

log_success() {
    echo "✅ $(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

log_warning() {
    echo "⚠️  $(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

log_error() {
    echo "❌ $(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

# პროექტის ბილდის შემოწმება
echo "🔨 პროექტის ბილდის შემოწმება..."
cd "$PROJECT_DIR"

# Build-ის შემოწმება (მხოლოდ compilation, არა restore)
if dotnet build --no-restore --verbosity quiet > "$LOG_DIR/build_$TIMESTAMP.log" 2>&1; then
    log_success "პროექტი წარმატებით აიწყო"
    BUILD_SUCCESS=true
else
    # თუ --no-restore არ მუშაობს, სცადოთ ჩვეულებრივი build
    if dotnet build --verbosity quiet > "$LOG_DIR/build_$TIMESTAMP.log" 2>&1; then
        log_success "პროექტი წარმატებით აიწყო"
        BUILD_SUCCESS=true
    else
        log_error "პროექტის ბილდის შეცდომა - იხილეთ $LOG_DIR/build_$TIMESTAMP.log"
        BUILD_SUCCESS=false
    fi
fi

# Database კავშირის შემოწმება
echo "🗃️  მონაცემთა ბაზის კავშირის შემოწმება..."
if dotnet ef database drop --force --dry-run > /dev/null 2>&1; then
    log_success "მონაცემთა ბაზის კავშირი აქტიურია"
    DB_CONNECTION=true
else
    log_warning "მონაცემთა ბაზის კავშირის პრობლემა"
    DB_CONNECTION=false
fi

# სერვისების შემოწმება
echo "🔧 ASP.NET Core სერვისების შემოწმება..."
if pgrep -f "HospitalAssetTracker" > /dev/null; then
    log_success "ASP.NET Core სერვისი გაშვებულია"
    SERVICE_RUNNING=true
else
    log_warning "ASP.NET Core სერვისი არ მუშაობს"
    SERVICE_RUNNING=false
fi

# პორტების შემოწმება
echo "🌐 პორტების შემოწმება..."
PORTS_OK=true
for port in 5001 5002 5003; do
    if ss -tuln | grep ":$port " > /dev/null; then
        log_success "პორტი $port ხელმისაწვდომია"
    else
        log_warning "პორტი $port არ მუშაობს"
        PORTS_OK=false
    fi
done

# ფაილების სისტემის შემოწმება
echo "📁 მნიშვნელოვანი ფაილების შემოწმება..."
CRITICAL_FILES=(
    "Program.cs"
    "appsettings.json"
    "HospitalAssetTracker.csproj"
    "Data/ApplicationDbContext.cs"
)

FILES_OK=true
for file in "${CRITICAL_FILES[@]}"; do
    if [ -f "$PROJECT_DIR/$file" ]; then
        log_success "ფაილი არსებობს: $file"
    else
        log_error "ფაილი არ არსებობს: $file"
        FILES_OK=false
    fi
done

# Views-ების შემოწმება
echo "👁️  View ფაილების შემოწმება..."
VIEWS_DIR="$PROJECT_DIR/Views"
if [ -d "$VIEWS_DIR" ]; then
    VIEW_COUNT=$(find "$VIEWS_DIR" -name "*.cshtml" | wc -l)
    log_success "$VIEW_COUNT View ფაილი ნაპოვნია"
else
    log_error "Views დირექტორია არ არსებობს"
    FILES_OK=false
fi

# სტატიკური ფაილების შემოწმება
echo "📄 სტატიკური ფაილების შემოწმება..."
WWWROOT_DIR="$PROJECT_DIR/wwwroot"
if [ -d "$WWWROOT_DIR" ]; then
    log_success "wwwroot დირექტორია არსებობს"
    if [ -d "$WWWROOT_DIR/css" ] && [ -d "$WWWROOT_DIR/js" ]; then
        log_success "CSS და JS დირექტორიები არსებობს"
    else
        log_warning "CSS ან JS დირექტორია არ არსებობს"
    fi
else
    log_warning "wwwroot დირექტორია არ არსებობს"
fi

# ტესტის შედეგების ჯამი
echo ""
echo "📊 ტესტის შედეგები:"
echo "=================================================="

TOTAL_SCORE=0
MAX_SCORE=6

if [ "$BUILD_SUCCESS" = true ]; then
    echo "✅ პროექტის ბილდი: წარმატებული"
    ((TOTAL_SCORE++))
else
    echo "❌ პროექტის ბილდი: წარუმატებელი"
fi

if [ "$DB_CONNECTION" = true ]; then
    echo "✅ მონაცემთა ბაზა: კავშირი OK"
    ((TOTAL_SCORE++))
else
    echo "⚠️  მონაცემთა ბაზა: კავშირის პრობლემა"
fi

if [ "$SERVICE_RUNNING" = true ]; then
    echo "✅ ASP.NET სერვისი: გაშვებულია"
    ((TOTAL_SCORE++))
else
    echo "⚠️  ASP.NET სერვისი: არ მუშაობს"
fi

if [ "$PORTS_OK" = true ]; then
    echo "✅ პორტები: ყველა მუშაობს"
    ((TOTAL_SCORE++))
else
    echo "⚠️  პორტები: ზოგიერთი არ მუშაობს"
fi

if [ "$FILES_OK" = true ]; then
    echo "✅ ფაილები: ყველა არსებობს"
    ((TOTAL_SCORE++))
else
    echo "❌ ფაილები: ზოგიერთი გამოვიანება"
fi

# Health Score გამოთვლა
HEALTH_PERCENTAGE=$(( (TOTAL_SCORE * 100) / MAX_SCORE ))
echo ""
echo "🏥 სისტემის ჯანმრთელობა: $HEALTH_PERCENTAGE% ($TOTAL_SCORE/$MAX_SCORE)"

if [ $HEALTH_PERCENTAGE -ge 80 ]; then
    echo "🟢 სისტემის მუშაობა: შესანიშნავი"
    HEALTH_STATUS="EXCELLENT"
elif [ $HEALTH_PERCENTAGE -ge 60 ]; then
    echo "🟡 სისტემის მუშაობა: კარგი"
    HEALTH_STATUS="GOOD"
else
    echo "🔴 სისტემის მუშაობა: მოითხოვს ყურადღებას"
    HEALTH_STATUS="NEEDS_ATTENTION"
fi

# ავტომატური ბექაპი თუ ყველაფერი კარგია
if [ $HEALTH_PERCENTAGE -ge 60 ]; then
    echo ""
    echo "💾 ავტომატური ბექაპის შექმნა..."
    BACKUP_FILE="$BACKUP_DIR/auto_backup_$TIMESTAMP.tar.gz"
    if tar -czf "$BACKUP_FILE" "$PROJECT_DIR" 2>/dev/null; then
        log_success "ბექაპი შეიქმნა: $BACKUP_FILE"
    else
        log_error "ბექაპის შექმნის შეცდომა"
    fi
fi

# შედეგების შენახვა JSON ფორმატში
cat > "$LOG_DIR/test_results_$TIMESTAMP.json" << EOF
{
    "timestamp": "$(date -Iseconds)",
    "health_percentage": $HEALTH_PERCENTAGE,
    "health_status": "$HEALTH_STATUS",
    "build_success": $BUILD_SUCCESS,
    "db_connection": $DB_CONNECTION,
    "service_running": $SERVICE_RUNNING,
    "ports_ok": $PORTS_OK,
    "files_ok": $FILES_OK,
    "total_score": $TOTAL_SCORE,
    "max_score": $MAX_SCORE,
    "log_file": "$LOG_FILE"
}
EOF

echo ""
echo "📋 დეტალური ლოგი შენახულია: $LOG_FILE"
echo "📊 JSON შედეგები: $LOG_DIR/test_results_$TIMESTAMP.json"
echo ""
echo "🏁 ტესტი დასრულდა: $(date '+%Y-%m-%d %H:%M:%S')"
echo "=================================================="

# გამოსვლის კოდი
if [ $HEALTH_PERCENTAGE -ge 60 ]; then
    exit 0
else
    exit 1
fi
