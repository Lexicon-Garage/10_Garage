namespace Garage.Web.Models
{
    public class ParkingAllocation
    {
        public int Id { get; set; }
        public int ParkingSpotId { get; set; }
        public ParkingSpot ParkingSpot { get; set; } = null!;
        public int ParkedVehicleId { get; set; }
        public ParkedVehicle ParkedVehicle { get; set; } = null!;
        public int OccupiedCapacity { get; set; }
    }
}
