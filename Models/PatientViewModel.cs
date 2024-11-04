using CareConnect.Models;

namespace Connect.Models
{
    public class PatientViewModel
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public string Name { get; set; } 
        public string Surname { get; set; } 
        public string Condition { get; set; }
        public string Medication { get; set; }
        public string Allergies { get; set; }
   
    }
}
