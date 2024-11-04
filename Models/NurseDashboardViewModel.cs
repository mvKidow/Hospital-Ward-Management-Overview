namespace Connect.Models
{
    public class NurseDashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int PendingVitals { get; set; }
        public int MedicationsDue { get; set; }
        public int DoctorRequests { get; set; }
        public List<PatientDistributionData> PatientDistribution { get; set; }
        public List<UpcomingTask> UpcomingTasks { get; set; }
    }
}
