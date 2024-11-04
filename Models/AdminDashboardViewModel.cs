using CareConnect.Models;

namespace Connect.Models
{
    public class AdminDashboardViewModel
    {
        public int AvailableDoctors { get; set; }
        public int AvailableBeds { get; set; }
        public int AvailablePatients { get; set; }
        public int? AvailableNurses { get; set; }
        public int TotalConsumables { get; set; } 
    }
}
