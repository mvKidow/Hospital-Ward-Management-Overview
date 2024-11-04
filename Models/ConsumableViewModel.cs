using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class ConsumableViewModel
    {
        public int ConsumableId { get; set; }
        [Required, MaxLength(50)]
        public string ConsumableName { get; set; }
        [Required, MaxLength(100)]
        public string ConsumableType { get; set; }
        public int QuantityAvailable { get; set; }
        [Required]
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
    }
}
