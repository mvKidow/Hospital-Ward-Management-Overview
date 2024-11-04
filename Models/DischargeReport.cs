using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CareConnect.Models
{
    public class DischargeReport
    {
        [Key]
        public int DischargeReportId { get; set; }
        [ForeignKey("PatientFile")]
        public int PatientFileId { get; set; }
        public PatientFile PatientFile { get; set; }
        public DateTime DischargeDate { get; set; }
        public decimal TotalBill { get; set; }

        [Required]
        public string DischargeSummary { get; set; }
        [Required]
        public string TreatmentProvided { get; set; }
        [Required]
        public string MedicationPrescribed { get; set; }

        // New fields
        public decimal DoctorFee { get; set; }
        public decimal MedicationFee { get; set; }
        public decimal RoomFee { get; set; }
        public decimal OtherCosts { get; set; }
        public int LengthOfStay { get; set; }
        public string VitalSignsAtDischarge { get; set; }
        public List<string> Prescriptions { get; set; }
    }
}
