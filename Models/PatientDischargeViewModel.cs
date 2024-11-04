using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class PatientDischargeViewModel
    {
        public int PatientFileId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime AdmissionDate { get; set; }
        public int BedId { get; set; }
        public string Status { get; set; }
        public string PatientType { get; set; }
        public string ConditionName { get; set; }
        public string MedicationName { get; set; }
        public string AllergyName { get; set; }
        public string WardName { get; set; }
        public string BedNumber { get; set; }
        public int RoomNumber { get; set; }

        // Changed from required to nullable
        public string? BedStatus { get; set; }
        public int LengthOfStay { get; set; }

        [Required(ErrorMessage = "Discharge date is required")]
        public DateTime? DischargeDate { get; set; }

        [Required(ErrorMessage = "Doctor fee is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Doctor fee must be greater than 0")]
        public decimal DoctorFee { get; set; }

        [Required(ErrorMessage = "Medication fee is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Medication fee must be greater than 0")]
        public decimal MedicationFee { get; set; }

        [Required(ErrorMessage = "Room per day fee is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Room per day fee must be greater than 0")]
        public decimal RoomPerDay { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Other costs must be greater than or equal to 0")]
        public decimal OtherCosts { get; set; }

        [Required(ErrorMessage = "Total amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
        public decimal Total { get; set; }

        [Required(ErrorMessage = "Discharge summary is required")]
        [StringLength(500, ErrorMessage = "Discharge summary cannot exceed 500 characters")]
        public string DischargeSummary { get; set; }

        [Required(ErrorMessage = "Treatment details are required")]
        [StringLength(500, ErrorMessage = "Treatment details cannot exceed 500 characters")]
        public string TreatmentProvided { get; set; }

        [Required(ErrorMessage = "Vital signs are required")]
        [StringLength(500, ErrorMessage = "Vital signs cannot exceed 500 characters")]
        public string VitalSignsAtDischarge { get; set; }


    
    }
}
