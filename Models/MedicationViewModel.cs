namespace Connect.Models
{
    public class MedicationViewModel
    {
        public int MedicationId { get; set; }
        public string MedicationName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
