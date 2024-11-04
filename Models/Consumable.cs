using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class Consumable
    {
        [Key]
        public int ConsumableId { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; }
        [Required, MaxLength(100)]
        public string Type { get; set; }
        public int QuantityAvailable { get; set; }

        [ForeignKey("SupplierId")]
        public int SupplierId { get; set; }
        public bool IsDeleted { get; set; }
    }
}