namespace Garage.Web.ViewModels.Admin
{
    public class ActiveParkingViewModel
    {
        public int ParkingSessionId { get; set; }

        public string OwnerName { get; set; } = string.Empty;

        public string VehicleTypeName { get; set; } = string.Empty;

        public string RegistrationNumber { get; set; } = string.Empty;

        public string SpotNumber { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public DateTime CheckInTime { get; set; }

        public TimeSpan ParkingTime => DateTime.Now - CheckInTime;
    }
}
