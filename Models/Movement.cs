using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class Movement
    {
        [Key]
        public int MovementId { get; set; }
        [Required, MaxLength(100)]
        public string FromLocation { get; set; }
        [Required,MaxLength(100)]
        public string ToLocation { get; set; }
        public DateTime Date { get; set; }

        [ForeignKey("PatientFileId")]
        public int PatientFileId { get; set; }
    }
}
