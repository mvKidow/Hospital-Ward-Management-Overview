namespace Connect.Models
{
    public class PatientListViewModelDoctor
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PatientType { get; set; }
        public string ConditionName { get; set; }
        public string MedicationName { get; set; }
        public string AllergyName { get; set; }
    }
}
