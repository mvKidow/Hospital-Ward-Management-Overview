namespace Connect.Models
{
    public class RoomViewModel
    {
        public int RoomId { get; set; }
        public int RoomNumber { get; set; }
        public int Capacity { get; set; }
        public string AvailabilityStatus { get; set; }
        public int WardId { get; set; }
        public string WardName { get; set; }
        public int OccupiedBeds { get; set; }
        public int TotalBeds { get; set; }
    }
}
