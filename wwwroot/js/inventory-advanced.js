/**
 * Advanced Inventory Management JavaScript
 * Handles search, filtering, bulk operations, and interactive features
 */

// Global variables
let selectedItems = new Set();
let isCompactView = false;
let sortDirection = {};

// Initialize the page functionality
function initializeAdvancedInventory() {
    console.log('Initializing Advanced Inventory Management...');
    
    // Initialize event handlers
    initializeEventHandlers();
    initializeQuickFilters();
    initializeBulkOperations();
    initializeDataTable();
    initializeTooltips();
    
    // Set initial sort indicators
    updateSortIndicators();
    
    console.log('Advanced Inventory Management initialized successfully');
}

// Initialize all event handlers
function initializeEventHandlers() {
    // Search form submission
    $('#searchForm').on('submit', function(e) {
        // Reset page number when searching
        $('input[name="PageNumber"]').val(1);
        showLoadingOverlay();
    });

    // Real-time search with debounce
    let searchTimeout;
    $('#searchInput').on('input', function() {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            if ($(this).val().length > 2 || $(this).val().length === 0) {
                $('#searchForm').submit();
            }
        }, 500);
    });

    // Select all checkbox
    $('#selectAll').on('change', function() {
        const isChecked = $(this).is(':checked');
        $('.row-select').prop('checked', isChecked);
        updateSelectedItems();
    });

    // Individual row selection
    $(document).on('change', '.row-select', function() {
        updateSelectedItems();
        
        // Update select all checkbox
        const totalRows = $('.row-select').length;
        const selectedRows = $('.row-select:checked').length;
        
        $('#selectAll').prop('indeterminate', selectedRows > 0 && selectedRows < totalRows);
        $('#selectAll').prop('checked', selectedRows === totalRows);
    });

    // Sortable column headers
    $('.sortable').on('click', function() {
        const sortBy = $(this).data('sort');
        const currentSort = $('input[name="SortBy"]').val();
        const currentOrder = $('input[name="SortOrder"]').val();
        
        let newOrder = 'asc';
        if (currentSort === sortBy && currentOrder === 'asc') {
            newOrder = 'desc';
        }
        
        $('input[name="SortBy"]').val(sortBy);
        $('input[name="SortOrder"]').val(newOrder);
        $('input[name="PageNumber"]').val(1);
        
        $('#searchForm').submit();
    });

    // Bulk operation type change
    $('#bulkOperationType').on('change', function() {
        updateBulkOperationFields();
    });

    // Quick filter buttons
    $('.quick-filter-btn').on('click', function() {
        const filterName = $(this).data('filter');
        applyQuickFilter(filterName);
    });

    // Form validation
    $('form').on('submit', function() {
        validateForm(this);
    });
}

// Initialize quick filters
function initializeQuickFilters() {
    $('.quick-filter-btn').each(function() {
        $(this).on('click', function(e) {
            e.preventDefault();
            
            // Toggle active state
            $(this).toggleClass('active');
            
            // If it's active, apply the filter
            if ($(this).hasClass('active')) {
                // Remove active state from other filters
                $('.quick-filter-btn').not(this).removeClass('active');
                
                const filterName = $(this).data('filter');
                applyQuickFilter(filterName);
            } else {
                // Clear filter
                clearAllFilters();
            }
        });
    });
}

// Apply a quick filter
function applyQuickFilter(filterName) {
    // Clear existing filters first
    resetForm();
    
    switch (filterName) {
        case 'Low Stock Items':
            $('select[name="StockLevelFilter"]').val('2'); // LowStock
            break;
        case 'Critical Stock Items':
            $('select[name="StockLevelFilter"]').val('3'); // CriticalStock
            break;
        case 'High Value Items':
            $('input[name="MinTotalValue"]').val('1000');
            break;
        case 'Expiring Warranty':
            $('input[name="ShowExpiringWarrantyOnly"]').prop('checked', true);
            break;
    }
    
    // Submit the form
    $('#searchForm').submit();
}

// Initialize bulk operations
function initializeBulkOperations() {
    // Update fields when operation type changes
    $('#bulkOperationType').on('change', updateBulkOperationFields);
}

