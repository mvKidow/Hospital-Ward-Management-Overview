using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class DoctorVisit
    {
        [Key]
        public int DoctorVisitId { get; set; }
        public DateTime? Date { get; set; }
        [Required]
        public string Instructions { get; set; }

        [ForeignKey("PatientFileId")]
        public int PatientFileId { get; set; }
        [ForeignKey("UserId")]
        public int UserId { get; set; }
      

    }
}
