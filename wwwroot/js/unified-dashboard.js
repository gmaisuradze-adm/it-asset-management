/**
 * Unified Business Logic Dashboard JavaScript
 * Hospital IT Asset Management System
 */

class UnifiedDashboard {
    constructor() {
        this.refreshInterval = 5 * 60 * 1000; // 5 minutes
        this.charts = {};
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.initializeCharts();
        this.startAutoRefresh();
        this.loadNotifications();
    }

    setupEventListeners() {
        // Refresh button
        const refreshBtn = document.querySelector('[onclick="refreshDashboard()"]');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => this.refreshDashboard());
        }

        // Quick action buttons
        document.querySelectorAll('.quick-action-btn').forEach(btn => {
            btn.addEventListener('click', (e) => this.handleQuickAction(e));
        });

        // Alert dismissal
        document.querySelectorAll('.alert-dismiss').forEach(btn => {
            btn.addEventListener('click', (e) => this.dismissAlert(e));
        });

        // Tab switches
        document.querySelectorAll('[data-bs-toggle="tab"]').forEach(tab => {
            tab.addEventListener('shown.bs.tab', (e) => this.onTabShown(e));
        });
    }

    async refreshDashboard() {
        try {
            this.showLoadingState();
            
            // Refresh various components
            await Promise.all([
                this.refreshMetrics(),
                this.refreshActivities(),
                this.refreshAlerts(),
                this.refreshWorkflowSummary()
            ]);

            this.updateLastRefreshTime();
            this.hideLoadingState();
            
            this.showSuccessMessage('Dashboard refreshed successfully');
        } catch (error) {
            console.error('Error refreshing dashboard:', error);
            this.showErrorMessage('Failed to refresh dashboard');
            this.hideLoadingState();
        }
    }

    async refreshMetrics() {
        try {
            const response = await fetch('/UnifiedBusinessLogic/GetPerformanceMetrics');
            const result = await response.json();
            
            if (result.success) {
                this.updateMetricsDisplay(result.data);
            }
        } catch (error) {
            console.error('Error refreshing metrics:', error);
        }
    }

    async refreshActivities() {
        try {
            const response = await fetch('/UnifiedBusinessLogic/GetRecentActivities?count=10');
            const result = await response.json();
            
            if (result.success) {
                this.updateActivitiesDisplay(result.data);
            }
        } catch (error) {
            console.error('Error refreshing activities:', error);
        }
    }

    async refreshAlerts() {
        try {
            const response = await fetch('/UnifiedBusinessLogic/GetAlerts?unreadOnly=true');
            const result = await response.json();
            
            if (result.success) {
                this.updateAlertsDisplay(result.data);
            }
        } catch (error) {
            console.error('Error refreshing alerts:', error);
        }
    }

    async refreshWorkflowSummary() {
        try {
            const response = await fetch('/UnifiedBusinessLogic/GetWorkflowSummary');
            const result = await response.json();
            
            if (result.success) {
                this.updateWorkflowDisplay(result.data);
            }
        } catch (error) {
            console.error('Error refreshing workflow summary:', error);
        }
    }

    updateMetricsDisplay(metrics) {
        // Update performance metrics
        const utilizationCircle = document.querySelector('[data-percentage]');
        if (utilizationCircle && metrics.assetUtilizationRate !== undefined) {
            utilizationCircle.setAttribute('data-percentage', Math.round(metrics.assetUtilizationRate));
            utilizationCircle.style.setProperty('--percentage', Math.round(metrics.assetUtilizationRate));
            utilizationCircle.querySelector('.progress-text').textContent = `${Math.round(metrics.assetUtilizationRate)}%`;
        }

        // Update other metric displays
        this.updateMetricValue('avg-processing-time', `${Math.round(metrics.averageRequestProcessingTime * 10) / 10}h`);
        this.updateMetricValue('procurement-efficiency', `${Math.round(metrics.procurementEfficiency)}%`);
        this.updateMetricValue('user-satisfaction', `${metrics.userSatisfactionScore}/10`);
    }

    updateActivitiesDisplay(activities) {
        const container = document.querySelector('.activity-timeline');
        if (!container || !activities) return;

        const html = activities.map(activity => `
            <div class="activity-item d-flex align-items-start mb-3">
                <div class="activity-icon me-3">
                    <i class="${activity.icon} text-${activity.colorClass}"></i>
                </div>
                <div class="flex-grow-1">
                    <div class="activity-content">
                        <strong>${activity.action}</strong> by ${activity.userName}
                    </div>
                    <div class="activity-description text-muted small">
                        ${activity.description}
                    </div>
                    <div class="activity-time text-muted small">
                        ${this.formatDateTime(activity.timestamp)}
                    </div>
                </div>
            </div>
        `).join('');

        container.innerHTML = html;
    }

    updateAlertsDisplay(alerts) {
        const alertsContainer = document.querySelector('.alerts-container');
        if (!alertsContainer || !alerts || alerts.length === 0) return;

        const html = alerts.slice(0, 3).map(alert => `
            <div class="col-md-4">
                <div class="d-flex align-items-center">
                    <i class="${alert.icon} me-2 text-${alert.colorClass}"></i>
                    <div>
                        <strong>${alert.title}</strong><br>
                        <small>${alert.message}</small>
                    </div>
                </div>
            </div>
        `).join('');

        alertsContainer.innerHTML = html;
    }

    updateWorkflowDisplay(workflow) {
        this.updateMetricValue('pending-approvals', workflow.pendingApprovals);
        this.updateMetricValue('active-workflows', workflow.activeWorkflows);
        this.updateMetricValue('completed-today', workflow.completedToday);
        this.updateMetricValue('overdue-items', workflow.overdueItems);
    }

    updateMetricValue(selector, value) {
        const element = document.querySelector(`[data-metric="${selector}"]`) || 
                      document.querySelector(`.${selector}`) ||
                      document.getElementById(selector);
        if (element) {
            element.textContent = value;
        }
    }

    initializeCharts() {
        this.initializeAssetChart();
        this.initializeProgressCircles();
    }

    initializeAssetChart() {
        const ctx = document.getElementById('assetStatusChart');
        if (!ctx || typeof Chart === 'undefined') return;

        // This will be called from the view with actual data
    }

    initializeProgressCircles() {
        document.querySelectorAll('.progress-circle').forEach(circle => {
            const percentage = circle.getAttribute('data-percentage') || 0;
            circle.style.setProperty('--percentage', percentage);
        });
    }

    async handleQuickAction(event) {
        event.preventDefault();
        const button = event.currentTarget;
        const actionId = button.getAttribute('data-action-id');
        
        if (!actionId) return;

        try {
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Processing...';

            const response = await fetch('/UnifiedBusinessLogic/ExecuteQuickAction', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({ actionId: actionId })
            });

            const result = await response.json();
            
            if (result.success) {
                this.showSuccessMessage('Action executed successfully');
                // Optionally redirect or refresh specific components
            } else {
                this.showErrorMessage(result.message || 'Failed to execute action');
            }
        } catch (error) {
            console.error('Error executing quick action:', error);
            this.showErrorMessage('An error occurred while executing the action');
        } finally {
            button.disabled = false;
            button.innerHTML = button.getAttribute('data-original-text') || 'Execute';
        }
    }

    async dismissAlert(event) {
        event.preventDefault();
        const button = event.currentTarget;
        const alertId = button.getAttribute('data-alert-id');
        
        if (!alertId) return;

        try {
            const response = await fetch('/UnifiedBusinessLogic/DismissAlert', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({ alertId: parseInt(alertId) })
            });

            const result = await response.json();
            
            if (result.success) {
                // Remove the alert from display
                const alertElement = button.closest('.alert, .alert-item');
                if (alertElement) {
                    alertElement.remove();
                }
            }
        } catch (error) {
            console.error('Error dismissing alert:', error);
        }
    }

    onTabShown(event) {
        const targetTab = event.target.getAttribute('data-bs-target');
        
        // Refresh data for the newly shown tab if needed
        switch (targetTab) {
            case '#approvals':
                this.refreshApprovals();
                break;
            case '#tasks':
                this.refreshTasks();
                break;
            case '#overdue':
                this.refreshOverdueItems();
                break;
            case '#assignments':
                this.refreshAssignments();
                break;
        }
    }

    async loadNotifications() {
        try {
            // Load unread notifications for the notification bell
            const response = await fetch('/Api/Notifications/Unread');
            const result = await response.json();
            
            if (result.success) {
                this.updateNotificationBell(result.data);
            }
        } catch (error) {
            console.error('Error loading notifications:', error);
        }
    }

    updateNotificationBell(notifications) {
        const bell = document.querySelector('.notification-bell');
        const badge = document.querySelector('.notification-badge');
        
        if (bell && badge) {
            const count = notifications.length;
            badge.textContent = count;
            badge.style.display = count > 0 ? 'inline' : 'none';
        }
    }

    startAutoRefresh() {
        setInterval(() => {
            this.refreshDashboard();
        }, this.refreshInterval);
    }

    showLoadingState() {
        const refreshBtn = document.querySelector('[onclick="refreshDashboard()"]');
        if (refreshBtn) {
            refreshBtn.disabled = true;
            refreshBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Refreshing...';
        }
    }

    hideLoadingState() {
        const refreshBtn = document.querySelector('[onclick="refreshDashboard()"]');
        if (refreshBtn) {
            refreshBtn.disabled = false;
            refreshBtn.innerHTML = '<i class="fas fa-sync-alt"></i> Refresh';
        }
    }

    updateLastRefreshTime() {
        const timeElement = document.getElementById('lastUpdated');
        if (timeElement) {
            timeElement.textContent = new Date().toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
        }
    }

    showSuccessMessage(message) {
        this.showToast(message, 'success');
    }

    showErrorMessage(message) {
        this.showToast(message, 'error');
    }

    showToast(message, type = 'info') {
        // Create a simple toast notification
        const toast = document.createElement('div');
        toast.className = `alert alert-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} alert-dismissible fade show position-fixed`;
        toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        toast.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(toast);
        
        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 5000);
    }

    getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    formatDateTime(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
    }

    // Utility methods for tab-specific refreshes
    async refreshApprovals() {
        // Implementation for refreshing approvals tab
    }

    async refreshTasks() {
        // Implementation for refreshing tasks tab
    }

    async refreshOverdueItems() {
        // Implementation for refreshing overdue items tab
    }

    async refreshAssignments() {
        // Implementation for refreshing assignments tab
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Only initialize on pages that have the unified dashboard
    if (document.querySelector('.unified-dashboard') || 
        document.querySelector('#assetStatusChart') ||
        window.location.pathname.includes('UnifiedBusinessLogic')) {
        
        window.unifiedDashboard = new UnifiedDashboard();
    }
});

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = UnifiedDashboard;
}
