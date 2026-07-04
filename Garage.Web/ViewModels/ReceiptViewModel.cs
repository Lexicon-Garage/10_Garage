using Garage.Web.Models;

namespace Garage.Web.ViewModels
{
    public class ReceiptViewModel
    {
        private const decimal HourlyRate = 5m;
        public string RegistrationNumber { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }

        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }

        public TimeSpan ParkingDuration  => CheckOutTime - CheckInTime;

        public decimal Price => (int)Math.Ceiling(ParkingDuration.TotalHours) * HourlyRate;
    }
}
