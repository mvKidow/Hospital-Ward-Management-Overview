using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }
        public string PatientName { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public string Status { get; set; }
        [ForeignKey("PatientFileId")]
        public int PatientFileId { get; set; }
        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public string LastName { get; set; }
        public bool IsDeleted { get; set; }
        public string  Reason { get; set; }

    }
}

