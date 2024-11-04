namespace Connect.Models
{
    public class BedViewModel
    {

        public int BedId { get; set; }
        public bool IsOccupied { get; set; } // Change Status to IsOccupied
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } // Ensure you have this property if needed
        public string WardName { get; set; } // Ensure you have this property if needed
        public AdminPatientFileViewModel CurrentPatientFile { get; set; }
    }
}
