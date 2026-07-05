using Garage.Web.Models;

namespace Garage.Web.ViewModels
{
    public class ParkedVehicleOverviewViewModel
    {
        public int Id { get; set; }
        public VehicleType VehicleType { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public DateTime ArrivedTime { get; set; }
        public TimeSpan ParkedDuration => DateTime.Now - ArrivedTime;

        public string ParkedDurationDisplay =>
            ParkedDuration.Days > 0
                ? $"{ParkedDuration.Days}d {ParkedDuration.Hours}h {ParkedDuration.Minutes}m"
                : $"{ParkedDuration.Hours}h {ParkedDuration.Minutes}m";
    }
}