using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class DischargeReportViewModel
    {
        public int DischargeReportId { get; set; }

        public int PatientFileId { get; set; }

        // Patient Information
        [Display(Name = "Patient Name")]
        public string PatientName { get; set; }

        [Display(Name = "Patient Surname")]
        public string PatientSurname { get; set; }

        // Doctor Information
        [Display(Name = "Doctor Name")]
        public string DoctorName { get; set; }

        [Display(Name = "Doctor Surname")]
        public string DoctorSurname { get; set; }

        [Display(Name = "Doctor Title")]
        public string DoctorTitle { get; set; }

        public int DoctorUserId { get; set; }

        // Discharge Details
        [Display(Name = "Discharge Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DischargeDate { get; set; }

        [Display(Name = "Length of Stay (Days)")]
        public int LengthOfStay { get; set; }

        // Medical Information
        [Display(Name = "Discharge Summary")]
        public string DischargeSummary { get; set; }

        [Display(Name = "Treatment Provided")]
        public string TreatmentProvided { get; set; }

        [Display(Name = "Medication Prescribed")]
        public string MedicationPrescribed { get; set; }

        [Display(Name = "Vital Signs at Discharge")]
        public string VitalSignsAtDischarge { get; set; }

        // Billing Information
        [Display(Name = "Doctor Fee")]
        [DisplayFormat(DataFormatString = "R {0:N2}")]
        public decimal DoctorFee { get; set; }

        [Display(Name = "Medication Fee")]
        [DisplayFormat(DataFormatString = "R {0:N2}")]
        public decimal MedicationFee { get; set; }

        [Display(Name = "Room Fee")]
        [DisplayFormat(DataFormatString = "R {0:N2}")]
        public decimal RoomFee { get; set; }

        [Display(Name = "Other Costs")]
        [DisplayFormat(DataFormatString = "R {0:N2}")]
        public decimal OtherCosts { get; set; }

        [Display(Name = "Total Bill")]
        [DisplayFormat(DataFormatString = "R {0:N2}")]
        public decimal TotalBill { get; set; }

        // Additional Properties for Hospital Information
        public string HospitalName { get; set; } = "CareConnect";
        public string HospitalAddress { get; set; } = "123 Healthcare Street";
        public string HospitalCity { get; set; } = "Port Elizabeth";
        public string HospitalPhone { get; set; } = "(011) 456-7890";

        // Helper Properties
        public string FullPatientName => $"{PatientName} {PatientSurname}";
        public string FullDoctorName => $"{DoctorTitle} {DoctorName} {DoctorSurname}";

        // You might want to add a property for storing the hospital logo
        public byte[] HospitalLogo { get; set; }

        // Additional Properties for Report Generation
        public string ReportNumber => $"DR{DischargeReportId:D6}";
        public string FormattedDischargeDate => DischargeDate.ToString("dd MMMM yyyy");
    }
}
