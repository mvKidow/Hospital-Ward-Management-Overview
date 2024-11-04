using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class AssignRoomAndBedRequest
    {
        public int UserId { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public int WardId { get; set; }
        public string WardName { get; set; }
        public DateTime AdmissionDate { get; set; }
        public int RoomNumber { get; set; }

        [Required(ErrorMessage = "Please select a bed")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid bed")]
        public int BedId { get; set; }

        public class RequiredIfRoomSelectedAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var model = (AssignRoomAndBedRequest)validationContext.ObjectInstance;

                if (model.RoomNumber > 0 && (value == null || (int)value == 0))
                {
                    return new ValidationResult(ErrorMessage ?? "Bed must be selected when a room is chosen");
                }

                return ValidationResult.Success;
            }
        }
    }
}
