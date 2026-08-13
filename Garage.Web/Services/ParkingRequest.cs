namespace Garage.Web.Services;

public record ParkingRequest(
    int VehicleId,
    string VehicleOwnerId,
    DateOnly OwnerDateOfBirth,
    bool VehicleHasActiveSession,
    int SpotId,
    bool SpotIsOutOfService,
    bool SpotHasActiveSession);