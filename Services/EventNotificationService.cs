using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using System.Text.Json;

namespace HospitalAssetTracker.Services
{
    /// <summary>
    /// Event-Driven Notification Service for Workflow Orchestration
    /// Manages event publishing, subscription, processing, and notification delivery
    /// </summary>
    public interface IEventNotificationService
    {
        // === EVENT PUBLISHING ===
        Task<bool> PublishEventAsync(WorkflowEvent workflowEvent);
        Task<bool> PublishEventBatchAsync(List<WorkflowEvent> events);
        Task<bool> PublishEventAsync(string eventType, object eventData, string userId, Guid? workflowId = null);

        // === EVENT PROCESSING ===
        Task<List<WorkflowEvent>> GetPendingEventsAsync(int maxEvents = 100);
        Task<EventProcessingResult> ProcessEventAsync(WorkflowEvent workflowEvent);
        Task<EventProcessingResult> ProcessEventBatchAsync(List<WorkflowEvent> events);
        Task<bool> MarkEventAsProcessedAsync(int eventId, string processingResult);

        // === EVENT SUBSCRIPTIONS ===
        Task<EventSubscription> CreateSubscriptionAsync(CreateSubscriptionRequest request, string userId);
        Task<bool> UpdateSubscriptionAsync(int subscriptionId, UpdateSubscriptionRequest request, string userId);
        Task<bool> DeleteSubscriptionAsync(int subscriptionId, string userId);
        Task<List<EventSubscription>> GetActiveSubscriptionsAsync(string? eventType = null);

        // === NOTIFICATION DELIVERY ===
        Task<Models.NotificationResult> SendNotificationAsync(Models.NotificationRequest request);
        Task<List<Models.NotificationResult>> SendNotificationBatchAsync(List<Models.NotificationRequest> requests);
        Task<NotificationDeliveryStatusModel> GetNotificationStatusAsync(int notificationId);

        // === EVENT ANALYTICS ===
        Task<EventAnalyticsModel> GetEventAnalyticsAsync(DateTime fromDate, DateTime toDate);
        Task<List<EventTypeMetrics>> GetEventTypeMetricsAsync(DateTime fromDate, DateTime toDate);
        Task<List<SubscriptionMetrics>> GetSubscriptionMetricsAsync();
    }

    public class EventNotificationService : IEventNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<EventNotificationService> _logger;

