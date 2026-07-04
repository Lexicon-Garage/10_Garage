using Garage.Web.Models;

namespace Garage.Web.ViewModels
{
    public class ReceiptViewModel
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; } 

        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }

        public TimeSpan ParkingDuration { get; set; }

        public decimal Price { get; set; }
    }
}
