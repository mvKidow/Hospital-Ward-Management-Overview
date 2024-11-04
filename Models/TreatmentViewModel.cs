namespace Connect.Models
{
    public class TreatmentViewModel
    {
        public int PatientId { get; set; }
        public bool ChangeIVDrip { get; set; }
        public bool ChangeCatheter { get; set; }
        public bool DressWound { get; set; }
    }
}
