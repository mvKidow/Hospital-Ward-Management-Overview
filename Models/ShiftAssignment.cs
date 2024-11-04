using CareConnect.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class ShiftAssignment
    {

        [Key]
        public int ShiftAssignmentId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Shift { get; set; }

        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public bool IsDeleted { get; set; }

    }
}
