using System.Net;
using System.Security;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Middleware
{
    public class BugTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BugTrackingMiddleware> _logger;

        public BugTrackingMiddleware(RequestDelegate next, ILogger<BugTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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
            try
            {
                // Get services from context
                var bugTrackingService = context.RequestServices.GetService<BugTrackingService>();
                
                if (bugTrackingService != null)
                {
                    // Determine module from request path
                    var moduleName = GetModuleFromPath(context.Request.Path);
                    
                    // Auto-register the bug
                    await bugTrackingService.RegisterBugAsync(
                        title: $"Unhandled Exception in {moduleName}",
                        description: $"Exception occurred while processing: {context.Request.Method} {context.Request.Path}",
                        moduleName: moduleName,
                        errorMessage: exception.Message,
                        stackTrace: exception.StackTrace ?? "",
                        severity: DetermineSeverity(exception)
                    );
                    
                    _logger.LogError(exception, "Auto-registered bug for unhandled exception in {ModuleName}", moduleName);
                }
            }
            catch (Exception bugEx)
            {
                // Don't let bug tracking prevent error handling
                _logger.LogError(bugEx, "Error in bug tracking middleware");
            }

            // Set response
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "An internal server error occurred.",
                message = "The error has been automatically logged for investigation."
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private string GetModuleFromPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "Unknown";
            
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0)
            {
                return segments[0] switch
                {
                    "Assets" or "AssetDashboard" => "Asset Management",
                    "Requests" or "RequestDashboard" => "Request Management",
                    "Procurement" or "ProcurementDashboard" => "Procurement Management",
                    "Warehouse" or "Inventory" => "Inventory Management",
                    "BugTracking" => "Bug Tracking",
                    "Home" => "Main Dashboard",
                    _ => segments[0]
                };
            }
            
            return "Unknown";
        }

        private string DetermineSeverity(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException or NullReferenceException => "High",
                UnauthorizedAccessException or SecurityException => "Critical",
                TimeoutException or InvalidOperationException => "Medium",
                _ => "Medium"
            };
        }
    }
}
