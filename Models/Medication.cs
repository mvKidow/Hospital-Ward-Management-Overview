using System.ComponentModel.DataAnnotations;

namespace CareConnect.Models
{
    public class Medication
    {
        [Key]
         public int MedicationId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }     
    }
}