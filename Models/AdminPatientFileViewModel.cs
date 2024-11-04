namespace Connect.Models
{
    public class AdminPatientFileViewModel
    {
        public int? PatientFileId { get; set; } 
        public DateTime? AdmissionDate { get; set; }
        public DateTime? DischargeDate { get; set; }
        public int? UserId { get; set; } 
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Title { get; set; }
        public string PatientType { get; set; }
        public int? ConditionId { get; set; }
        public int? MedicationId { get; set; }
        public int? AllergyId { get; set; }
    }
}