// Initialize DataTable features (if using DataTables)
function initializeDataTable() {
    // This would be implemented if we were using DataTables library
    // For now, we're using server-side pagination
    console.log('DataTable features initialized');
}

// Initialize tooltips
function initializeTooltips() {
    // Initialize Bootstrap tooltips
    if (typeof bootstrap !== 'undefined') {
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
}

// Update selected items tracking
function updateSelectedItems() {
    selectedItems.clear();
    
    $('.row-select:checked').each(function() {
        selectedItems.add({
            id: $(this).val(),
            name: $(this).data('item-name')
        });
    });
    
    $('#selectedCount').text(selectedItems.size);
    
    // Show/hide bulk actions panel based on selection
    if (selectedItems.size > 0 && $('#bulkActionsPanel').hasClass('d-none')) {
        $('#bulkActionsPanel').removeClass('d-none');
    }
}

// Toggle bulk actions panel
function toggleBulkActions() {
    $('#bulkActionsPanel').toggleClass('d-none');
    
    if (!$('#bulkActionsPanel').hasClass('d-none')) {
        $('#bulkToggleBtn').html('<i class="fas fa-times"></i> Hide Bulk Actions');
    } else {
        $('#bulkToggleBtn').html('<i class="fas fa-tasks"></i> Bulk Actions');
    }
}

// Update bulk operation fields based on selected operation
function updateBulkOperationFields() {
    const operationType = $('#bulkOperationType').val();
    const valueColumn = $('#bulkValueColumn');
    const reasonColumn = $('#bulkReasonColumn');
    const valueLabel = $('#bulkValueLabel');
    const valueSelect = $('#bulkValue');
    
    // Hide all additional fields first
    valueColumn.hide();
    reasonColumn.hide();
    valueSelect.empty();
    
    switch (operationType) {
        case '1': // Update Status
            valueLabel.text('New Status');
            populateStatusOptions(valueSelect);
            valueColumn.show();
            break;
            
        case '2': // Update Condition
            valueLabel.text('New Condition');
            populateConditionOptions(valueSelect);
            valueColumn.show();
            break;
            
        case '3': // Update Location
            valueLabel.text('New Location');
            populateLocationOptions(valueSelect);
            valueColumn.show();
            reasonColumn.show();
            break;
            
        case '4': // Adjust Stock
            valueLabel.text('Adjustment Type');
            populateAdjustmentOptions(valueSelect);
            valueColumn.show();
            reasonColumn.show();
            break;
            
        case '5': // Transfer Items
            valueLabel.text('Transfer To');
            populateLocationOptions(valueSelect);
            valueColumn.show();
            reasonColumn.show();
            break;
            
        case '6': // Export Selected
            // No additional fields needed
            break;
    }
}

// Populate status options
function populateStatusOptions(select) {
    const statuses = [
        { value: '0', text: 'In Stock' },
        { value: '1', text: 'Reserved' },
        { value: '2', text: 'Allocated' },
        { value: '3', text: 'In Transit' },
        { value: '4', text: 'Deployed' },
        { value: '5', text: 'On Loan' }
    ];
    
    statuses.forEach(status => {
        select.append(`<option value="${status.value}">${status.text}</option>`);
    });
}

// Populate condition options
function populateConditionOptions(select) {
    const conditions = [
        { value: '0', text: 'New' },
        { value: '1', text: 'Excellent' },
        { value: '2', text: 'Good' },
        { value: '3', text: 'Fair' },
        { value: '4', text: 'Poor' },
        { value: '5', text: 'Defective' }
    ];
    
    conditions.forEach(condition => {
        select.append(`<option value="${condition.value}">${condition.text}</option>`);
    });
}

// Populate location options
function populateLocationOptions(select) {
    // This would typically be populated from the server
    // For now, we'll use a placeholder
    select.append('<option value="">Select Location...</option>');
    
    // In a real implementation, this would be an AJAX call
    // to get available locations
}

// Populate adjustment type options
function populateAdjustmentOptions(select) {
    const adjustments = [
        { value: '1', text: 'Increase Stock' },
        { value: '2', text: 'Decrease Stock' },
        { value: '3', text: 'Set Absolute Quantity' }
    ];
    
    adjustments.forEach(adjustment => {
        select.append(`<option value="${adjustment.value}">${adjustment.text}</option>`);
    });
}

// Execute bulk operation
function executeBulkOperation() {
    const operationType = $('#bulkOperationType').val();
    
    if (!operationType) {
        showAlert('Please select a bulk operation type.', 'warning');
        return;
    }
    
    if (selectedItems.size === 0) {
        showAlert('Please select items to perform bulk operation.', 'warning');
        return;
    }
    
    // Confirm operation
    const itemCount = selectedItems.size;
    const operationName = $('#bulkOperationType option:selected').text();
    
    if (!confirm(`Are you sure you want to perform "${operationName}" on ${itemCount} selected items?`)) {
        return;
    }
    
    // Prepare data
    const bulkData = {
        SelectedItemIds: Array.from(selectedItems).map(item => parseInt(item.id)),
        OperationType: parseInt(operationType),
        Reason: $('#bulkReason').val()
    };
    
    // Add operation-specific data
    const bulkValue = $('#bulkValue').val();
    if (bulkValue) {
        switch (operationType) {
            case '1': // Status
                bulkData.NewStatus = parseInt(bulkValue);
                break;
            case '2': // Condition
                bulkData.NewCondition = parseInt(bulkValue);
                break;
            case '3': // Location
                bulkData.NewLocationId = parseInt(bulkValue);
                break;
        }
    }
    
    // Show loading
    showLoadingOverlay();
    
    // Execute bulk operation
    $.ajax({
        url: '/Inventory/ExecuteBulkOperation',
        type: 'POST',
        data: JSON.stringify(bulkData),
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(result) {
            hideLoadingOverlay();
            
            if (result.success) {
                showAlert(`Bulk operation completed successfully. ${result.successfulItems} items processed.`, 'success');
                
                // Refresh the page
                window.location.reload();
            } else {
                showAlert(`Bulk operation failed: ${result.message}`, 'danger');
                
                if (result.errorMessages && result.errorMessages.length > 0) {
                    console.error('Bulk operation errors:', result.errorMessages);
                }
            }
        },
        error: function(xhr, status, error) {
            hideLoadingOverlay();
            showAlert('An error occurred while executing the bulk operation.', 'danger');
            console.error('Bulk operation error:', error);
        }
    });
}

// Clear selection
function clearSelection() {
    selectedItems.clear();
    $('.row-select').prop('checked', false);
    $('#selectAll').prop('checked', false).prop('indeterminate', false);
    $('#selectedCount').text('0');
    $('#bulkActionsPanel').addClass('d-none');
    $('#bulkToggleBtn').html('<i class="fas fa-tasks"></i> Bulk Actions');
}

// Show stock adjustment modal
function showStockAdjustment(itemId, itemName) {
    $('#adjustItemId').val(itemId);
    $('#adjustItemName').text(itemName);
    
    // Get current stock (this would typically be an AJAX call)
    $('#currentStock').text('Loading...');
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('stockAdjustmentModal'));
    modal.show();
    
    // In a real implementation, you would fetch current stock via AJAX
    setTimeout(() => {
        $('#currentStock').text('25 units'); // Example
    }, 500);
}

