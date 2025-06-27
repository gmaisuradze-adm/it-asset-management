// Unified Business Logic Dashboard JavaScript
// Georgian Hospital IT Asset Management System

class UnifiedDashboard {
    constructor() {
        this.refreshInterval = 5 * 60 * 1000; // 5 minutes
        this.charts = {};
        this.init();
    }

    init() {
        this.bindEvents();
        this.startAutoRefresh();
        this.loadRealtimeData();
    }

    bindEvents() {
        // Refresh button
        $('#refreshDashboard').on('click', () => this.refreshAll());
        
        // Card click handlers
        $('.metric-card').on('click', (e) => this.handleMetricCardClick(e));
        
        // Action buttons
        $('#managerActionsBtn').on('click', () => this.showManagerActions());
        $('#itSupportActionsBtn').on('click', () => this.showITSupportActions());
        $('#automationSuggestionsBtn').on('click', () => this.showAutomationSuggestions());
    }

    async refreshAll() {
        this.showLoadingIndicator();
        
        try {
            const response = await fetch('/UnifiedBusinessLogic/GetDashboardData', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.updateDashboardData(data);
                this.showSuccessMessage('Dashboard updated successfully');
            } else {
                throw new Error('Failed to fetch dashboard data');
            }
        } catch (error) {
            console.error('Dashboard refresh error:', error);
            this.showErrorMessage('Failed to refresh dashboard data');
        } finally {
            this.hideLoadingIndicator();
        }
    }

    updateDashboardData(data) {
        // Update metric cards
        this.updateMetricCard('totalRequests', data.totalRequests);
        this.updateMetricCard('pendingApprovals', data.pendingApprovals);
        this.updateMetricCard('completedToday', data.completedToday);
        this.updateMetricCard('automationEfficiency', data.automationEfficiency, 'percentage');

        // Update role-based action counts
        $('#managerActionsCount').text(data.managerActions);
        $('#itSupportActionsCount').text(data.itSupportActions);
        $('#pendingDecisionsCount').text(data.pendingDecisions);
        $('#autoFulfilledTodayCount').text(data.autoFulfilledToday);

        // Update workflow performance
        this.updateWorkflowChart(data.successfulWorkflows, data.failedWorkflows);
        $('#averageProcessingTime').text(this.formatDuration(data.averageProcessingTime));

        // Update asset management overview
        $('#assetsNeedingAttention').text(data.assetsNeedingAttention);
        $('#lowStockItems').text(data.lowStockItems);

        // Update system alerts
        this.updateSystemAlerts(data.systemAlerts);

        // Refresh recommendations
        this.refreshRecommendations();
    }

    updateMetricCard(elementId, value, format = 'number') {
        const element = $(`#${elementId}`);
        if (element.length) {
            let displayValue = value;
            
            switch (format) {
                case 'percentage':
                    displayValue = (value * 100).toFixed(1) + '%';
                    break;
                case 'currency':
                    displayValue = new Intl.NumberFormat('ka-GE', {
                        style: 'currency',
                        currency: 'GEL'
                    }).format(value);
                    break;
                default:
                    displayValue = value.toLocaleString();
            }
            
            element.text(displayValue);
            this.animateValueChange(element);
        }
    }

    animateValueChange(element) {
        element.addClass('value-updated');
        setTimeout(() => element.removeClass('value-updated'), 1000);
    }

    updateWorkflowChart(successful, failed) {
        if (this.charts.workflow) {
            this.charts.workflow.data.datasets[0].data = [successful, failed];
            this.charts.workflow.update();
        }
    }

    async refreshRecommendations() {
        const container = $('#recommendationsContainer');
        
        try {
            container.html('<div class="text-center"><i class="fas fa-spinner fa-spin"></i> Loading recommendations...</div>');
            
            const response = await fetch('/UnifiedBusinessLogic/GetRecentRecommendations');
            if (response.ok) {
                const html = await response.text();
                container.html(html);
                this.bindRecommendationEvents();
            } else {
                throw new Error('Failed to load recommendations');
            }
        } catch (error) {
            console.error('Recommendations refresh error:', error);
            container.html('<div class="text-danger text-center">Failed to load recommendations</div>');
        }
    }

    bindRecommendationEvents() {
        $('.recommendation-action-btn').on('click', (e) => {
            e.preventDefault();
            const assetId = $(e.target).data('asset-id');
            const action = $(e.target).data('action');
            this.executeRecommendationAction(assetId, action);
        });

        $('.view-asset-btn').on('click', (e) => {
            e.preventDefault();
            const assetId = $(e.target).data('asset-id');
            this.viewAssetDetails(assetId);
        });
    }

