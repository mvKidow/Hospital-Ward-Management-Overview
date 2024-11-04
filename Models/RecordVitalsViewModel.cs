using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;

namespace Connect.Models
{
    public class RecordVitalsViewModel
    {

        public int? PatientFileId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
    
        [Required(ErrorMessage = "Blood pressure is required.")]
        public decimal BloodPressure { get; set; }

        [Required(ErrorMessage = "Temperature is required.")]
        public decimal Temperature { get; set; }

        [Required(ErrorMessage = "Sugar level is required.")]
        public decimal SugarLevel { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }
}
