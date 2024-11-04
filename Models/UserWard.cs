using CareConnect.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect.Models
{
    public class UserWard
    {
        [Key]
        public int UserWardId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int WardId { get; set; }
        public Ward Ward { get; set; }
        public bool IsDeleted { get; set; }
    }
}
