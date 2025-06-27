using System;

namespace HospitalAssetTracker.Models
{
    public class MaintenanceAssetViewModel
    {
        public required Asset Asset { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public int DaysOverdue { get; set; }
        public bool IsOverdue { get; set; }
        public bool NeverMaintained { get; set; }
        public bool MaintenanceInProgress { get; set; }
    }
}
