using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class Nurse : User
    {
       
        [Required]
        public string Department { get; set; }
    }
}