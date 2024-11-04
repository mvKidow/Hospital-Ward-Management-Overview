using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect.Models
{
    public class MedsPerscription
    {
        [Key]
        public int MedsPerscriptionId { get; set; }
        [Required]
        public string MedsPerscriptionName { get; set; }
        [ForeignKey("MedicationId")]
        public int MedicationId { get; set; }
        [ForeignKey("PersriptionId")]
        public int PerscriptionId { get; set; }

    }
}
