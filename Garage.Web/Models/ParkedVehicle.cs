using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Garage.Web.Models
{
    public class ParkedVehicle
    {
        public int Id { get; set; }
        [Range(5, 60)]
        public required string RegistrationNumber { get; set; }
        public required VehicleType VehicleType { get; set; }
       
        [Range(1, 20)]
        public string? Color { get; set; }
        [Range(1, 16)]
        public required int NumberOfWheels { get; set; }
        [Range(1, 50)]
        public string? Model { get; set; }
        [Range(1, 50)]
        public required BrandType BrandType { get; set; }

        [DataType(DataType.DateTime)]
        public required DateTime ArrivedTime { get; init; }
    }
}
