using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class Treatment
    {
        [Key]
        public int TreatmentId { get; set; }
        public DateTime Date { get; set; }
        public string Dosage { get; set; }
        [ForeignKey("PatientFileId")]
        public int PatientFileId { get; set; }
        [ForeignKey("MedicationId")]
        public int MedicationId { get; set; }

        [ForeignKey("UserIdId")]
        public int UserId {  get; set; }
        public bool IsDeleted { get; set; }
    }
}
