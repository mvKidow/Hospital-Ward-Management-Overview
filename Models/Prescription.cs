using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }
        public DateTime Date { get; set; }
        [Required, MaxLength(50)]
        public string Dosage { get; set; }

        [ForeignKey("PatientFileId")]
        public int PatientFileId { get; set; }

        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public string MedicationPrescribed { get; set; }
        public bool IsDelete { get; set; }
     


    }
}
