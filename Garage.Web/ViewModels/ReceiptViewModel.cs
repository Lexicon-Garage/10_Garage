using Garage.Web.Models;

namespace Garage.Web.ViewModels
{
    public class ReceiptViewModel
    {

        //ToDo: ReceiptViewModel har hårdkodat timpris (5 kr)
        private const decimal HourlyRate = 5m;
        public string RegistrationNumber { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }

        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        private TimeSpan ParkingDuration  => CheckOutTime - CheckInTime;
        public string ParkingDurationFormatted => ParkingDuration.ToString(@"hh\:mm\:ss");
        public decimal Price => Math.Ceiling((decimal)ParkingDuration.TotalHours) * HourlyRate;
    }
}
