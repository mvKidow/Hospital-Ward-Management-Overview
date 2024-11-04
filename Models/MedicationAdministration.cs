using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect.Models
{
    public class MedicationAdministration
    {
        [Key]
        public int  MedicationAdminId { get; set; }

        [ForeignKey("MedicationId")]
        public int MedicationId {  get; set; }

        [ForeignKey("PatientFileId")]
        public int PatientFileId { get; set;}

        public string MedicationType {  get; set; }
    }
}
