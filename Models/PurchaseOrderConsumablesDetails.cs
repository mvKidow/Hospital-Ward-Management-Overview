using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect.Models
{
    public class PurchaseOrderConsumablesDetails
    {
        [Key]
        public int PurchaseOrderConsumablesDetailsId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public double PurchaseAmount { get; set; }
        [ForeignKey("PurchaseOrderId")]
        public int PurchaseOrderId { get; set; }

        [ForeignKey("ConsumablesId")]
        public int ConsumablesId { get; set; }
    }
}
