using System.ComponentModel.DataAnnotations;

namespace Garage.Web.ViewModels
{
    public class ParkingDetailsViewModel
    {
        public int SessionId { get; set; }
        public int VehicleId { get; set; }

        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Vehicle Type")]
        public string VehicleTypeName { get; set; } = string.Empty;

        [Display(Name = "Brand")]
        public string BrandName { get; set; } = string.Empty;

        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;

        [Display(Name = "Color")]
        public string Color { get; set; } = string.Empty;

        [Display(Name = "Number of wheels")]
        public int NumberOfWheels { get; set; }

        [Display(Name = "Checked in")]
        public DateTime CheckInTime { get; set; }

        [Display(Name = "Checked out")]
        public DateTime? CheckOutTime { get; set; }

        [Display(Name = "Parking spots")]
        public List<string> SpotNumbers { get; set; } = new();

        public bool IsActive => CheckOutTime == null;

        [Display(Name = "Parked for")]
        public TimeSpan ParkedDuration => (CheckOutTime ?? DateTime.Now) - CheckInTime;

        public string ParkedDurationDisplay =>
            ParkedDuration.Days > 0
                ? $"{ParkedDuration.Days}d {ParkedDuration.Hours}h {ParkedDuration.Minutes}m"
                : $"{ParkedDuration.Hours}h {ParkedDuration.Minutes}m";
    }
}