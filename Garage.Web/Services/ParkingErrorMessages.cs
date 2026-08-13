using Garage.Web.Services;

public static class ParkingErrorMessages
{
    public static string ToUserMessage(this ParkingError error) => error switch
    {
        ParkingError.VehicleNotOwnedByUser => "You can only park your own vehicles.",
        ParkingError.VehicleAlreadyParked  => "This vehicle already has an active parking session.",
        ParkingError.OwnerUnderage         => "The vehicle owner must be at least 18 years old to park.",
        ParkingError.SpotOutOfService      => "That parking spot is out of service. Please choose another.",
        ParkingError.SpotOccupied          => "That parking spot was just taken. Please choose another.",
        ParkingError.None                  => string.Empty,
        _                                  => "The vehicle could not be parked. Please try again."
    };
}