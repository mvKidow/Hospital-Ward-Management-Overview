using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class Doctor : User
    {
       
        [Required, MaxLength(50)]
        public string Specialization { get; set; }
    }
}
