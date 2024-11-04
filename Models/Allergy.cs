using System.ComponentModel.DataAnnotations;

namespace CareConnect.Models
{
    public class Allergy
    {
        [Key]
        public int AllergyId { get; set; }
        [Required,MaxLength(50)]
        public string Name { get; set; }
        [Required,MaxLength(50)]
        public string Severity { get; set; }
        public bool IsDeleted { get; set; }
    }
}
