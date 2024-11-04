namespace Connect.Models
{
    public class ViewModel
    {
        public class ReportViewModel
        {
            public string UserId { get; set; }
            public string ProfilePhoto { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string AllergyName { get; set; }
            public string ConditionName { get; set; }
            public string CurrentMedication { get; set; }
            public string PatientId { get; set; }

            public ICollection<PrescriptionHistoryItem> Prescriptions { get; set; }
            public ICollection<MedicalHistoryItem> MedicalHistory { get; set; }
        }

        public class PrescriptionHistoryItem
        {
            public DateTime Date { get; set; }
            public string Medication { get; set; }
            public string Dosage { get; set; }
            public string DoctorName { get; set; }
            public bool IsActive { get; set; }
        }

        public class MedicalHistoryItem
        {
            public DateTime Date { get; set; }
            public string EventType { get; set; }
            public string Description { get; set; }
            public string RecordedBy { get; set; }
        }
    }
}
