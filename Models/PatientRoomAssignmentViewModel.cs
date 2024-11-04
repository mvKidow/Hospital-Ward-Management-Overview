using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class PatientRoomAssignmentViewModel
    {
        [Required]
        public string PatientName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AdmissionDate { get; set; }

        [Required]
        public int WardNo { get; set; }

        [Required]
        public int RoomNo { get; set; }

        [Required]
        public int BedNo { get; set; }
    }
}
