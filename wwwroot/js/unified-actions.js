// Unified Business Logic Actions JavaScript
// Georgian Hospital IT Asset Management System

class UnifiedActions {
    constructor() {
        this.baseUrl = '/UnifiedBusinessLogic';
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadInitialData();
        this.initializeDataTables();
    }

    bindEvents() {
        // Tab switching events
        $('button[data-bs-toggle="tab"]').on('shown.bs.tab', (e) => {
            this.handleTabSwitch(e.target.getAttribute('data-bs-target'));
        });

        // Auto-refresh every 2 minutes for actions
        setInterval(() => {
            this.refreshCurrentTab();
        }, 120000);
    }

    loadInitialData() {
        // Load data for the active tab
        const activeTab = $('.nav-link.active').attr('data-bs-target');
        this.handleTabSwitch(activeTab);
    }

    handleTabSwitch(targetTab) {
        switch (targetTab) {
            case '#manager-actions':
                this.loadManagerActions();
                break;
            case '#itsupport-actions':
                this.loadITSupportActions();
                break;
            case '#automation-actions':
                this.loadAutomationRules();
                break;
        }
    }

    async loadManagerActions() {
        await Promise.all([
            this.loadPendingApprovals(),
            this.loadStrategicDecisions()
        ]);
    }

    async loadITSupportActions() {
        await Promise.all([
            this.loadAssetAssignments(),
            this.loadMaintenanceTasks(),
            this.loadUrgentIssues()
        ]);
    }

    async loadPendingApprovals() {
        try {
            const response = await this.apiCall('GetManagerActions');
            const approvals = response.pendingApprovals || [];
            
            const html = approvals.map(approval => `
                <div class="list-group-item d-flex justify-content-between align-items-center">
                    <div>
                        <h6 class="mb-1">${approval.title}</h6>
                        <p class="mb-1 text-muted">${approval.description}</p>
                        <small class="text-muted">Priority: ${approval.priority}</small>
                    </div>
                    <div class="btn-group btn-group-sm">
                        <button class="btn btn-success" onclick="approveRequest(${approval.id})">
                            <i class="fas fa-check"></i>
                        </button>
                        <button class="btn btn-danger" onclick="rejectRequest(${approval.id})">
                            <i class="fas fa-times"></i>
                        </button>
                        <button class="btn btn-info" onclick="viewDetails(${approval.id})">
                            <i class="fas fa-eye"></i>
                        </button>
                    </div>
                </div>
            `).join('');

            $('#pendingApprovalsList').html(html || '<p class="text-muted">No pending approvals</p>');
        } catch (error) {
            console.error('Error loading pending approvals:', error);
            this.showError('Failed to load pending approvals');
        }
    }

    async loadStrategicDecisions() {
        try {
            const response = await this.apiCall('GetManagerActions');
            const decisions = response.strategicDecisions || [];
            
            const html = decisions.map(decision => `
                <div class="list-group-item">
                    <div class="d-flex w-100 justify-content-between">
                        <h6 class="mb-1">${decision.title}</h6>
                        <small class="text-muted">${decision.priority}</small>
                    </div>
                    <p class="mb-1">${decision.description}</p>
                    <div class="btn-group btn-group-sm mt-2">
                        <button class="btn btn-primary" onclick="viewRecommendation(${decision.id})">
                            View Details
                        </button>
                        <button class="btn btn-success" onclick="implementDecision(${decision.id})">
                            Implement
                        </button>
                    </div>
                </div>
            `).join('');

            $('#strategicDecisionsList').html(html || '<p class="text-muted">No strategic decisions pending</p>');
        } catch (error) {
            console.error('Error loading strategic decisions:', error);
            this.showError('Failed to load strategic decisions');
        }
    }

    async loadAssetAssignments() {
        try {
            const response = await this.apiCall('GetITSupportActions');
            const assignments = response.assetAssignments || [];
            
            const html = assignments.map(assignment => `
                <div class="list-group-item">
                    <div class="d-flex w-100 justify-content-between">
                        <h6 class="mb-1">Asset: ${assignment.assetTag}</h6>
                        <small class="text-muted">${assignment.requestDate}</small>
                    </div>
                    <p class="mb-1">User: ${assignment.userName}</p>
                    <p class="mb-1">Department: ${assignment.department}</p>
                    <div class="btn-group btn-group-sm">
                        <button class="btn btn-success" onclick="assignAsset(${assignment.id})">
                            Assign
                        </button>
                        <button class="btn btn-warning" onclick="postponeAssignment(${assignment.id})">
                            Postpone
                        </button>
                    </div>
                </div>
            `).join('');

            $('#assetAssignmentsList').html(html || '<p class="text-muted">No asset assignments pending</p>');
        } catch (error) {
            console.error('Error loading asset assignments:', error);
            this.showError('Failed to load asset assignments');
        }
    }

