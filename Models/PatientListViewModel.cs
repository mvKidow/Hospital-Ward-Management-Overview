namespace Connect.Models
{
    public class PatientListViewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PatientType { get; set; }
        public string ConditionDescription { get; set; }
        public byte[]? ProfilePhoto { get; set; } 
    }
}
