namespace Connect.Models
{
    public class PatientReportViewModel
    {
        public IEnumerable<DoctorPatientViewModel> Patients { get; set; }
        public IEnumerable<PrescriptionViewModel> Prescriptions { get; set; }
            public int PrescriptionId { get; set; }
            public int UserId { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string ProfilePhoto { get; set; }
            public string Medication { get; set; }
            public string Dosage { get; set; }
            public DateTime Date { get; set; }
            public string ConditionName { get; set; }
            public string CurrentMedication { get; set; }
            public string AllergyName { get; set; }
            public int PatientFileId { get; set; }
        
    }
}
