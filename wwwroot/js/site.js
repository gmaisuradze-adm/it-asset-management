// Site-wide JavaScript for Hospital Asset Tracker - Enhanced for .NET 9

// Initialize Bootstrap components and modern features
document.addEventListener('DOMContentLoaded', function() {
    initializeBootstrapComponents();
    initializeModernFeatures();
    setupProgressiveEnhancements();
});

// Initialize all Bootstrap components
function initializeBootstrapComponents() {
    // Initialize Bootstrap tooltips with improved options
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => {
        return new bootstrap.Tooltip(tooltipTriggerEl, {
            trigger: 'hover focus',
            animation: true,
            delay: { show: 300, hide: 100 }
        });
    });

    // Initialize Bootstrap popovers with modern options
    const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
    const popoverList = [...popoverTriggerList].map(popoverTriggerEl => {
        return new bootstrap.Popover(popoverTriggerEl, {
            trigger: 'click',
            html: true,
            sanitize: true
        });
    });

    // Auto-hide alerts with fade animation
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert.close();
        }, 5000);
    });
}

// Initialize modern web features
function initializeModernFeatures() {
    // Intersection Observer for lazy loading
    if ('IntersectionObserver' in window) {
        const lazyImages = document.querySelectorAll('img[data-src]');
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    imageObserver.unobserve(img);
                }
            });
        });
        lazyImages.forEach(img => imageObserver.observe(img));
    }

    // Service Worker registration for PWA features
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('/sw.js').catch(err => {
            console.log('Service Worker registration failed:', err);
        });
    }

    // Dark mode preference detection
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        document.body.classList.add('dark-mode-supported');
    }
}

// Progressive enhancement features
function setupProgressiveEnhancements() {
    // Enhanced form validation
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });

    // Automatic table sorting enhancement
    const tables = document.querySelectorAll('table.sortable');
    tables.forEach(table => {
        if (!table.classList.contains('dataTable')) {
            enhanceTableSorting(table);
        }
    });
}