    async loadMaintenanceTasks() {
        try {
            const response = await this.apiCall('GetITSupportActions');
            const tasks = response.maintenanceTasks || [];
            
            const html = tasks.map(task => `
                <div class="list-group-item">
                    <div class="d-flex w-100 justify-content-between">
                        <h6 class="mb-1">${task.assetTag}</h6>
                        <span class="badge badge-${task.priorityClass}">${task.priority}</span>
                    </div>
                    <p class="mb-1">${task.taskType}: ${task.description}</p>
                    <small class="text-muted">Due: ${task.dueDate}</small>
                    <div class="btn-group btn-group-sm mt-2">
                        <button class="btn btn-primary" onclick="startMaintenance(${task.id})">
                            Start
                        </button>
                        <button class="btn btn-success" onclick="completeMaintenance(${task.id})">
                            Complete
                        </button>
                    </div>
                </div>
            `).join('');

            $('#maintenanceTasksList').html(html || '<p class="text-muted">No maintenance tasks</p>');
        } catch (error) {
            console.error('Error loading maintenance tasks:', error);
            this.showError('Failed to load maintenance tasks');
        }
    }

    async loadUrgentIssues() {
        try {
            const response = await this.apiCall('GetITSupportActions');
            const issues = response.urgentIssues || [];
            
            const html = issues.map(issue => `
                <div class="list-group-item">
                    <div class="d-flex w-100 justify-content-between">
                        <h6 class="mb-1 text-danger">${issue.title}</h6>
                        <small class="text-muted">${issue.reportedTime}</small>
                    </div>
                    <p class="mb-1">${issue.description}</p>
                    <p class="mb-1">Reporter: ${issue.reporterName}</p>
                    <div class="btn-group btn-group-sm">
                        <button class="btn btn-danger" onclick="escalateIssue(${issue.id})">
                            Escalate
                        </button>
                        <button class="btn btn-warning" onclick="assignIssue(${issue.id})">
                            Assign
                        </button>
                        <button class="btn btn-info" onclick="viewIssueDetails(${issue.id})">
                            Details
                        </button>
                    </div>
                </div>
            `).join('');

            $('#urgentIssuesList').html(html || '<p class="text-muted">No urgent issues</p>');
        } catch (error) {
            console.error('Error loading urgent issues:', error);
            this.showError('Failed to load urgent issues');
        }
    }

    initializeDataTables() {
        // Initialize automation rules DataTable
        if ($.fn.DataTable.isDataTable('#automationRulesTable')) {
            $('#automationRulesTable').DataTable().destroy();
        }

        $('#automationRulesTable').DataTable({
            ajax: {
                url: `${this.baseUrl}/GetAutomationRules`,
                type: 'GET',
                dataSrc: 'rules'
            },
            columns: [
                { data: 'name' },
                { data: 'trigger' },
                { data: 'action' },
                { 
                    data: 'isActive',
                    render: function(data) {
                        return data ? '<span class="badge badge-success">Active</span>' : '<span class="badge badge-secondary">Inactive</span>';
                    }
                },
                { 
                    data: 'successRate',
                    render: function(data) {
                        return `${(data * 100).toFixed(1)}%`;
                    }
                },
                {
                    data: 'id',
                    render: function(data, type, row) {
                        return `
                            <div class="btn-group btn-group-sm">
                                <button class="btn btn-primary" onclick="editAutomationRule(${data})">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="btn btn-${row.isActive ? 'warning' : 'success'}" onclick="toggleAutomationRule(${data})">
                                    <i class="fas fa-${row.isActive ? 'pause' : 'play'}"></i>
                                </button>
                                <button class="btn btn-danger" onclick="deleteAutomationRule(${data})">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </div>
                        `;
                    }
                }
            ],
            responsive: true,
            pageLength: 10,
            order: [[0, 'asc']]
        });
    }

    async loadAutomationRules() {
        try {
            $('#automationRulesTable').DataTable().ajax.reload();
        } catch (error) {
            console.error('Error loading automation rules:', error);
            this.showError('Failed to load automation rules');
        }
    }

    refreshCurrentTab() {
        const activeTab = $('.nav-link.active').attr('data-bs-target');
        this.handleTabSwitch(activeTab);
    }

