using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }
        public int RoomNumber { get; set; }
        public int Capacity { get; set; }
        public string AvailabilityStatus { get; set; }

        [ForeignKey("WardId")]
        public int WardId { get; set; }
       
    }
}