// Submit stock adjustment
function submitStockAdjustment() {
    const itemId = $('#adjustItemId').val();
    const adjustmentType = $('#adjustmentType').val();
    const quantity = $('#adjustmentQuantity').val();
    const reason = $('#adjustmentReason').val();
    
    if (!quantity || !reason) {
        showAlert('Please fill in all required fields.', 'warning');
        return;
    }
    
    const adjustmentData = {
        SelectedItemIds: [parseInt(itemId)],
        OperationType: 4, // Adjust Stock
        AdjustmentType: parseInt(adjustmentType),
        QuantityAdjustment: parseInt(quantity),
        Reason: reason
    };
    
    showLoadingOverlay();
    
    $.ajax({
        url: '/Inventory/ExecuteBulkOperation',
        type: 'POST',
        data: JSON.stringify(adjustmentData),
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(result) {
            hideLoadingOverlay();
            
            if (result.success) {
                showAlert('Stock adjustment completed successfully.', 'success');
                
                // Close modal
                bootstrap.Modal.getInstance(document.getElementById('stockAdjustmentModal')).hide();
                
                // Refresh the page
                window.location.reload();
            } else {
                showAlert(`Stock adjustment failed: ${result.message}`, 'danger');
            }
        },
        error: function(xhr, status, error) {
            hideLoadingOverlay();
            showAlert('An error occurred while adjusting stock.', 'danger');
            console.error('Stock adjustment error:', error);
        }
    });
}

