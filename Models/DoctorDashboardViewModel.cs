namespace Connect.Models
{
    public class DoctorDashboardViewModel
    {
     
            public int PatientsToSeeToday { get; set; }
            public int PendingTasks { get; set; }
            public int SucessTasks { get; set; }
            public List<ConditionTrendViewModel> ConditionTrends { get; set; }
            public int PrescriptionsToday { get; set; }
            public int AppointmentsToday { get; set; }
            public List<AppointmentViewModel> UpcomingAppointments { get; set; }
            public string Status { get; set; }
            public List<PendingTaskViewModel> PendingInformation { get; set; }
            public int TotalPatients { get; set; }
    }

     public class ConditionTrendViewModel
     {
            public string ConditionDescription { get; set; }
            public int ConditionCount { get; set; }
     }


    
}
