using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class Bed
    {
        [Key]
     
        public int BedId { get; set; }
        [Required]
        public string Status { get; set; }

        [ForeignKey("RoomId")]
        public int RoomId { get; set; }
    }
}
