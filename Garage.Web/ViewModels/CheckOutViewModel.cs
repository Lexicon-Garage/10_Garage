namespace Garage.Web.ViewModels
{
    public class CheckOutViewModel
    {
        public int SessionId { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string VehicleTypeName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public DateTime CheckInTime { get; set; }
        public List<string> SpotNumbers { get; set; } = new();

        public TimeSpan ParkedDuration => DateTime.Now - CheckInTime;
    }
}