// Enhanced Asset Tracker utility object with modern ES6+ features
const AssetTracker = {
    // Enhanced confirmation with modern styling
    confirmDelete: function(message, options = {}) {
        const defaultMessage = 'Are you sure you want to delete this item? This action cannot be undone.';
        
        // If browser supports modern dialog, use it
        if (window.confirm) {
            return confirm(message || defaultMessage);
        }
        
        // Fallback to custom modal if available
        return this.showConfirmModal(message || defaultMessage, options);
    },

    // Modern currency formatting with Intl API
    formatCurrency: function(value, currency = 'USD', locale = 'en-US') {
        return new Intl.NumberFormat(locale, {
            style: 'currency',
            currency: currency,
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(value);
    },

    // Enhanced date formatting with relative time
    formatDate: function(dateString, options = {}) {
        const date = new Date(dateString);
        const defaultOptions = {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            ...options
        };
        
        // Check if date is recent for relative formatting
        if (options.relative) {
            const now = new Date();
            const diffTime = Math.abs(now - date);
            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
            
            if (diffDays === 0) return 'Today';
            if (diffDays === 1) return 'Yesterday';
            if (diffDays < 7) return `${diffDays} days ago`;
        }
        
        return date.toLocaleDateString('en-US', defaultOptions);
    },

    // Enhanced loading state with accessibility
    showLoading: function(element, text = 'Loading...') {
        const spinner = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>';
        const originalText = element.dataset.originalText || element.textContent;
        
        element.dataset.originalText = originalText;
        element.innerHTML = spinner + text;
        element.disabled = true;
        element.setAttribute('aria-busy', 'true');
    },

    // Enhanced loading cleanup
    hideLoading: function(element) {
        const originalText = element.dataset.originalText || element.textContent.replace(/^.*?(?=\w)/, '');
        element.innerHTML = originalText;
        element.disabled = false;
        element.removeAttribute('aria-busy');
        delete element.dataset.originalText;
    },

    // Modern fetch-based form submission
    async submitForm(form, options = {}) {
        const submitBtn = form.querySelector('button[type="submit"]');
        const formData = new FormData(form);
        
        try {
            this.showLoading(submitBtn, options.loadingText);
            
            const response = await fetch(form.action || window.location.href, {
                method: form.method || 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });
            
            if (response.ok) {
                const result = await response.json();
                if (options.successMessage) {
                    this.showAlert(options.successMessage, 'success');
                }
                return result;
            } else {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
        } catch (error) {
            this.showAlert(error.message || 'An error occurred while submitting the form.', 'danger');
            throw error;
        } finally {
            this.hideLoading(submitBtn);
        }
    },

    // Enhanced alert system with auto-dismiss and animations
    showAlert: function(message, type = 'info', options = {}) {
        const alertContainer = document.getElementById('alert-container') || 
                             document.querySelector('.alert-container') || 
                             document.body;
        
        const alertId = `alert-${Date.now()}`;
        const alertDiv = document.createElement('div');
        alertDiv.id = alertId;
        alertDiv.className = `alert alert-${type} alert-dismissible fade show ${options.className || ''}`;
        alertDiv.setAttribute('role', 'alert');
        
        alertDiv.innerHTML = `
            <i class="bi bi-${this.getAlertIcon(type)} me-2"></i>
            <span>${message}</span>
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;
        
        alertContainer.insertBefore(alertDiv, alertContainer.firstChild);
        
        // Auto-hide with customizable timeout
        const timeout = options.timeout !== undefined ? options.timeout : 5000;
        if (timeout > 0) {
            setTimeout(() => {
                const bsAlert = bootstrap.Alert.getOrCreateInstance(alertDiv);
                bsAlert.close();
            }, timeout);
        }
        
        return alertId;
    },

    // Enhanced icon mapping with more types
    getAlertIcon: function(type) {
        const icons = {
            'success': 'check-circle-fill',
            'danger': 'exclamation-triangle-fill',
            'error': 'exclamation-triangle-fill',
            'warning': 'exclamation-triangle-fill',
            'info': 'info-circle-fill',
            'primary': 'info-circle-fill',
            'secondary': 'gear-fill',
            'light': 'lightbulb-fill',
            'dark': 'moon-fill'
        };
        return icons[type] || 'info-circle-fill';
    },

    // Enhanced DataTables initialization with modern features
    initDataTable: function(tableId, options = {}) {
        const defaultOptions = {
            responsive: true,
            pageLength: 25,
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                 '<"row"<"col-sm-12"tr>>' +
                 '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Search assets...",
                lengthMenu: "Show _MENU_ entries",
                info: "Showing _START_ to _END_ of _TOTAL_ entries",
                infoEmpty: "No entries available",
                infoFiltered: "(filtered from _MAX_ total entries)",
                emptyTable: "No data available in table",
                loadingRecords: "Loading...",
                processing: "Processing...",
                paginate: {
                    first: "First",
                    last: "Last",
                    next: "Next",
                    previous: "Previous"
                }
            },
            processing: true,
            stateSave: true,
            stateDuration: 60 * 60 * 24, // 24 hours
            columnDefs: [
                {
                    targets: 'no-sort',
                    orderable: false
                },
                {
                    targets: 'date-sort',
                    type: 'date'
                }
            ]
        };

        const mergedOptions = { ...defaultOptions, ...options };
        const table = $(tableId).DataTable(mergedOptions);
        
        // Add export buttons if requested
        if (options.export) {
            new $.fn.dataTable.Buttons(table, {
                buttons: ['copy', 'csv', 'excel', 'pdf', 'print']
            });
            table.buttons().container().appendTo(`${tableId}_wrapper .col-md-6:eq(0)`);
        }
        
        return table;
    },

    // New utility functions for modern web features
    
    // Debounce function for performance optimization
    debounce: function(func, wait, immediate = false) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                timeout = null;
                if (!immediate) func.apply(this, args);
            };
            const callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(this, args);
        };
    },

    // Local storage with JSON support
    storage: {
        set: function(key, value) {
            try {
                localStorage.setItem(key, JSON.stringify(value));
                return true;
            } catch (e) {
                console.warn('Local storage not available:', e);
                return false;
            }
        },
        get: function(key, defaultValue = null) {
            try {
                const item = localStorage.getItem(key);
                return item ? JSON.parse(item) : defaultValue;
            } catch (e) {
                console.warn('Error reading from local storage:', e);
                return defaultValue;
            }
        },
        remove: function(key) {
            try {
                localStorage.removeItem(key);
                return true;
            } catch (e) {
                console.warn('Error removing from local storage:', e);
                return false;
            }
        }
    },

    // Theme management
    theme: {
        toggle: function() {
            const currentTheme = document.documentElement.getAttribute('data-bs-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            document.documentElement.setAttribute('data-bs-theme', newTheme);
            AssetTracker.storage.set('preferred-theme', newTheme);
            return newTheme;
        },
        
        set: function(theme) {
            document.documentElement.setAttribute('data-bs-theme', theme);
            AssetTracker.storage.set('preferred-theme', theme);
        },
        
        init: function() {
            const savedTheme = AssetTracker.storage.get('preferred-theme');
            const systemTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
            const theme = savedTheme || systemTheme;
            this.set(theme);
        }
    }
};

// Enhanced table sorting function
function enhanceTableSorting(table) {
    const headers = table.querySelectorAll('th[data-sort]');
    headers.forEach(header => {
        header.style.cursor = 'pointer';
        header.innerHTML += ' <i class="bi bi-arrow-down-up text-muted"></i>';
        
        header.addEventListener('click', () => {
            const column = header.dataset.sort;
            const direction = header.dataset.direction === 'asc' ? 'desc' : 'asc';
            header.dataset.direction = direction;
            
            // Simple client-side sorting implementation
            sortTableByColumn(table, column, direction);
        });
    });
}

// Simple table sorting implementation
function sortTableByColumn(table, column, direction) {
    const tbody = table.querySelector('tbody');
    const rows = Array.from(tbody.querySelectorAll('tr'));
    const columnIndex = [...table.querySelectorAll('th')].findIndex(th => th.dataset.sort === column);
    
    rows.sort((a, b) => {
        const aValue = a.cells[columnIndex]?.textContent.trim() || '';
        const bValue = b.cells[columnIndex]?.textContent.trim() || '';
        
        const comparison = aValue.localeCompare(bValue, undefined, { numeric: true });
        return direction === 'asc' ? comparison : -comparison;
    });
    
    rows.forEach(row => tbody.appendChild(row));
}

// Initialize theme on load
document.addEventListener('DOMContentLoaded', () => {
    AssetTracker.theme.init();
});

// Export for use in other scripts
window.AssetTracker = AssetTracker;
