namespace Garage.Web.ViewModels;

/// <summary>
/// One active parking session, for the parking overview (US06).
/// Deliberately separate from VehicleOverviewViewModel, which describes a
/// registered vehicle and has no session data.
/// </summary>
public class ParkingOverviewViewModel
{
    public int SessionId { get; set; }
    public int VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleTypeName { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public List<string> SpotNumbers { get; set; } = new();
    public decimal HourlyRateAtCheckIn { get; set; }
    public decimal RunningCost => Math.Ceiling((decimal)ParkedDuration.TotalHours) * HourlyRateAtCheckIn;

    public TimeSpan ParkedDuration => DateTime.Now - CheckInTime;

    public string ParkedDurationDisplay =>
        ParkedDuration.Days > 0
            ? $"{ParkedDuration.Days}d {ParkedDuration.Hours}h {ParkedDuration.Minutes}m"
            : $"{ParkedDuration.Hours}h {ParkedDuration.Minutes}m";
}