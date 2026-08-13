using Microsoft.AspNetCore.Mvc;

namespace Garage.Web.ViewModels.Admin
{
    public class ParkingSpotListViewModel
    {
        public int Id { get; set; }

        public string SpotNumber { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public bool IsOutOfService { get; set; }

        public string VehicleTypeName { get; set; } = string.Empty;

        // Calculated from active ParkingSession.
        // This should NOT exist in the ParkingSpot entity.
        public bool IsOccupied { get; set; }
    }
}
