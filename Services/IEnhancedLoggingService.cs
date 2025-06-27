using Microsoft.Extensions.Logging;

namespace HospitalAssetTracker.Services
{
    /// <summary>
    /// Enhanced logging service interface for structured logging with context
    /// </summary>
    public interface IEnhancedLoggingService
    {
        // Performance logging
        Task LogPerformanceAsync(string operation, TimeSpan duration, string? userId = null, Dictionary<string, object>? additionalData = null);
        
        // Security logging
        Task LogSecurityEventAsync(string eventType, string description, string? userId = null, string? ipAddress = null, bool isSuccessful = true);
        
        // Business operation logging
        Task LogBusinessOperationAsync(string operation, string entityType, int? entityId, string? userId = null, Dictionary<string, object>? context = null);
        
        // Error logging with context
        Task LogErrorWithContextAsync(Exception exception, string operation, string? userId = null, Dictionary<string, object>? context = null);
        
        // User activity logging
        Task LogUserActivityAsync(string activity, string? userId = null, string? details = null, Dictionary<string, object>? metadata = null);
        
        // System health logging
        Task LogSystemHealthAsync(string component, string status, Dictionary<string, object>? metrics = null);
        
        // API request logging
        Task LogApiRequestAsync(string method, string path, int statusCode, TimeSpan duration, string? userId = null, long? responseSize = null);
        
        // Database operation logging
        Task LogDatabaseOperationAsync(string operation, string table, TimeSpan duration, int? recordsAffected = null, string? userId = null);
    }

