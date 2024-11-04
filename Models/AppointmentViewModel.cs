using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace Connect.Models
{
    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; } 
        public string Name { get; set; }
        public DateTime Date {  get; set; }
        public DateTime Time { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public int PatientFileId { get; set; }
        public int UserId { get; set; }
    }
}
