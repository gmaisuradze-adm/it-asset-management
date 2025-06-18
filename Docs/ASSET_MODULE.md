# Asset Module - Hospital IT Asset Tracking System

## 🚨 **N1 CRITICAL INTEGRATION MANDATE** 🚨

### **Asset Module: Core of Four-Module Maximum Efficiency System**

**This Asset Module is the authoritative lifecycle management component of the N1 critical four-module integrated system.** It CANNOT and MUST NOT operate independently - it is designed to work in complete harmony with:

- **←→ Warehouse/Inventory Module**: Receives equipment that becomes tracked assets; supplies components for maintenance
- **←→ Request Module**: Processes service requests for asset maintenance, repairs, and replacements  
- **←→ Procurement Module**: Receives newly acquired equipment; triggers replacement purchases

### **Integration Requirements for Maximum Efficiency:**
- **Real-time cross-module data synchronization** - Asset status changes immediately update related modules
- **Automated workflow triggers** - Asset lifecycle events automatically initiate actions in other modules
- **Unified business process flow** - Every asset operation considers impacts across all four modules
- **Streamlined IT equipment processes** - Zero redundant data entry, maximum operational throughput

**CRITICAL**: Any attempt to use this Asset Module without full integration with the other three modules will result in system failure to meet hospital IT asset management objectives. The four modules are architected as ONE unified solution for maximum efficiency.

---

## Overview
This module manages all IT assets in the hospital, including their lifecycle, movement, maintenance, and audit logging. It serves as the central hub for tracking all deployed IT equipment from initial deployment through end-of-life disposal. The module is built using ASP.NET Core 8.0, Entity Framework Core, and follows best practices for security, validation, and error handling.

## Key Components
- **Models**: Asset, AssetMovement, MaintenanceRecord, AuditLog
- **Services**: AssetService (implements IAssetService)
- **Controllers**: AssetsController
- **Views**: Responsive Bootstrap 5 views for asset management
- **Integration Points**: Direct connections to Warehouse, Request, and Procurement modules

## Features
- Full CRUD for assets with complete lifecycle management
- Asset tag uniqueness validation across all modules
- Status and movement tracking with cross-module synchronization
- Maintenance scheduling and warranty tracking
- Audit logging for all changes with cross-module references
- Role-based authorization and security
- Export and reporting capabilities with integrated data
- Real-time integration with Warehouse deployments
- Automated asset creation from Request fulfillment
- Procurement-linked asset registration and warranty tracking

## Testing
Unit tests for the service layer are located in `/Tests/AssetServiceTests.cs` and cover core business logic.

## Security
- All actions are protected with [Authorize] attributes and role checks
- Input validation and anti-forgery tokens are enforced
- Audit logs are maintained for all data changes

## Error Handling
- All exceptions are logged
- User-friendly error messages are shown in the UI
- Null checks and nullable reference handling are enforced throughout the codebase

## Contribution
- Follow the repository pattern and service interface conventions
- Add unit tests for new business logic
- Update this documentation with any architectural or business logic changes

---
For more details, see inline code comments and the main README.md.
