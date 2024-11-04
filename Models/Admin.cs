using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class Admin : User
    {
        [Required]
        public string AdminSpecificField { get; set; }


    }
}
