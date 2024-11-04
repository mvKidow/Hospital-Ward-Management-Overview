using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect.Models
{
    public class Visit
    {
        public int VisitId { get; set; }
        [Required]
        public DateTime VisitDate { get; set; }
        public DateTime VisitTime { get; set;}
        [Required,MaxLength(50)]
        public string VisitingPurpose {  get; set; }
        [ForeignKey("UserId")]
        public int UserId {  get; set; }
       
    }
}
