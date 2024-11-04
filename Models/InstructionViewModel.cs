using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect.Models
{
    public class InstructionViewModel
    {
       
            public string Name { get; set; }
            public byte[]? ProfilePhoto { get; set; }
            public int InstructionId { get; set; }
         
            [Required]
            [DataType(DataType.Date)]
            [Range(typeof(DateTime), "2024-01-01", "2024-12-31", ErrorMessage = "Date must be Current.")]
             public DateTime Date { get; set; }
            [Required]
             public string Details { get; set; }
             [Required]
             public string Importance { get; set; }
             [Required]
            public string DoctorName { get; set; }
            public int UserId { get; set; }
            public int PatientFileId { get; set; }


    }
}
