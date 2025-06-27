# Step 4: Advanced Asset Search & Management - Testing Protocol

## Overview
This document provides comprehensive testing procedures for the newly implemented advanced asset search, filtering, bulk operations, export functionality, and asset comparison features.

## Pre-Testing Setup

### Test Data Requirements
1. **Minimum 50 test assets** with varied data:
   - Different categories (Desktop, Laptop, Printer, Network, Server, Tablet, Monitor, Phone, Other)
   - Different statuses (Available, InUse, UnderMaintenance, MaintenancePending, InTransit, Reserved, Lost, Stolen, Decommissioned, PendingApproval)
   - Multiple locations and departments
   - Various price ranges ($100 - $5000+)
   - Mix of assigned and unassigned assets
   - Different warranty expiry dates (expired, expiring soon, future)

2. **Test Users with Different Roles**:
   - Admin user
   - IT Support user  
   - Asset Manager user
   - Department Head user
   - Regular User

### Database Setup
```sql
-- Sample test data creation commands
-- Run these to populate test data if needed
INSERT INTO Assets (AssetTag, Category, Brand, Model, Status, LocationId, PurchasePrice, WarrantyExpiry, Department)
VALUES 
('DSK-0001001', 0, 'Dell', 'OptiPlex 7090', 1, 1, 1299.99, '2025-12-31', 'IT'),
('LAP-0001002', 1, 'Lenovo', 'ThinkPad X1', 1, 2, 1899.99, '2024-06-30', 'Finance'),
-- Add more test data as needed...
```

## Test Plan Structure

### Phase 1: Advanced Search Interface Testing
### Phase 2: Bulk Operations Testing  
### Phase 3: Export Functionality Testing
### Phase 4: Asset Comparison Testing
### Phase 5: Performance & Security Testing
### Phase 6: Browser Compatibility Testing

---

## Phase 1: Advanced Search Interface Testing

### Test 1.1: Basic Search Functionality
**Objective**: Verify basic text search across multiple fields

**Test Steps**:
1. Navigate to `/Assets/IndexAdvanced`
2. Enter search term "Dell" in search box
3. Click Search button
4. Verify results show only assets containing "Dell"
5. Repeat with other terms: asset tags, model numbers, serial numbers

**Expected Results**:
- Search results filter correctly
- Search highlights matching terms
- Results show proper pagination
- Loading indicators appear during search

**Test Data**: Search for "Dell", "ThinkPad", specific asset tag, serial number

### Test 1.2: Category Filtering
**Objective**: Test category filter functionality

**Test Steps**:
1. Select "Desktop" from category dropdown
2. Click Apply Filters
3. Verify only desktop assets are shown
4. Select multiple categories using checkboxes
5. Verify combined results

**Expected Results**:
- Single category filter works correctly
- Multiple category selection works
- Filter chips appear showing active filters
- Easy filter removal functionality

### Test 1.3: Status Filtering
**Objective**: Test asset status filtering

**Test Steps**:
1. Select "Available" status
2. Apply filter and verify results
3. Select multiple statuses
4. Test "In Use" filter specifically
5. Test decommissioned asset filtering

**Expected Results**:
- Status filters work correctly
- Status badges display with proper colors
- Multiple status selection works
- Filter combinations work properly

### Test 1.4: Location Filtering
**Objective**: Test location-based filtering

**Test Steps**:
1. Select specific location from dropdown
2. Apply filter and verify assets in that location
3. Test building-level filtering
4. Test department filtering
5. Combine with other filters

**Expected Results**:
- Location filter shows correct assets
- Location hierarchy works properly
- Department filtering works
- Combined filters work correctly

### Test 1.5: Date Range Filtering
**Objective**: Test purchase date and installation date filtering

**Test Steps**:
1. Set purchase date range (e.g., last 6 months)
2. Apply filter and verify results
3. Test installation date range
4. Test warranty expiry date filtering
5. Test invalid date ranges

