using Microsoft.AspNetCore.Mvc.Rendering;

namespace Connect.Models
{
    public class AssignDoctorViewModel
    {
        public PatientFileDoctorAssignmentDetailsViewModel PatientFile { get; set; }
        public List<SelectListItem> AvailableDoctors { get; set; }
    }
}
