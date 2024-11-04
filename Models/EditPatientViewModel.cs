namespace Connect.Models
{
    public class EditPatientViewModel
    {
        public int PatientFileId { get; set; } 
        public string Name { get; set; } 
        public string Surname { get; set; }
        public string PatientType {  get; set; }
        public int ConditionId { get; set; } 
        public byte[] ProfilePhoto { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; } 

      
        public IEnumerable<ConditionViewModel> Conditions { get; set; }
    }
}
