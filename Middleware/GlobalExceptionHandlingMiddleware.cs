using System.Net;
using System.Text.Json;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Middleware
{
    /// <summary>
    /// Global exception handling middleware for centralized error processing
    /// Provides consistent error responses and comprehensive logging
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IAuditService _auditService;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger,
            IWebHostEnvironment environment,
            IAuditService auditService)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var errorId = Guid.NewGuid().ToString();
            var requestPath = context.Request.Path.Value ?? "Unknown";
            var requestMethod = context.Request.Method;
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var ipAddress = GetClientIpAddress(context);
            var userId = GetCurrentUserId(context);

            // Log the exception with comprehensive context
            _logger.LogError(exception,
                "Global Exception Handler - Error ID: {ErrorId} | Path: {RequestPath} | Method: {RequestMethod} | User: {UserId} | IP: {IpAddress} | UserAgent: {UserAgent}",
                errorId, requestPath, requestMethod, userId, ipAddress, userAgent);

            // Audit log the error for security and compliance
            try
            {
                await _auditService.LogAsync(
                    Models.AuditAction.Error,
                    "System",
                    null,
                    userId,
                    $"Global Exception - Error ID: {errorId} | Path: {requestPath} | Exception: {exception.GetType().Name}");
            }
            catch (Exception auditEx)
            {
                _logger.LogError(auditEx, "Failed to log exception to audit service for Error ID: {ErrorId}", errorId);
            }

            // Determine response based on exception type
            var errorResponse = CreateErrorResponse(exception, errorId);

            // Set response properties
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = errorResponse.StatusCode;

            // Serialize and write response
            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private ErrorResponse CreateErrorResponse(Exception exception, string errorId)
        {
            return exception switch
            {
                ArgumentNullException argEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorId = errorId,
                    Message = "Invalid request: Required parameter is missing",
                    Details = _environment.IsDevelopment() ? argEx.Message : null,
                    Timestamp = DateTime.UtcNow
                },

                ArgumentException argEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorId = errorId,
                    Message = "Invalid request: Parameter validation failed",
                    Details = _environment.IsDevelopment() ? argEx.Message : null,
                    Timestamp = DateTime.UtcNow
                },

                UnauthorizedAccessException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    ErrorId = errorId,
                    Message = "Access denied: Authentication required",
                    Details = _environment.IsDevelopment() ? exception.Message : null,
                    Timestamp = DateTime.UtcNow
                },

                InvalidOperationException invOpEx => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    ErrorId = errorId,
                    Message = "Operation cannot be completed due to current state",
                    Details = _environment.IsDevelopment() ? invOpEx.Message : null,
                    Timestamp = DateTime.UtcNow
                },

                TimeoutException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.RequestTimeout,
                    ErrorId = errorId,
                    Message = "Request timeout: Operation took too long to complete",
                    Details = _environment.IsDevelopment() ? exception.Message : null,
                    Timestamp = DateTime.UtcNow
                },

                NotImplementedException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.NotImplemented,
                    ErrorId = errorId,
                    Message = "Feature not implemented",
                    Details = _environment.IsDevelopment() ? exception.Message : null,
                    Timestamp = DateTime.UtcNow
                },

                FileNotFoundException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    ErrorId = errorId,
                    Message = "Requested resource not found",
                    Details = _environment.IsDevelopment() ? exception.Message : null,
                    Timestamp = DateTime.UtcNow
                },

                DirectoryServiceException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.ServiceUnavailable,
                    ErrorId = errorId,
                    Message = "External service unavailable",
                    Details = _environment.IsDevelopment() ? exception.Message : null,
                    Timestamp = DateTime.UtcNow
                },

                _ => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    ErrorId = errorId,
                    Message = "An unexpected error occurred. Please try again later.",
                    Details = _environment.IsDevelopment() ? $"{exception.GetType().Name}: {exception.Message}" : null,
                    Timestamp = DateTime.UtcNow
                }
            };
        }

        private static string GetClientIpAddress(HttpContext context)
        {
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

        private static string GetCurrentUserId(HttpContext context)
        {
            return context.User?.Identity?.Name ?? "Anonymous";
        }
    }

    /// <summary>
    /// Standardized error response model
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string ErrorId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string? TraceId { get; set; }
    }

    /// <summary>
    /// Custom exception for directory service operations
    /// </summary>
    public class DirectoryServiceException : Exception
    {
        public DirectoryServiceException(string message) : base(message) { }
        public DirectoryServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}