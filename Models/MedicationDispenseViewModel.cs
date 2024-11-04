namespace Connect.Models
{
    public class MedicationDispenseViewModel
    {
        public int PatientFileId { get; set; }
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public DateTime AdministeredAt { get; set; } = DateTime.Now;
    }
}
