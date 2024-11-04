namespace Connect.Models
{
    public class TreatPatientViewModel
    {
        public int PatientFileId { get; set; }
        public string TreatmentType { get; set; } 
        public string Notes { get; set; }
        public DateTime TreatedAt { get; set; } = DateTime.Now;
    }
}
