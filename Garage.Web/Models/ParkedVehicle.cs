using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace Garage.Web.Models
{
    public class ParkedVehicle
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 5)]
        public required string RegistrationNumber { get; set; }

        [EnumDataType(typeof(VehicleType))]
        public required VehicleType VehicleType { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string? Color { get; set; }

        [Range(1, 16)]
        public required int NumberOfWheels { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string? Model { get; set; }

        [EnumDataType(typeof(BrandType))]
        public required BrandType BrandType { get; set; }
            
        [DataType(DataType.DateTime)]
        public required DateTime ArrivedTime { get; init; }

        public ICollection<ParkingAllocation> ParkingAllocations { get; set; } = new List<ParkingAllocation>();

        [NotMapped]
        public int SpaceRequired => VehicleType switch
        {
            VehicleType.Bicycle => 3,

            VehicleType.Motorcycle => 5,

            VehicleType.Car => 15,

            VehicleType.Boat => 30,

            VehicleType.Bus => 45,

            VehicleType.Airplane => 45,

            _ => throw new ArgumentOutOfRangeException()
        };

    }
}
