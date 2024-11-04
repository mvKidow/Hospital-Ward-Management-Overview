using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class StockRequest
    {
        [Key]
        public int StockRequestId { get; set; }
        public DateTime RequestDate { get; set; }
        public int Quantity { get; set; }


        [ForeignKey("ConsumableId")]
        public int consumableId { get; set; }

        [ForeignKey("WardId")]
        public int WardId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
