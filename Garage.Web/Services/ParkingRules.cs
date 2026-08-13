namespace Garage.Web.Services;

public static class ParkingRules
{
    public const int MinimumOwnerAge = 18;

    public static ParkingError Validate(ParkingRequest r, string currentUserId, DateTime now)
    {
        if (r.VehicleOwnerId != currentUserId)          return ParkingError.VehicleNotOwnedByUser;
        if (r.VehicleHasActiveSession)                  return ParkingError.VehicleAlreadyParked;
        if (AgeAt(r.OwnerDateOfBirth, now) < MinimumOwnerAge) return ParkingError.OwnerUnderage;
        if (r.SpotIsOutOfService)                       return ParkingError.SpotOutOfService;
        if (r.SpotHasActiveSession)                     return ParkingError.SpotOccupied;

        return ParkingError.None;
    }

    public static int AgeAt(DateOnly birthDate, DateTime at)
    {
        var today = DateOnly.FromDateTime(at);
        var age = today.Year - birthDate.Year;
        if (today < birthDate.AddYears(age)) age--;
        return age;
    }
}
