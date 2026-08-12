public class ReceiptViewModel
{
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;      // was the enum
    public DateTime CheckInTime { get; set; }
    public DateTime CheckOutTime { get; set; }
    public decimal Price { get; set; }                            // set from session.TotalPrice
    public List<string> SpotNumbers { get; set; } = new();

    private TimeSpan ParkingDuration => CheckOutTime - CheckInTime;
    public string ParkingDurationFormatted => ParkingDuration.ToString(@"hh\:mm\:ss");
}