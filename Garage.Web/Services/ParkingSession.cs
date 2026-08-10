namespace Garage.Web.Services;
public record NewParkingSession(
    int VehicleId, int ParkingSpotId, DateTime CheckInTime, decimal HourlyRateAtCheckIn);

public static class ParkingSessionFactory
{
    public static NewParkingSession Create(int vehicleId, int spotId, DateTime now, decimal hourlyRate)
        => new(vehicleId, spotId, now, hourlyRate);
}
