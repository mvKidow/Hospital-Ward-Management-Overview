using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class Instrustion
    {
        [Key]
        public int InstructionId { get; set; }
        public string Details { get; set; }
        public DateTime Date { get; set; }

        [ForeignKey("PatientFileId")]
        public int PatientFileId { get; set; }

        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
