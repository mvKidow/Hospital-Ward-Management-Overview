namespace Connect.Models
{
    public class DoctorPatientViewModel
    {

        public int PatientFileId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public byte[]? ProfilePhoto { get; set; }
        public string ConditionName { get; set; }
        public string MedicationName { get; set; } 
        public string AllergyName { get; set; }
    }
}