    async executeRecommendationAction(assetId, action) {
        const confirmed = await this.showConfirmationDialog(
            `Execute Action`,
            `Are you sure you want to ${action.toLowerCase()} Asset #${assetId}?`
        );

        if (!confirmed) return;

        try {
            const response = await fetch('/UnifiedBusinessLogic/ExecuteAssetAction', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    assetId: assetId,
                    action: action
                })
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    this.showSuccessMessage(`Action ${action} executed successfully for Asset #${assetId}`);
                    this.refreshRecommendations();
                } else {
                    this.showErrorMessage(result.message || 'Action execution failed');
                }
            } else {
                throw new Error('Action execution failed');
            }
        } catch (error) {
            console.error('Action execution error:', error);
            this.showErrorMessage('Failed to execute action');
        }
    }

    viewAssetDetails(assetId) {
        window.open(`/Assets/Details/${assetId}`, '_blank');
    }

    showManagerActions() {
        this.openModal('/UnifiedBusinessLogic/ManagerActions', 'Manager Actions', 'lg');
    }

    showITSupportActions() {
        this.openModal('/UnifiedBusinessLogic/ITSupportActions', 'IT Support Actions', 'lg');
    }

    showAutomationSuggestions() {
        this.openModal('/UnifiedBusinessLogic/AutomationSuggestions', 'Automation Suggestions', 'xl');
    }

    updateSystemAlerts(alertCount) {
        const alertsContainer = $('#systemAlertsContainer');
        
        if (alertCount > 0) {
            alertsContainer.show();
            $('#systemAlertsCount').text(alertCount);
        } else {
            alertsContainer.hide();
        }
    }

    async openModal(url, title, size = 'lg') {
        try {
            const response = await fetch(url);
            if (response.ok) {
                const html = await response.text();
                
                const modalId = 'dynamicModal';
                const modalHtml = `
                    <div class="modal fade" id="${modalId}" tabindex="-1" role="dialog">
                        <div class="modal-dialog modal-${size}" role="document">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <h5 class="modal-title">${title}</h5>
                                    <button type="button" class="close" data-dismiss="modal">
                                        <span>&times;</span>
                                    </button>
                                </div>
                                <div class="modal-body">
                                    ${html}
                                </div>
                            </div>
                        </div>
                    </div>
                `;
                
                // Remove existing modal
                $(`#${modalId}`).remove();
                
                // Add new modal
                $('body').append(modalHtml);
                $(`#${modalId}`).modal('show');
                
                // Clean up when modal is hidden
                $(`#${modalId}`).on('hidden.bs.modal', function() {
                    $(this).remove();
                });
                
            } else {
                throw new Error('Failed to load modal content');
            }
        } catch (error) {
            console.error('Modal loading error:', error);
            this.showErrorMessage('Failed to load content');
        }
    }

    loadRealtimeData() {
        // Load real-time metrics via SignalR or polling
        this.startRealtimeUpdates();
    }

    startRealtimeUpdates() {
        // Implement SignalR connection for real-time updates
        // For now, use polling as fallback
        setInterval(() => {
            this.updateRealtimeMetrics();
        }, 30000); // Update every 30 seconds
    }

    async updateRealtimeMetrics() {
        try {
            const response = await fetch('/UnifiedBusinessLogic/GetRealtimeMetrics');
            if (response.ok) {
                const metrics = await response.json();
                this.updateRealtimeIndicators(metrics);
            }
        } catch (error) {
            console.warn('Realtime metrics update failed:', error);
        }
    }

    updateRealtimeIndicators(metrics) {
        // Update real-time indicators
        if (metrics.activeUsers !== undefined) {
            $('#activeUsersCount').text(metrics.activeUsers);
        }
        
        if (metrics.systemLoad !== undefined) {
            const loadPercentage = (metrics.systemLoad * 100).toFixed(1);
            $('#systemLoadPercentage').text(`${loadPercentage}%`);
            
            // Update load indicator color
            const loadIndicator = $('#systemLoadIndicator');
            loadIndicator.removeClass('bg-success bg-warning bg-danger');
            
            if (metrics.systemLoad < 0.6) {
                loadIndicator.addClass('bg-success');
            } else if (metrics.systemLoad < 0.8) {
                loadIndicator.addClass('bg-warning');
            } else {
                loadIndicator.addClass('bg-danger');
            }
        }
    }

    startAutoRefresh() {
        setInterval(() => {
            this.refreshAll();
        }, this.refreshInterval);
    }

    formatDuration(seconds) {
        const hours = Math.floor(seconds / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
    }

    showLoadingIndicator() {
        $('#dashboardLoadingIndicator').show();
    }

    hideLoadingIndicator() {
        $('#dashboardLoadingIndicator').hide();
    }

    showSuccessMessage(message) {
        this.showToast(message, 'success');
    }

    showErrorMessage(message) {
        this.showToast(message, 'error');
    }

    showToast(message, type = 'info') {
        const toastHtml = `
            <div class="toast toast-${type}" role="alert" style="position: fixed; top: 20px; right: 20px; z-index: 9999;">
                <div class="toast-body">
                    <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'} mr-2"></i>
                    ${message}
                </div>
            </div>
        `;
        
        const $toast = $(toastHtml);
        $('body').append($toast);
        
        $toast.toast({ delay: 3000 });
        $toast.toast('show');
        
        $toast.on('hidden.bs.toast', function() {
            $(this).remove();
        });
    }

    async showConfirmationDialog(title, message) {
        return new Promise((resolve) => {
            const modalHtml = `
                <div class="modal fade" id="confirmationModal" tabindex="-1" role="dialog">
                    <div class="modal-dialog" role="document">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">${title}</h5>
                                <button type="button" class="close" data-dismiss="modal">
                                    <span>&times;</span>
                                </button>
                            </div>
                            <div class="modal-body">
                                <p>${message}</p>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                                <button type="button" class="btn btn-primary" id="confirmAction">Confirm</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            
            $('#confirmationModal').remove();
            $('body').append(modalHtml);
            
            $('#confirmationModal').modal('show');
            
            $('#confirmAction').on('click', () => {
                $('#confirmationModal').modal('hide');
                resolve(true);
            });
            
            $('#confirmationModal').on('hidden.bs.modal', function() {
                $(this).remove();
                resolve(false);
            });
        });
    }
}

// Initialize dashboard when document is ready
$(document).ready(function() {
    window.unifiedDashboard = new UnifiedDashboard();
});

// Export for global access
window.UnifiedDashboard = UnifiedDashboard;
