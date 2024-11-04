using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class Patient:User
    {
       
        public string PatientType { get; set; }
        [ForeignKey("Condition")]
        public int ConditionId { get; set; }
        [ForeignKey("Medication")]
        public int? MedicationId { get; set; }
        [ForeignKey("Allergy")]
        public int? AllergyId { get; set; }

    }
}