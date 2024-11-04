using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect.Models
{
    public class PatientTreatmen
    {
        [Key]
        public int  PatientTreatmentId { get; set; }
        [ForeignKey("TreatmentId")]
        public int TreatmentId { get; set; }

        [ForeignKey("PatientFileId")]
        public int PatientFileId { get; set;}

        [Required,MaxLength(100)]
        public string TreatmentName { get; set; }
   
    }
}
