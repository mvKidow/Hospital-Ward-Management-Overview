namespace Connect.Models
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string FullName => $"{Name} {Surname}";
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
    }
}
