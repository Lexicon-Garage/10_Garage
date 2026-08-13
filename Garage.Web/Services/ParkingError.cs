namespace Garage.Web.Services;

public enum ParkingError
{
    None,
    VehicleNotOwnedByUser,
    VehicleAlreadyParked,
    OwnerUnderage,
    SpotOutOfService,
    SpotOccupied
}