using Microsoft.AspNetCore.Mvc.Rendering;

namespace Connect.Models
{
    public class PatientFileDoctorAssignmentDetailsViewModel
    {
        public int PatientFileId { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public string ConditionName { get; set; }
        public string ConditionDescription { get; set; }
        public byte[] ProfilePhoto { get; set; }
        public int? AssignedDoctorId { get; set; }
        public List<SelectListItem> AvailableDoctors { get; set; }
        public int WardId { get; set; }
    }
}