        public EventNotificationService(
            ApplicationDbContext context,
            IAuditService auditService,
            ILogger<EventNotificationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Event Publishing

        /// <summary>
        /// Publish a workflow event for processing
        /// </summary>
        public async Task<bool> PublishEventAsync(WorkflowEvent workflowEvent)
        {
            try
            {
                _logger.LogInformation("Publishing event {EventType} for workflow {WorkflowId}", 
                    workflowEvent.EventType, workflowEvent.WorkflowId);

                workflowEvent.Timestamp = DateTime.UtcNow;
                workflowEvent.IsProcessed = false;

                _context.WorkflowEvents.Add(workflowEvent);
                await _context.SaveChangesAsync();

                // Process immediately if it's a high-priority event
                if (IsHighPriorityEvent(workflowEvent))
                {
                    _ = Task.Run(async () => await ProcessEventAsync(workflowEvent));
                }

                await _auditService.LogAsync(
                    AuditAction.Create,
                    "WorkflowEvent",
                    workflowEvent.Id,
                    workflowEvent.UserId,
                    $"Published event: {workflowEvent.EventType}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event {EventType}", workflowEvent.EventType);
                return false;
            }
        }

        /// <summary>
        /// Publish multiple events as a batch
        /// </summary>
        public async Task<bool> PublishEventBatchAsync(List<WorkflowEvent> events)
        {
            try
            {
                _logger.LogInformation("Publishing batch of {EventCount} events", events.Count);

                var timestamp = DateTime.UtcNow;
                foreach (var evt in events)
                {
                    evt.Timestamp = timestamp;
                    evt.IsProcessed = false;
                }

                _context.WorkflowEvents.AddRange(events);
                await _context.SaveChangesAsync();

                // Process high-priority events immediately
                var highPriorityEvents = events.Where(IsHighPriorityEvent).ToList();
                if (highPriorityEvents.Any())
                {
                    _ = Task.Run(async () => await ProcessEventBatchAsync(highPriorityEvents));
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event batch of {EventCount} events", events.Count);
                return false;
            }
        }

        /// <summary>
        /// Publish event with simplified parameters
        /// </summary>
        public async Task<bool> PublishEventAsync(string eventType, object eventData, string userId, Guid? workflowId = null)
        {
            var workflowEvent = new WorkflowEvent
            {
                WorkflowId = workflowId ?? Guid.Empty,
                EventType = Enum.Parse<WorkflowEventType>(eventType),
                EventData = JsonSerializer.Serialize(eventData),
                UserId = userId
            };

            return await PublishEventAsync(workflowEvent);
        }

        #endregion

        #region Event Processing

        /// <summary>
        /// Get pending events for processing
        /// </summary>
        public async Task<List<WorkflowEvent>> GetPendingEventsAsync(int maxEvents = 100)
        {
            return await _context.WorkflowEvents
                .Where(e => !e.IsProcessed)
                .OrderBy(e => e.Timestamp)
                .Take(maxEvents)
                .Include(e => e.User)
                .Include(e => e.Workflow)
                .ToListAsync();
        }

        /// <summary>
        /// Process a single workflow event
        /// </summary>
        public async Task<EventProcessingResult> ProcessEventAsync(WorkflowEvent workflowEvent)
        {
            var result = new EventProcessingResult
            {
                TotalEvents = 1,
                ProcessingDuration = TimeSpan.Zero
            };

            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("Processing event {EventId} of type {EventType}", 
                    workflowEvent.Id, workflowEvent.EventType);

                // Get active subscriptions for this event type
                var subscriptions = await GetActiveSubscriptionsForEventAsync(workflowEvent);

                var notifications = new List<Models.NotificationRequest>();

                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        // Evaluate subscription filters
                        if (await EvaluateSubscriptionFiltersAsync(subscription, workflowEvent))
                        {
                            // Create notification based on subscription configuration
                            var notification = await CreateNotificationFromSubscriptionAsync(subscription, workflowEvent);
                            notifications.Add(notification);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing subscription {SubscriptionId} for event {EventId}", 
                            subscription.Id, workflowEvent.Id);
                        result.Errors.Add(new EventProcessingError
                        {
                            EventId = workflowEvent.Id,
                            ErrorMessage = $"Subscription {subscription.Id}: {ex.Message}",
                            ErrorTime = DateTime.UtcNow
                        });
                    }
                }

                // Send notifications
                if (notifications.Any())
                {
                    var notificationResults = await SendNotificationBatchAsync(notifications);
                    var successfulNotifications = notificationResults.Count(n => n.Success);
                    
                    _logger.LogInformation("Sent {SuccessCount}/{TotalCount} notifications for event {EventId}", 
                        successfulNotifications, notifications.Count, workflowEvent.Id);
                }

                // Execute event-triggered actions
                await ExecuteEventTriggeredActionsAsync(workflowEvent);

                // Mark event as processed
                await MarkEventAsProcessedAsync(workflowEvent.Id, "Processed successfully");

                result.ProcessedEvents = 1;
                result.ProcessingDuration = DateTime.UtcNow - startTime;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventId}", workflowEvent.Id);
                
                result.FailedEvents = 1;
                result.Errors.Add(new EventProcessingError
                {
                    EventId = workflowEvent.Id,
                    ErrorMessage = ex.Message,
                    ErrorTime = DateTime.UtcNow,
                    StackTrace = ex.StackTrace
                });
                result.ProcessingDuration = DateTime.UtcNow - startTime;

