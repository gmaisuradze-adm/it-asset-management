-- Insert sample bug tracking data
INSERT INTO bug_tracking (bug_title, bug_description, status, severity, module_name, error_message, reported_by, reported_date, version_found, created_at, updated_at)
VALUES 
('AssetDashboard Loading Error', 'Unable to load complete dashboard data. Please try again.', 'Fixed', 'High', 'Asset Management', 'PostgreSQL DateTime comparison issue', 'System Auto-Detection', CURRENT_TIMESTAMP, '1.0.0', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('Chart.js Version Compatibility', 'Chart.js v2 syntax causing rendering issues', 'Fixed', 'Medium', 'Asset Management', 'Chart.js syntax error', 'System Auto-Detection', CURRENT_TIMESTAMP - INTERVAL '1 day', '1.0.0', CURRENT_TIMESTAMP - INTERVAL '1 day', CURRENT_TIMESTAMP),
('Bootstrap 4 to 5 Migration', 'FontAwesome icons not displaying correctly', 'Fixed', 'Low', 'Frontend', 'Missing Bootstrap Icons', 'System Auto-Detection', CURRENT_TIMESTAMP - INTERVAL '2 days', '1.0.0', CURRENT_TIMESTAMP - INTERVAL '2 days', CURRENT_TIMESTAMP);

-- Insert system version
INSERT INTO system_versions (version_number, release_notes, bugs_fixed, features_added, release_date, is_current, created_by, created_at)
VALUES 
('1.1.0', 'Added Bug Tracking and Version Management System. Fixed AssetDashboard loading issues. Updated UI to Bootstrap 5.', 3, 2, CURRENT_TIMESTAMP, true, 'System', CURRENT_TIMESTAMP),
('1.0.0', 'Initial release of Hospital Asset Tracker', 0, 10, CURRENT_TIMESTAMP - INTERVAL '7 days', false, 'System', CURRENT_TIMESTAMP - INTERVAL '7 days');

-- Insert bug fix history
INSERT INTO bug_fix_history (bug_id, version_id, fix_details, files_changed, test_status, fixed_date, fixed_by)
VALUES 
(1, 1, 'Fixed PostgreSQL DateTime comparison issues in AssetBusinessLogicService', 'Services/AssetBusinessLogicService.cs', 'Passed', CURRENT_TIMESTAMP, 'System'),
(2, 1, 'Updated Chart.js to v3+ syntax and fixed responsive layout', 'Views/AssetDashboard/Index.cshtml', 'Passed', CURRENT_TIMESTAMP, 'System'),
(3, 1, 'Migrated from FontAwesome to Bootstrap Icons', 'Views/AssetDashboard/Index.cshtml, Views/Shared/_Layout.cshtml', 'Passed', CURRENT_TIMESTAMP, 'System');