    /// <summary>
    /// Enhanced logging service implementation with structured logging
    /// </summary>
    public class EnhancedLoggingService : IEnhancedLoggingService
    {
        private readonly ILogger<EnhancedLoggingService> _logger;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EnhancedLoggingService(
            ILogger<EnhancedLoggingService> logger,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task LogPerformanceAsync(string operation, TimeSpan duration, string? userId = null, Dictionary<string, object>? additionalData = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["DurationMs"] = duration.TotalMilliseconds,
                ["UserId"] = userId ?? GetCurrentUserId(),
                ["Timestamp"] = DateTime.UtcNow
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            _logger.LogInformation("Performance: {Operation} completed in {DurationMs}ms by user {UserId}",
                operation, duration.TotalMilliseconds, userId ?? GetCurrentUserId());

            // Log slow operations as warnings
            if (duration.TotalSeconds > 5)
            {
                _logger.LogWarning("Slow Operation: {Operation} took {DurationMs}ms - consider optimization",
                    operation, duration.TotalMilliseconds);
            }
        }

        public async Task LogSecurityEventAsync(string eventType, string description, string? userId = null, string? ipAddress = null, bool isSuccessful = true)
        {
            var currentUserId = userId ?? GetCurrentUserId();
            var currentIpAddress = ipAddress ?? GetCurrentIpAddress();

            var logLevel = isSuccessful ? LogLevel.Information : LogLevel.Warning;
            var status = isSuccessful ? "SUCCESS" : "FAILURE";

            _logger.Log(logLevel, "Security Event: {EventType} - {Status} | User: {UserId} | IP: {IpAddress} | Description: {Description}",
                eventType, status, currentUserId, currentIpAddress, description);

            // Audit log security events
            try
            {
                await _auditService.LogAsync(
                    Models.AuditAction.Login, // Use appropriate action based on event type
                    "Security",
                    null,
                    currentUserId,
                    $"{eventType}: {description} - {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to audit log security event: {EventType}", eventType);
            }
        }

        public async Task LogBusinessOperationAsync(string operation, string entityType, int? entityId, string? userId = null, Dictionary<string, object>? context = null)
        {
            var currentUserId = userId ?? GetCurrentUserId();

            var contextData = context ?? new Dictionary<string, object>();
            contextData["Operation"] = operation;
            contextData["EntityType"] = entityType;
            contextData["EntityId"] = entityId;
            contextData["UserId"] = currentUserId;
            contextData["Timestamp"] = DateTime.UtcNow;

            _logger.LogInformation("Business Operation: {Operation} on {EntityType} {EntityId} by user {UserId}",
                operation, entityType, entityId, currentUserId);
        }

        public async Task LogErrorWithContextAsync(Exception exception, string operation, string? userId = null, Dictionary<string, object>? context = null)
        {
            var currentUserId = userId ?? GetCurrentUserId();
            var errorId = Guid.NewGuid().ToString();

            var contextData = context ?? new Dictionary<string, object>();
            contextData["ErrorId"] = errorId;
            contextData["Operation"] = operation;
            contextData["UserId"] = currentUserId;
            contextData["ExceptionType"] = exception.GetType().Name;
            contextData["Timestamp"] = DateTime.UtcNow;

            _logger.LogError(exception, "Error in operation {Operation} - Error ID: {ErrorId} | User: {UserId} | Exception: {ExceptionType}",
                operation, errorId, currentUserId, exception.GetType().Name);

            // Audit log the error
            try
            {
                await _auditService.LogAsync(
                    Models.AuditAction.Error,
                    "System",
                    null,
                    currentUserId,
                    $"Error in {operation} - Error ID: {errorId} | Exception: {exception.GetType().Name}");
            }
            catch (Exception auditEx)
            {
                _logger.LogError(auditEx, "Failed to audit log error for operation: {Operation}", operation);
            }
        }

        public async Task LogUserActivityAsync(string activity, string? userId = null, string? details = null, Dictionary<string, object>? metadata = null)
        {
            var currentUserId = userId ?? GetCurrentUserId();

            var activityData = metadata ?? new Dictionary<string, object>();
            activityData["Activity"] = activity;
            activityData["UserId"] = currentUserId;
            activityData["Details"] = details;
            activityData["Timestamp"] = DateTime.UtcNow;
            activityData["IpAddress"] = GetCurrentIpAddress();
            activityData["UserAgent"] = GetCurrentUserAgent();

            _logger.LogInformation("User Activity: {Activity} by user {UserId} | Details: {Details}",
                activity, currentUserId, details);
        }

        public async Task LogSystemHealthAsync(string component, string status, Dictionary<string, object>? metrics = null)
        {
            var healthData = metrics ?? new Dictionary<string, object>();
            healthData["Component"] = component;
            healthData["Status"] = status;
            healthData["Timestamp"] = DateTime.UtcNow;

            var logLevel = status.ToUpper() switch
            {
                "HEALTHY" => LogLevel.Information,
                "DEGRADED" => LogLevel.Warning,
                "UNHEALTHY" => LogLevel.Error,
                _ => LogLevel.Information
            };

            _logger.Log(logLevel, "System Health: {Component} is {Status}", component, status);

            if (metrics != null)
            {
                foreach (var metric in metrics)
                {
                    _logger.LogDebug("Health Metric: {Component}.{MetricName} = {MetricValue}",
                        component, metric.Key, metric.Value);
                }
            }
        }

        public async Task LogApiRequestAsync(string method, string path, int statusCode, TimeSpan duration, string? userId = null, long? responseSize = null)
        {
            var currentUserId = userId ?? GetCurrentUserId();

            var requestData = new Dictionary<string, object>
            {
                ["Method"] = method,
                ["Path"] = path,
                ["StatusCode"] = statusCode,
                ["DurationMs"] = duration.TotalMilliseconds,
                ["UserId"] = currentUserId,
                ["ResponseSize"] = responseSize,
                ["Timestamp"] = DateTime.UtcNow,
                ["IpAddress"] = GetCurrentIpAddress()
            };

            var logLevel = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

            _logger.Log(logLevel, "API Request: {Method} {Path} returned {StatusCode} in {DurationMs}ms | User: {UserId} | Size: {ResponseSize}",
                method, path, statusCode, duration.TotalMilliseconds, currentUserId, responseSize);
        }

        public async Task LogDatabaseOperationAsync(string operation, string table, TimeSpan duration, int? recordsAffected = null, string? userId = null)
        {
            var currentUserId = userId ?? GetCurrentUserId();

            _logger.LogDebug("Database Operation: {Operation} on {Table} completed in {DurationMs}ms | Records: {RecordsAffected} | User: {UserId}",
                operation, table, duration.TotalMilliseconds, recordsAffected, currentUserId);

            // Log slow database operations as warnings
            if (duration.TotalSeconds > 2)
            {
                _logger.LogWarning("Slow Database Operation: {Operation} on {Table} took {DurationMs}ms - consider optimization",
                    operation, table, duration.TotalMilliseconds);
            }
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
        }

        private string GetCurrentIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "Unknown";

            // Check for forwarded IP first (for load balancers/proxies)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Check for real IP header
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fall back to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string GetCurrentUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
        }
    }
}