using CareConnect.Models;

namespace Connect.Models
{
    public class PatientFileViewModel
    {
        public int PatientFileId { get; set; }
        public int BedId { get; set; }
        public int RoomId { get; set; }
        public string PatientType { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string ToLocation { get; set; }
        public int ConditionId { get; set; }
    }
}
