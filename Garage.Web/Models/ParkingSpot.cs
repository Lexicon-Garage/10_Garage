using System.ComponentModel.DataAnnotations.Schema;

namespace Garage.Web.Models
{
    public class ParkingSpot
    {
        public int Id { get; set; }
        public string SpotNumber { get; set; } = "";

        public int Capacity { get; set; } = 15;
        public ICollection<ParkingAllocation> ParkingAllocations { get; set; } = new List<ParkingAllocation>();

        [NotMapped]
        public int UsedCapacity => ParkingAllocations.Sum(x => x.OccupiedCapacity);

        [NotMapped]
        public int AvailableCapacity => Capacity - UsedCapacity;

        [NotMapped]
        public bool IsFull => AvailableCapacity <= 0;

        [NotMapped]
        public bool IsEmpty => UsedCapacity == 0;
        public bool CanFit(int capacity)
        {
            return AvailableCapacity >= capacity;
        }
    }
}
