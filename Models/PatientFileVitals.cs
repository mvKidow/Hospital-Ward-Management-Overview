using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class PatientFileVitals
    {
        [Key]
        public int PatientFileVitalsId { get; set; }

        [ForeignKey("VitalsId")]
        public int VitalsId { get; set; }
        public Vitals Vitals { get; set; }

        [ForeignKey("PatientFileId")]
        public int PatientFileId { get; set; }
        public PatientFile PatientFile { get; set; }
    }
}

