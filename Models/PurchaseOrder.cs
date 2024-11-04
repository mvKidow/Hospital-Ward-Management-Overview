using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int PurchaseOrderId { get; set; }
        [Required]
        public int PurchaseOrderStatus { get; set; }
        [Required]
        public int PurchaseOrderNumber { get; set; }

        [ForeignKey("SupplierId")]
        public int SupplierId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
