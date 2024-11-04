using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class ContactUs
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Phone]
        public string PhoneNumber {  get; set; }
        public string Message {  get; set; }    

    }
}