                return result;
            }
        }

        /// <summary>
        /// Process multiple events as a batch
        /// </summary>
        public async Task<EventProcessingResult> ProcessEventBatchAsync(List<WorkflowEvent> events)
        {
            var result = new EventProcessingResult
            {
                TotalEvents = events.Count
            };

            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("Processing batch of {EventCount} events", events.Count);

                var processingTasks = events.Select(async evt =>
                {
                    try
                    {
                        await ProcessEventAsync(evt);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing event {EventId} in batch", evt.Id);
                        result.Errors.Add(new EventProcessingError
                        {
                            EventId = evt.Id,
                            ErrorMessage = ex.Message,
                            ErrorTime = DateTime.UtcNow
                        });
                        return false;
                    }
                });

                var results = await Task.WhenAll(processingTasks);
                
                result.ProcessedEvents = results.Count(r => r);
                result.FailedEvents = results.Count(r => !r);
                result.ProcessingDuration = DateTime.UtcNow - startTime;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event batch");
                result.FailedEvents = events.Count;
                result.ProcessingDuration = DateTime.UtcNow - startTime;
                return result;
            }
        }

        /// <summary>
        /// Mark event as processed
        /// </summary>
        public async Task<bool> MarkEventAsProcessedAsync(int eventId, string processingResult)
        {
            try
            {
                var workflowEvent = await _context.WorkflowEvents.FindAsync(eventId);
                if (workflowEvent != null)
                {
                    workflowEvent.IsProcessed = true;
                    workflowEvent.ProcessedAt = DateTime.UtcNow;
                    workflowEvent.ProcessingResult = processingResult;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking event {EventId} as processed", eventId);
                return false;
            }
        }

        #endregion

        #region Event Subscriptions

        /// <summary>
        /// Create new event subscription
        /// </summary>
        public async Task<EventSubscription> CreateSubscriptionAsync(CreateSubscriptionRequest request, string userId)
        {
            var subscription = new EventSubscription
            {
                Name = request.Name,
                Description = request.Description,
                EventType = request.EventType,
                Filters = JsonSerializer.Serialize(request.Filters),
                NotificationConfig = JsonSerializer.Serialize(request.NotificationConfig),
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            _context.EventSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditAction.Create,
                "EventSubscription",
                subscription.Id,
                userId,
                $"Created event subscription: {subscription.Name}");

            return subscription;
        }

        /// <summary>
        /// Update existing event subscription
        /// </summary>
        public async Task<bool> UpdateSubscriptionAsync(int subscriptionId, UpdateSubscriptionRequest request, string userId)
        {
            try
            {
                var subscription = await _context.EventSubscriptions.FindAsync(subscriptionId);
                if (subscription == null)
                    return false;

                subscription.Name = request.Name ?? subscription.Name;
                subscription.Description = request.Description ?? subscription.Description;
                subscription.EventType = request.EventType ?? subscription.EventType;
                
                if (request.Filters != null)
                    subscription.Filters = JsonSerializer.Serialize(request.Filters);
                if (request.NotificationConfig != null)
                    subscription.NotificationConfig = JsonSerializer.Serialize(request.NotificationConfig);
                if (request.IsActive.HasValue)
                    subscription.IsActive = request.IsActive.Value;

                subscription.LastModified = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    AuditAction.Update,
                    "EventSubscription",
                    subscription.Id,
                    userId,
                    $"Updated event subscription: {subscription.Name}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription {SubscriptionId}", subscriptionId);
                return false;
            }
        }

        /// <summary>
        /// Delete event subscription
        /// </summary>
        public async Task<bool> DeleteSubscriptionAsync(int subscriptionId, string userId)
        {
            try
            {
                var subscription = await _context.EventSubscriptions.FindAsync(subscriptionId);
                if (subscription == null)
                    return false;

                _context.EventSubscriptions.Remove(subscription);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    AuditAction.Delete,
                    "EventSubscription",
                    subscription.Id,
                    userId,
                    $"Deleted event subscription: {subscription.Name}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subscription {SubscriptionId}", subscriptionId);
                return false;
            }
        }

        /// <summary>
        /// Get active event subscriptions
        /// </summary>
        public async Task<List<EventSubscription>> GetActiveSubscriptionsAsync(string? eventType = null)
        {
            var query = _context.EventSubscriptions.Where(s => s.IsActive);
            
            if (!string.IsNullOrEmpty(eventType))
            {
                query = query.Where(s => s.EventType == eventType);
            }

            return await query.ToListAsync();
        }

        #endregion

        #region Notification Delivery

        /// <summary>
        /// Send a notification
        /// </summary>
        public async Task<Models.NotificationResult> SendNotificationAsync(Models.NotificationRequest request)
        {
            var result = new Models.NotificationResult
            {
                NotificationId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Success = true
            };

            try
            {
                // Create notification record
                var notification = new Notification
                {
                    RecipientUserId = request.RecipientUserId,
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type,
                    Priority = request.Priority,
                    Status = Models.NotificationStatus.Created,
                    CreatedAt = DateTime.UtcNow,
                    RelatedEntityType = request.RelatedEntityType,
                    RelatedEntityId = request.RelatedEntityId,
                    ActionUrl = request.ActionUrl,
                    Metadata = JsonSerializer.Serialize(request.Metadata)
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Send notification based on type
                var success = await DeliverNotificationAsync(notification);
                
                notification.Status = success ? Models.NotificationStatus.Delivered : Models.NotificationStatus.Failed;
                notification.DeliveredAt = success ? DateTime.UtcNow : null;
                await _context.SaveChangesAsync();

                result.NotificationId = notification.Id;
                result.Success = success;
                result.DeliveryStatus = success ? Models.NotificationDeliveryStatus.Delivered : Models.NotificationDeliveryStatus.Failed;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to {RecipientUserId}", request.RecipientUserId);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Send multiple notifications as a batch
        /// </summary>
        public async Task<List<Models.NotificationResult>> SendNotificationBatchAsync(List<Models.NotificationRequest> requests)
        {
            var results = new List<Models.NotificationResult>();

            try
            {
                var sendingTasks = requests.Select(async request =>
                {
                    try
                    {
                        return await SendNotificationAsync(request);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in batch notification for {RecipientUserId}", request.RecipientUserId);
                        return new Models.NotificationResult
                        {
                            Success = false,
                            ErrorMessage = ex.Message,
                            CreatedAt = DateTime.UtcNow
                        };
                    }
                });

                results = (await Task.WhenAll(sendingTasks)).ToList();
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification batch");
                return results;
            }
        }

        /// <summary>
        /// Get notification delivery status
        /// </summary>
        public async Task<NotificationDeliveryStatusModel> GetNotificationStatusAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
            {
                return new NotificationDeliveryStatusModel
                {
                    NotificationId = notificationId,
                    Status = NotificationStatus.NotFound
                };
            }

            return new NotificationDeliveryStatusModel
            {
                NotificationId = notificationId,
                Status = notification.Status,
                CreatedAt = notification.CreatedAt,
                SentAt = notification.SentAt,
                DeliveredAt = notification.DeliveredAt,
                ReadAt = notification.ReadAt
            };
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Get event analytics for date range
        /// </summary>
        public async Task<EventAnalyticsModel> GetEventAnalyticsAsync(DateTime fromDate, DateTime toDate)
        {
            var events = await _context.WorkflowEvents
                .Where(e => e.Timestamp >= fromDate && e.Timestamp <= toDate)
                .ToListAsync();

            var analytics = new EventAnalyticsModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalEvents = events.Count,
                ProcessedEvents = events.Count(e => e.IsProcessed),
                PendingEvents = events.Count(e => !e.IsProcessed),
                ProcessingSuccessRate = events.Count > 0 ? (double)events.Count(e => e.IsProcessed) / events.Count * 100 : 0
            };

            // Group by event type
            analytics.EventsByType = events
                .GroupBy(e => e.EventType.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Group by day
            analytics.EventsByDay = events
                .GroupBy(e => e.Timestamp.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count());

            return analytics;
        }

        /// <summary>
        /// Get event type metrics
        /// </summary>
        public async Task<List<EventTypeMetrics>> GetEventTypeMetricsAsync(DateTime fromDate, DateTime toDate)
        {
            var events = await _context.WorkflowEvents
                .Where(e => e.Timestamp >= fromDate && e.Timestamp <= toDate)
                .ToListAsync();

            return events
                .GroupBy(e => e.EventType.ToString())
                .Select(g => new EventTypeMetrics
                {
                    EventType = g.Key,
                    TotalEvents = g.Count(),
                    ProcessedEvents = g.Count(e => e.IsProcessed),
                    ProcessingSuccessRate = g.Any() ? (double)g.Count(e => e.IsProcessed) / g.Count() * 100 : 0,
                    AverageProcessingTime = TimeSpan.FromMinutes(5) // Placeholder
                })
                .OrderByDescending(m => m.TotalEvents)
                .ToList();
        }

        /// <summary>
        /// Get subscription metrics
        /// </summary>
        public async Task<List<SubscriptionMetrics>> GetSubscriptionMetricsAsync()
        {
            var subscriptions = await _context.EventSubscriptions
                .Include(s => s.CreatedByUser)
                .ToListAsync();

            return subscriptions.Select(s => new SubscriptionMetrics
            {
                SubscriptionId = s.Id,
                SubscriptionName = s.Name,
                EventType = s.EventType,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                TotalTriggered = 0, // Would need to track this
                LastTriggered = null // Would need to track this
            }).ToList();
        }

        #endregion

        #region Private Helper Methods

        private static bool IsHighPriorityEvent(WorkflowEvent workflowEvent)
        {
            return workflowEvent.EventType switch
            {
                WorkflowEventType.WorkflowFailed => true,
                WorkflowEventType.StepFailed => true,
                _ => false
            };
        }

        private async Task<List<EventSubscription>> GetActiveSubscriptionsForEventAsync(WorkflowEvent workflowEvent)
        {
            return await _context.EventSubscriptions
                .Where(s => s.IsActive && s.EventType == workflowEvent.EventType.ToString())
                .ToListAsync();
        }

        private static async Task<bool> EvaluateSubscriptionFiltersAsync(EventSubscription subscription, WorkflowEvent workflowEvent)
        {
            // Implementation for filter evaluation
            return await Task.FromResult(true); // Placeholder - would implement actual filter logic
        }

        private static async Task<Models.NotificationRequest> CreateNotificationFromSubscriptionAsync(EventSubscription subscription, WorkflowEvent workflowEvent)
        {
            // Implementation for creating notification from subscription
            return await Task.FromResult(new Models.NotificationRequest
            {
                RecipientUserId = subscription.CreatedByUserId,
                Type = NotificationType.InApp,
                Title = $"Workflow Event: {workflowEvent.EventType}",
                Message = $"Event triggered in workflow {workflowEvent.WorkflowId}",
                Priority = NotificationPriority.Medium,
                Metadata = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["WorkflowId"] = workflowEvent.WorkflowId,
                    ["EventType"] = workflowEvent.EventType.ToString(),
                    ["EventData"] = workflowEvent.EventData
                })
            });
        }

        private async Task ExecuteEventTriggeredActionsAsync(WorkflowEvent workflowEvent)
        {
            // Implementation for executing event-triggered actions
            await Task.CompletedTask; // Placeholder
        }

        private async Task<bool> DeliverNotificationAsync(Notification notification)
        {
            // Implementation for actual notification delivery
            return await Task.FromResult(true); // Placeholder
        }

        private static string GetDeliveryMethodForType(NotificationType notificationType)
        {
            return notificationType switch
            {
                NotificationType.Email => "Email",
                NotificationType.SMS => "SMS",
                NotificationType.InApp => "In-App",
                NotificationType.Push => "Push",
                _ => "Unknown"
            };
        }

        #endregion
    }

    #region Supporting Models

    public class CreateSubscriptionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public Dictionary<string, object> Filters { get; set; } = new();
        public Dictionary<string, object> NotificationConfig { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSubscriptionRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? EventType { get; set; }
        public Dictionary<string, object>? Filters { get; set; }
        public Dictionary<string, object>? NotificationConfig { get; set; }
        public bool? IsActive { get; set; }
    }

    public class NotificationDeliveryStatusModel
    {
        public int NotificationId { get; set; }
        public NotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class EventAnalyticsModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalEvents { get; set; }
        public int ProcessedEvents { get; set; }
        public int PendingEvents { get; set; }
        public double ProcessingSuccessRate { get; set; }
        public Dictionary<string, int> EventsByType { get; set; } = new();
        public Dictionary<DateTime, int> EventsByDay { get; set; } = new();
    }

    public class EventTypeMetrics
    {
        public string EventType { get; set; } = string.Empty;
        public int TotalEvents { get; set; }
        public int ProcessedEvents { get; set; }
        public double ProcessingSuccessRate { get; set; }
        public TimeSpan AverageProcessingTime { get; set; }
    }

    public class SubscriptionMetrics
    {
        public int SubscriptionId { get; set; }
        public string SubscriptionName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalTriggered { get; set; }
        public DateTime? LastTriggered { get; set; }
    }

    #endregion
}