    async apiCall(endpoint, method = 'GET', data = null) {
        const options = {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        };

        if (data && method !== 'GET') {
            options.body = JSON.stringify(data);
        }

        const response = await fetch(`${this.baseUrl}/${endpoint}`, options);
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return await response.json();
    }

    showSuccess(message) {
        toastr.success(message);
    }

    showError(message) {
        toastr.error(message);
    }

    showInfo(message) {
        toastr.info(message);
    }
}

// Action functions for buttons
async function approveRequest(requestId) {
    if (confirm('Are you sure you want to approve this request?')) {
        try {
            const response = await unifiedActions.apiCall('ProcessManagerAction', 'POST', {
                actionType: 'Approve',
                targetId: requestId
            });
            
            if (response.success) {
                unifiedActions.showSuccess('Request approved successfully');
                unifiedActions.loadPendingApprovals();
            }
        } catch (error) {
            unifiedActions.showError('Failed to approve request');
        }
    }
}

async function rejectRequest(requestId) {
    const reason = prompt('Please provide a reason for rejection:');
    if (reason) {
        try {
            const response = await unifiedActions.apiCall('ProcessManagerAction', 'POST', {
                actionType: 'Reject',
                targetId: requestId,
                reason: reason
            });
            
            if (response.success) {
                unifiedActions.showSuccess('Request rejected');
                unifiedActions.loadPendingApprovals();
            }
        } catch (error) {
            unifiedActions.showError('Failed to reject request');
        }
    }
}

async function assignAsset(assignmentId) {
    try {
        const response = await unifiedActions.apiCall('ProcessITSupportAction', 'POST', {
            actionType: 'AssignAsset',
            targetId: assignmentId
        });
        
        if (response.success) {
            unifiedActions.showSuccess('Asset assigned successfully');
            unifiedActions.loadAssetAssignments();
        }
    } catch (error) {
        unifiedActions.showError('Failed to assign asset');
    }
}

async function startMaintenance(taskId) {
    try {
        const response = await unifiedActions.apiCall('ProcessITSupportAction', 'POST', {
            actionType: 'StartMaintenance',
            targetId: taskId
        });
        
        if (response.success) {
            unifiedActions.showSuccess('Maintenance task started');
            unifiedActions.loadMaintenanceTasks();
        }
    } catch (error) {
        unifiedActions.showError('Failed to start maintenance');
    }
}

function createNewAutomationRule() {
    $('#newAutomationRuleModal').modal('show');
}

async function saveAutomationRule() {
    const formData = new FormData(document.getElementById('newAutomationRuleForm'));
    const ruleData = Object.fromEntries(formData);
    
    try {
        const response = await unifiedActions.apiCall('CreateAutomationRule', 'POST', ruleData);
        
        if (response.success) {
            unifiedActions.showSuccess('Automation rule created successfully');
            $('#newAutomationRuleModal').modal('hide');
            unifiedActions.loadAutomationRules();
        }
    } catch (error) {
        unifiedActions.showError('Failed to create automation rule');
    }
}

async function toggleAutomationRule(ruleId) {
    try {
        const response = await unifiedActions.apiCall('ToggleAutomationRule', 'POST', {
            ruleId: ruleId
        });
        
        if (response.success) {
            unifiedActions.showSuccess('Automation rule toggled');
            unifiedActions.loadAutomationRules();
        }
    } catch (error) {
        unifiedActions.showError('Failed to toggle automation rule');
    }
}

// Quick actions
async function performBulkAssetUpdate() {
    unifiedActions.showInfo('Initiating bulk asset update...');
    // Implementation for bulk asset update
}

async function triggerInventoryAudit() {
    unifiedActions.showInfo('Starting inventory audit...');
    // Implementation for inventory audit
}

async function generateRecommendations() {
    unifiedActions.showInfo('Generating intelligent recommendations...');
    // Implementation for generating recommendations
}

async function optimizeWorkflows() {
    unifiedActions.showInfo('Analyzing and optimizing workflows...');
    // Implementation for workflow optimization
}

// Global functions for easy access
function refreshActions() {
    unifiedActions.refreshCurrentTab();
}

function loadPendingApprovals() {
    unifiedActions.loadPendingApprovals();
}

function loadStrategicDecisions() {
    unifiedActions.loadStrategicDecisions();
}

function loadAssetAssignments() {
    unifiedActions.loadAssetAssignments();
}

function loadMaintenanceTasks() {
    unifiedActions.loadMaintenanceTasks();
}

function loadUrgentIssues() {
    unifiedActions.loadUrgentIssues();
}

// Initialize when DOM is ready
const unifiedActions = new UnifiedActions();