**Expected Results**:
- Date pickers work correctly
- Date range filtering is accurate
- Invalid ranges show appropriate messages
- Date formatting is consistent

### Test 1.6: Price Range Filtering
**Objective**: Test price-based filtering

**Test Steps**:
1. Set price range $1000-$2000
2. Apply filter and verify assets in range
3. Test minimum price only
4. Test maximum price only
5. Test invalid price ranges

**Expected Results**:
- Price filtering works accurately
- Price display formatting is correct
- Invalid ranges are handled gracefully
- Currency formatting is consistent

### Test 1.7: Advanced Search Combinations
**Objective**: Test complex filter combinations

**Test Steps**:
1. Apply text search + category + status filters
2. Add date range to existing filters
3. Add price range to multi-filter search
4. Test maximum filter combination
5. Test filter conflict scenarios

**Expected Results**:
- Complex filter combinations work correctly
- Performance remains acceptable
- Filter logic is correct (AND operations)
- No conflicts between filters

### Test 1.8: Search Results Display
**Objective**: Verify search results presentation

**Test Steps**:
1. Perform various searches
2. Check asset card display
3. Test table view toggle
4. Verify sorting functionality
5. Test pagination controls

**Expected Results**:
- Asset information displays correctly
- Card/table toggle works smoothly
- Sorting by different columns works
- Pagination functions properly
- Asset actions are accessible

---

## Phase 2: Bulk Operations Testing

### Test 2.1: Asset Selection
**Objective**: Test multi-asset selection functionality

**Test Steps**:
1. Load asset list with 20+ assets
2. Select individual assets using checkboxes
3. Test "Select All" functionality
4. Test "Select None" functionality
5. Test selection across multiple pages

**Expected Results**:
- Individual selection works correctly
- Select all/none functions properly
- Selection persists across page navigation
- Selection count displays accurately
- Bulk actions panel appears when assets selected

### Test 2.2: Bulk Status Update
**Objective**: Test bulk status change operations

**Test Steps**:
1. Select 5 assets with "Available" status
2. Choose "Change Status" from bulk actions
3. Select "In Use" as new status
4. Add reason "Testing bulk status update"
5. Confirm operation
6. Verify all assets updated correctly

**Expected Results**:
- Bulk status update processes correctly
- All selected assets change status
- Audit trail records the change
- Success message shows correct count
- Asset list refreshes with new statuses

### Test 2.3: Bulk Location Update
**Objective**: Test bulk location change operations

**Test Steps**:
1. Select 3 assets from different locations
2. Choose "Change Location" from bulk actions
3. Select new location from dropdown
4. Add reason for move
5. Confirm operation
6. Verify location updates

**Expected Results**:
- All selected assets move to new location
- Location change is recorded in audit log
- Asset movement history is updated
- Success notification appears
- Asset list shows new locations

### Test 2.4: Bulk User Assignment
**Objective**: Test bulk assignment to users

**Test Steps**:
1. Select 4 unassigned assets
2. Choose "Assign to User" from bulk actions
3. Select user from dropdown
4. Add assignment notes
5. Confirm operation
6. Verify assignments

**Expected Results**:
- All assets assigned to selected user
- Asset status changes to "In Use"
- Assignment audit trail created
- User notification (if configured)
- Asset list shows assignments

### Test 2.5: Bulk Delete Operation
**Objective**: Test bulk deletion functionality

**Test Steps**:
1. Select 2 test assets (non-critical)
2. Choose "Delete Assets" from bulk actions
3. Enter deletion reason
4. Type confirmation text
5. Confirm deletion
6. Verify assets removed

**Expected Results**:
- Confirmation dialog appears
- Deletion requires reason and confirmation
- All selected assets are deleted
- Audit log records deletion
- Asset list updates correctly

**⚠️ Warning**: Use only test data for deletion tests

