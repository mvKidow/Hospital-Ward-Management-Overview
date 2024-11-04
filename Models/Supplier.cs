using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }
        [Required,MaxLength(50)]
        public string SupplierName { get; set;}
        [Required,EmailAddress, MaxLength(50)]
        public string Email{ get; set; }
        [Required, MaxLength(50)]
        public string SupplierType { get; set; }
        public bool IsDeleted { get; set; }
    }
}
