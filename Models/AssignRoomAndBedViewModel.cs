namespace Connect.Models
{
    public class AssignRoomAndBedViewModel
    {
        public AssignRoomAndBedRequest Request { get; set; }
        public List<RoomViewModel> Rooms { get; set; }
        public List<BedViewModel> Beds { get; set; }

        public AssignRoomAndBedViewModel()
        {
            Request = new AssignRoomAndBedRequest();
            Rooms = new List<RoomViewModel>();
            Beds = new List<BedViewModel>();
        }
    }
}