### Test 2.6: Bulk Operation Error Handling
**Objective**: Test error scenarios in bulk operations

**Test Steps**:
1. Select assets with conflicting statuses
2. Attempt invalid status transitions
3. Try to assign to non-existent user
4. Test network interruption during bulk operation
5. Test permission denied scenarios

**Expected Results**:
- Error messages are clear and specific
- Partial failures are handled gracefully
- Detailed error reporting for failed items
- Successful operations aren't rolled back
- User can retry failed operations

---

## Phase 3: Export Functionality Testing

### Test 3.1: Excel Export
**Objective**: Test Excel export functionality

**Test Steps**:
1. Perform search with 20+ results
2. Click "Export" dropdown
3. Select "Excel" format
4. Click "Export Selected"
5. Verify file download
6. Open Excel file and verify data

**Expected Results**:
- Excel file downloads successfully
- File contains correct asset data
- Formatting is proper and readable
- All selected columns are included
- File name includes timestamp

### Test 3.2: CSV Export
**Objective**: Test CSV export functionality

**Test Steps**:
1. Apply multiple filters to get specific results
2. Select CSV export format
3. Customize columns in export dialog
4. Download CSV file
5. Open in text editor and Excel
6. Verify data accuracy

**Expected Results**:
- CSV file downloads correctly
- Data is properly escaped and formatted
- Column selection works
- File opens correctly in Excel
- Special characters are handled properly

### Test 3.3: Export Customization
**Objective**: Test export customization options

**Test Steps**:
1. Open export customization dialog
2. Select/deselect different columns
3. Add custom title and description
4. Test "Include QR Codes" option
5. Test "Include Images" option
6. Export with custom settings

**Expected Results**:
- Column selection works correctly
- Custom title appears in export
- Optional fields can be included/excluded
- Settings are applied correctly
- Export matches selected options

### Test 3.4: Large Dataset Export
**Objective**: Test export performance with large datasets

**Test Steps**:
1. Search for all assets (1000+ if available)
2. Attempt Excel export
3. Monitor export progress
4. Verify file size and content
5. Test CSV export of same dataset

**Expected Results**:
- Large exports complete successfully
- Progress indicator shows during export
- File size is reasonable
- No memory issues or timeouts
- Data integrity maintained

### Test 3.5: Export with Filters Applied
**Objective**: Test export respects active filters

**Test Steps**:
1. Apply multiple filters (category, status, date range)
2. Verify filtered results in UI
3. Export filtered results
4. Open export file
5. Verify only filtered data is exported

**Expected Results**:
- Export contains only filtered data
- Filter criteria documented in export
- Data matches UI display
- No additional/missing records
- Export title reflects filters applied

---

## Phase 4: Asset Comparison Testing

### Test 4.1: Basic Asset Comparison
**Objective**: Test side-by-side asset comparison

**Test Steps**:
1. Select 2-3 similar assets (same category)
2. Click "Compare Selected" button
3. Review comparison modal/page
4. Verify all asset properties displayed
5. Check difference highlighting

**Expected Results**:
- Comparison view opens correctly
- All asset properties shown side-by-side
- Differences are clearly highlighted
- Similar values are grouped properly
- Interface is user-friendly

### Test 4.2: Multi-Asset Comparison
**Objective**: Test comparison with maximum assets

**Test Steps**:
1. Select 5 assets (maximum allowed)
2. Attempt comparison
3. Try to select 6th asset (should be prevented)
4. Perform comparison with 5 assets
5. Verify readability and usability

**Expected Results**:
- Maximum of 5 assets enforced
- Clear error message for excess selection
- 5-asset comparison is readable
- Horizontal scrolling works if needed
- All differences highlighted correctly

### Test 4.3: Comparison Export
**Objective**: Test export of comparison results

**Test Steps**:
1. Compare 3 different category assets
2. Export comparison as Excel
3. Export comparison as CSV
4. Verify exported comparison data
5. Check formatting and readability

