using System.ComponentModel.DataAnnotations;

namespace CareConnect.Models
{
    public class Condidtion
    {
        [Key]
        public int ConditionId { get; set; }

        [Required(ErrorMessage = "Condition name is required.")]
        [StringLength(100, ErrorMessage = "Condition name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
    }
}
