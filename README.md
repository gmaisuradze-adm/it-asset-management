# Hospital IT Asset Tracking System

## 🚨 **N1 CRITICAL INTEGRATION REQUIREMENT** 🚨

### **Four-Module Maximum Efficiency System**

This Hospital IT Asset Tracking System is built around the **N1 CRITICAL REQUIREMENT** that all four modules (Asset, Inventory/Warehouse, Request, and Procurement) **MUST work together in complete harmony for maximum efficiency**. 

**Key Integration Principles:**
- **Streamlined IT equipment request processes** - From request to deployment in minimal time
- **Optimal business process design** - Zero redundancy, maximum operational throughput
- **Maximum operational efficiency** - Real-time cross-module data synchronization
- **Unified ecosystem approach** - All modules operate as ONE integrated solution

**This system cannot achieve its objectives unless all four modules are implemented and operate together as designed.**

---

## 🏥 **Comprehensive Web Application**

A complete IT asset management solution built with ASP.NET Core 8.0 for managing and tracking all IT equipment in hospital environments through four integrated modules:

1. **Asset Module** - Complete lifecycle management of all IT equipment
2. **Inventory/Warehouse Module** - Stock management and supply chain control  
3. **Request Module** - Centralized IT service and equipment request processing
4. **Procurement Module** - Streamlined acquisition and vendor management

## ✨ **Core Features**

### **Integrated Asset Management**
- **Complete Asset Registration**: Track all IT equipment with detailed lifecycle information
- **Cross-Module Integration**: Assets automatically created from procurement, linked to inventory
- **Location Management**: Building, floor, room-based tracking with movement history
- **Automated Workflows**: Status changes trigger appropriate actions in other modules
- **Maintenance Integration**: Maintenance requests automatically check inventory for parts

### **Advanced Four-Module Features**
- **Unified Request Processing**: Single interface for all IT needs with intelligent routing
- **Automated Procurement**: Low stock levels trigger purchase orders automatically  
- **Real-Time Inventory**: Live stock levels visible across all modules
- **Complete Audit Trail**: Full transparency across all module interactions
- **Integrated Reporting**: Cross-module analytics and performance metrics

## Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, jQuery, DataTables, Chart.js
- **Reports**: ClosedXML (Excel)
- **QR Codes**: QRCoder library

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 12+
- Visual Studio Code or Visual Studio 2022

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd HospitalAssetTracker
   ```

2. **Set up PostgreSQL**
   - Install PostgreSQL
   - Create a database named `HospitalAssetTracker`
   - Update connection string in `appsettings.json`

3. **Install dependencies**
   ```bash
   dotnet restore
   ```

4. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the application**
   - Open browser to `https://localhost:5001`
   - Default admin login: `admin@hospital.com` / `Admin123!`
   - Default IT support login: `itsupport@hospital.com` / `ITSupport123!`

## Database Schema

### Core Tables
- **Assets**: Main asset information
- **Locations**: Building/floor/room structure
- **AssetMovements**: Movement history
- **MaintenanceRecords**: Maintenance and repair history
- **AuditLogs**: Complete audit trail
- **ApplicationUsers**: Extended user information

## User Roles

### Admin
- Full system access
- User management
- System configuration
- All reports and audit logs

### IT Support
- Asset management
- Location assignments
- Maintenance scheduling
- Standard reports

### Asset Manager
- Asset lifecycle management
- Reporting and analytics
- Maintenance oversight

### Department Head
- View departmental assets
- Generate reports
- Request asset movements

### User
- View assigned assets
- Basic asset information

## Configuration

### Connection String
Update `appsettings.json` with your PostgreSQL connection:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=HospitalAssetTracker;Username=your_user;Password=your_password"
  }
}
```

### Email Settings (Optional)
Configure SMTP settings for notifications:
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.hospital.com",
    "SmtpPort": 587,
    "Username": "noreply@hospital.com",
    "Password": "email_password"
  }
}
```

## API Endpoints

The system provides RESTful endpoints for integration:

- `GET /api/assets` - List all assets
- `GET /api/assets/{id}` - Get specific asset
- `POST /api/assets` - Create new asset
- `PUT /api/assets/{id}` - Update asset
- `DELETE /api/assets/{id}` - Delete asset
- `GET /api/locations` - List all locations
- `GET /api/reports/dashboard` - Dashboard data

## Security Features

- **Authentication**: ASP.NET Core Identity
- **Authorization**: Role-based access control
- **Audit Logging**: All actions logged with user details
- **HTTPS**: SSL/TLS encryption required
- **CSRF Protection**: Anti-forgery tokens
- **SQL Injection Prevention**: Entity Framework parameterized queries

## Reporting

### Standard Reports
- Asset inventory by category/location/status
- Maintenance schedules and history
- Warranty expiration reports
- Asset movement audit trails

### Export Formats
- **Excel**: Detailed asset listings with filtering
- **PDF**: Formatted reports for printing and distribution

## Maintenance

### Regular Tasks
- Database backup (recommended daily)
- Log file rotation
- Performance monitoring
- Security updates

### Monitoring
- Application logs via ASP.NET Core logging
- Database performance metrics
- User activity tracking
- System health checks

## Troubleshooting

### Common Issues

**Database Connection Failed**
- Verify PostgreSQL service is running
- Check connection string credentials
- Ensure database exists

**Migration Errors**
```bash
dotnet ef database drop
dotnet ef database update
```

**Permission Denied**
- Check user roles assignment
- Verify authentication status
- Review audit logs for access attempts

## Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support and questions:
- Create an issue in the repository
- Contact the IT department
- Review the documentation wiki

## Changelog

### Version 1.0.0
- Initial release
- Complete asset management system
- User role management
- Reporting capabilities
- Audit logging system

---

**Hospital IT Asset Tracking System** - Comprehensive IT asset management for healthcare environments.