**Expected Results**:
- Comparison exports successfully
- Side-by-side format maintained in export
- Differences clearly marked
- All asset data included
- Professional formatting applied

### Test 4.4: Comparison Edge Cases
**Objective**: Test comparison with unusual data

**Test Steps**:
1. Compare assets with missing data
2. Compare assets with very long descriptions
3. Compare assets with special characters
4. Compare assets with null values
5. Test comparison performance

**Expected Results**:
- Missing data handled gracefully
- Long text doesn't break layout
- Special characters display correctly
- Null values shown appropriately
- Performance remains acceptable

---

## Phase 5: Performance & Security Testing

### Test 5.1: Search Performance
**Objective**: Test search performance with large datasets

**Test Steps**:
1. Perform search on 1000+ assets
2. Apply complex multi-filter searches
3. Test pagination performance
4. Monitor response times
5. Test concurrent user searches

**Expected Results**:
- Search completes within 3 seconds
- Pagination loads quickly (<1 second)
- Complex filters don't significantly impact performance
- Multiple users can search simultaneously
- No database timeout errors

### Test 5.2: Bulk Operation Performance
**Objective**: Test bulk operation performance

**Test Steps**:
1. Select 100+ assets for bulk operation
2. Perform bulk status update
3. Monitor operation progress
4. Test bulk location change
5. Verify system stability

**Expected Results**:
- Bulk operations complete successfully
- Progress indicator works correctly
- No system slowdown during operations
- Database remains responsive
- Audit logs created efficiently

### Test 5.3: Export Performance
**Objective**: Test export performance with large datasets

**Test Steps**:
1. Export 1000+ assets to Excel
2. Export same dataset to CSV
3. Monitor memory usage
4. Test concurrent exports
5. Verify file generation speed

**Expected Results**:
- Large exports complete within reasonable time
- Memory usage stays within limits
- Multiple concurrent exports handled
- No server timeout errors
- Generated files are complete and accurate

### Test 5.4: Security Testing
**Objective**: Test security measures and authorization

**Test Steps**:
1. Test unauthorized access to advanced search
2. Verify bulk operation permissions
3. Test export restrictions by role
4. Attempt SQL injection in search fields
5. Test XSS prevention in search terms

**Expected Results**:
- Unauthorized users cannot access features
- Role-based restrictions work correctly
- SQL injection attempts are blocked
- XSS attempts are sanitized
- Audit logs capture security events

---

## Phase 6: Browser Compatibility Testing

### Test 6.1: Cross-Browser Functionality
**Objective**: Test compatibility across different browsers

**Test Browsers**:
- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

**Test Steps** (for each browser):
1. Navigate to advanced search page
2. Test all search functionality
3. Test bulk operations
4. Test export functionality
5. Test responsive design

**Expected Results**:
- All features work consistently across browsers
- UI displays correctly in each browser
- JavaScript functionality works properly
- File downloads work correctly
- No browser-specific errors

### Test 6.2: Mobile Responsiveness
**Objective**: Test mobile device compatibility

**Test Steps**:
1. Access advanced search on mobile device
2. Test search interface usability
3. Test bulk operation accessibility
4. Test export functionality on mobile
5. Verify touch interactions

**Expected Results**:
- Interface adapts properly to mobile screens
- Touch interactions work smoothly
- Text remains readable
- Buttons are appropriately sized
- All functionality remains accessible

### Test 6.3: Accessibility Testing
**Objective**: Test accessibility compliance

**Test Steps**:
1. Use screen reader software
2. Test keyboard navigation
3. Check color contrast ratios
4. Test ARIA labels and roles
5. Verify form accessibility

**Expected Results**:
- Screen readers can navigate interface
- All functionality accessible via keyboard
- Color contrast meets WCAG guidelines
- ARIA labels provide proper context
- Forms are properly labeled

---

## Test Data Templates

