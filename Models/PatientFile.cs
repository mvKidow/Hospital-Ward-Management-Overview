using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class PatientFile
    {
        [Key]
        [Required]
        public int PatientFileId { get; set; }

        [Required]
        public DateTime AdmissionDate { get; set; }

        public DateTime? DischargeDate { get; set; }

        [ForeignKey("Patient")]
        public int UserId { get; set; }
        public Patient Patient { get; set; }

        [ForeignKey("Bed")]
        public int BedId { get; set; }
        public Bed Bed { get; set; }

        public bool IsDeleted { get; set; }



    }
}
