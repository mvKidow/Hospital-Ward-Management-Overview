using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class CareConnectInfor
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string WorkingHours { get; set; }
        public string AboutTitle { get; set; }
        public string AboutDescription { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceDescription { get; set; }

    }
}