### Sample Asset Test Data
```csv
AssetTag,Category,Brand,Model,Status,Location,Department,PurchasePrice,WarrantyExpiry
DSK-0001001,Desktop,Dell,OptiPlex 7090,Available,IT-Floor1-Room101,IT,1299.99,2025-12-31
LAP-0001002,Laptop,Lenovo,ThinkPad X1,InUse,HR-Floor2-Room205,HR,1899.99,2024-06-30
PRN-0001003,Printer,HP,LaserJet Pro,UnderMaintenance,IT-Floor1-Room105,IT,799.99,2025-03-15
NET-0001004,Network,Cisco,Switch 2960,Available,IT-Floor1-ServerRoom,IT,2499.99,2026-01-20
SRV-0001005,Server,Dell,PowerEdge R740,InUse,IT-Floor1-DataCenter,IT,8999.99,2025-08-10
```

### Test User Accounts
```sql
-- Ensure these test users exist with appropriate roles
-- Admin: admin@hospital.com
-- IT Support: itsupport@hospital.com  
-- Asset Manager: assetmgr@hospital.com
-- Department Head: depthead@hospital.com
-- Regular User: user@hospital.com
```

---

## Test Results Template

### Test Execution Tracking
| Test ID | Test Name | Status | Issues Found | Notes |
|---------|-----------|--------|--------------|-------|
| 1.1 | Basic Search | ✅ PASS | None | Search works correctly |
| 1.2 | Category Filtering | ❌ FAIL | Filter reset issue | See bug #001 |
| 1.3 | Status Filtering | ⚠️ PARTIAL | Minor UI issue | Filter chips styling |
| ... | ... | ... | ... | ... |

### Issue Tracking Template
| Bug ID | Priority | Description | Steps to Reproduce | Expected | Actual | Status |
|--------|----------|-------------|-------------------|----------|--------|---------|
| #001 | High | Category filter resets | 1. Select category 2. Apply other filter | Filter persists | Filter clears | Open |
| #002 | Medium | Export button disabled | 1. Search assets 2. Click export | Button enabled | Button disabled | Fixed |

---

## Acceptance Criteria

### Must Pass Criteria
- ✅ All basic search functionality works correctly
- ✅ Bulk operations complete successfully without data loss
- ✅ Export functions generate accurate files
- ✅ Asset comparison displays correct information
- ✅ Security permissions are enforced
- ✅ Performance meets acceptable standards (< 3s search, < 30s bulk ops)

### Should Pass Criteria
- ✅ Cross-browser compatibility (Chrome, Firefox, Edge, Safari)
- ✅ Mobile responsiveness works properly
- ✅ Accessibility guidelines followed
- ✅ Error handling provides clear feedback
- ✅ UI/UX is intuitive and professional

### Nice to Have
- ✅ Advanced keyboard shortcuts
- ✅ Export progress indicators
- ✅ Search result caching
- ✅ Bulk operation undo functionality

---

## Post-Testing Actions

### Bug Fixes Priority
1. **Critical**: Data loss, security vulnerabilities, system crashes
2. **High**: Core functionality broken, major usability issues
3. **Medium**: Minor functionality issues, UI inconsistencies
4. **Low**: Cosmetic issues, enhancement requests

### Documentation Updates
- Update user manual with new features
- Create quick reference guide for bulk operations
- Document export formats and limitations
- Update training materials

### Deployment Checklist
- [ ] All critical and high priority bugs fixed
- [ ] Performance benchmarks met
- [ ] Security review completed
- [ ] Database migration scripts tested
- [ ] Backup procedures verified
- [ ] User training materials prepared
- [ ] Support team briefed on new features

---

## Conclusion

This comprehensive testing protocol ensures that all advanced asset search and management features are thoroughly validated before deployment. The structured approach covers functionality, performance, security, and usability across different scenarios and user types.

**Remember**: Always test on non-production data and have proper backups before testing potentially destructive operations like bulk delete.
