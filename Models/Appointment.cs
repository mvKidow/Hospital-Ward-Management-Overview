using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Permissions;

namespace CareConnect.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }
        public DateTime Date { get; set; }
        public string Purpose { get; set; }

        [ForeignKey("PatientFileId")]
        public int PatientFileId { get; set; }

        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public string Status { get; set; }
    }
}