<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# Hospital IT Asset Tracking System - Copilot Instructions

This is an ASP.NET Core 8.0 web application for managing IT assets in a hospital environment.

## Project Context

- **Purpose**: Track approximately 1,400 IT assets including desktops, laptops, printers, network devices, and servers
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: ASP.NET Core Identity with role-based authorization
- **Frontend**: Bootstrap 5, jQuery, DataTables, Chart.js
- **Architecture**: MVC pattern with service layer

## Key Models

- **Asset**: Core entity with category, brand, model, serial number, status, location
- **Location**: Building/floor/room structure for asset placement
- **AssetMovement**: History of asset transfers and movements
- **MaintenanceRecord**: Maintenance and repair history
- **AuditLog**: Complete audit trail of all system changes
- **ApplicationUser**: Extended identity user with hospital-specific fields

## User Roles & Permissions

- **Admin**: Full system access, user management, system configuration
- **IT Support**: Asset management, assignments, maintenance scheduling
- **Asset Manager**: Asset lifecycle management, reporting
- **Department Head**: View departmental assets, generate reports
- **User**: View assigned assets only

## Coding Standards

### Controllers
- Use dependency injection for services
- Implement proper authorization attributes
- Return appropriate HTTP status codes
- Use async/await patterns
- Include audit logging for data changes

### Services
- Follow repository pattern
- Use interfaces for dependency injection
- Implement proper error handling
- Include audit trails for all data modifications
- Use Entity Framework async methods

### Views
- Use Bootstrap 5 components
- Implement responsive design
- Include proper form validation
- Use DataTables for data grids
- Include export functionality (Excel/PDF)

### Security
- Always use [Authorize] attributes with appropriate roles
- Validate user input
- Use anti-forgery tokens
- Log security-related events
- Implement proper error handling without exposing sensitive data

## Database Patterns

- Use Entity Framework migrations
- Follow proper foreign key relationships
- Include proper indexes for performance
- Use soft deletes where appropriate
- Maintain audit trails

## Feature Requirements

### Asset Management
- Complete CRUD operations
- Asset tag uniqueness validation
- Status change tracking
- Movement history
- Maintenance scheduling
- Warranty tracking

### Reporting
- Export to Excel and PDF
- Dashboard with charts and metrics
- Audit log reports
- Maintenance reports
- Warranty expiration alerts

### Search & Filtering
- Fast text search across asset attributes
- Filter by category, status, location
- Advanced search capabilities
- Export filtered results

## UI/UX Guidelines

- Use Hospital/Medical themed colors and icons
- Implement user-friendly forms with validation
- Provide clear navigation and breadcrumbs
- Use appropriate Bootstrap components
- Include loading states and progress indicators
- Show success/error messages clearly

## Performance Considerations

- Use pagination for large data sets
- Implement proper caching where appropriate
- Optimize database queries
- Use async operations
- Minimize N+1 query problems

## Common Patterns

When implementing new features:
1. Create service interface and implementation
2. Add controller with proper authorization
3. Create responsive views with validation
4. Include audit logging
5. Add unit tests
6. Update documentation

## Error Handling

- Use try-catch blocks appropriately
- Log errors with sufficient context
- Return user-friendly error messages
- Don't expose sensitive information
- Handle database connection failures gracefully
