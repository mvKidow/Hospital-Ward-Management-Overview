using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class PrescriptionViewModel
    {
        //public string Name { get; set; }
        //public string Medication { get; set; }
        //public DateTime Date { get; set; }
        //[Required, MaxLength(50)]
        //public string Dosage { get; set; }
        //public int PatientFileId { get; set; }
            public int PrescriptionId { get; set; }

            [Required]
            public int PatientFileId { get; set; }

            public string Name { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime Date { get; set; }

            [Required]
            [MaxLength(50)]
            public string Dosage { get; set; }
            public byte[]? ProfilePhoto { get; set; }

           [Required]
            public string Medication { get; set; }
            public int UserId { get; set; }
        
    }
}