// Export functions
function exportInventory(format) {
    const searchData = $('#searchForm').serialize();
    
    showLoadingOverlay();
    
    const exportUrl = `/Inventory/ExportInventory?format=${format}&${searchData}`;
    
    // Create hidden link to trigger download
    const link = document.createElement('a');
    link.href = exportUrl;
    link.download = `inventory_export_${new Date().toISOString().split('T')[0]}.${format}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    // Hide loading after a delay
    setTimeout(() => {
        hideLoadingOverlay();
        showAlert(`Export started. Your ${format.toUpperCase()} file will download shortly.`, 'info');
    }, 1000);
}

// Pagination functions
function changePage(pageNumber) {
    $('input[name="PageNumber"]').val(pageNumber);
    $('#searchForm').submit();
}

function changePageSize(pageSize) {
    $('input[name="PageSize"]').val(pageSize);
    $('input[name="PageNumber"]').val(1);
    $('#searchForm').submit();
}

// Toggle compact view
function toggleCompactView() {
    isCompactView = !isCompactView;
    
    if (isCompactView) {
        $('#inventoryTable').addClass('table-sm');
        $('.table-responsive').addClass('compact-view');
    } else {
        $('#inventoryTable').removeClass('table-sm');
        $('.table-responsive').removeClass('compact-view');
    }
}

// Clear all filters
function clearAllFilters() {
    resetForm();
    $('#searchForm').submit();
}

// Reset advanced filters
function resetAdvancedFilters() {
    // Clear all advanced filter fields
    $('#advancedFilters input').val('');
    $('#advancedFilters select').val('');
    $('#advancedFilters input[type="checkbox"]').prop('checked', false);
}

// Save search (placeholder function)
function saveSearch() {
    const searchName = prompt('Enter a name for this search:');
    if (searchName) {
        // This would save the current search criteria
        showAlert(`Search "${searchName}" saved successfully.`, 'success');
    }
}

// Utility functions
function resetForm() {
    $('#searchForm')[0].reset();
    $('.quick-filter-btn').removeClass('active');
}

function updateSortIndicators() {
    const currentSort = $('input[name="SortBy"]').val();
    const currentOrder = $('input[name="SortOrder"]').val();
    
    // Reset all sort indicators
    $('.sortable i').removeClass('fa-sort-up fa-sort-down').addClass('fa-sort');
    
    // Set current sort indicator
    if (currentSort) {
        const sortIcon = $(`.sortable[data-sort="${currentSort}"] i`);
        sortIcon.removeClass('fa-sort');
        
        if (currentOrder === 'asc') {
            sortIcon.addClass('fa-sort-up');
        } else {
            sortIcon.addClass('fa-sort-down');
        }
    }
}

function showLoadingOverlay() {
    $('#loadingOverlay').removeClass('d-none');
}

function hideLoadingOverlay() {
    $('#loadingOverlay').addClass('d-none');
}

function showAlert(message, type = 'info') {
    // Create alert element
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    // Show at top of page
    $('body').prepend(alertHtml);
    
    // Auto-hide after 5 seconds
    setTimeout(() => {
        $('.alert').first().fadeOut(() => {
            $(this).remove();
        });
    }, 5000);
}

function validateForm(form) {
    // Basic form validation
    let isValid = true;
    
    $(form).find('input[required], select[required], textarea[required]').each(function() {
        if (!$(this).val()) {
            $(this).addClass('is-invalid');
            isValid = false;
        } else {
            $(this).removeClass('is-invalid');
        }
    });
    
    return isValid;
}

// Initialize when document is ready
$(document).ready(function() {
    // Auto-initialize if the function exists
    if (typeof initializeAdvancedInventory === 'function') {
        initializeAdvancedInventory();
    }
});
