using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class PatientAdmissionViewModel
    {
        public int UserId { get; set; }

        [DisplayName("Patient Name")]
        public string PatientName { get; set; }

        [DisplayName("Patient Surname")]
        public string PatientSurname { get; set; }

        [Required(ErrorMessage = "Patient Type is required")]
        [DisplayName("Patient Type")]
        public string PatientType { get; set; }

        [Required(ErrorMessage = "Condition is required")]
        [DisplayName("Condition")]
        public int ConditionId { get; set; }

        [DisplayName("Medication")]
        public int? MedicationId { get; set; }

        [DisplayName("Allergy")]
        public int? AllergyId { get; set; }
    }
}
