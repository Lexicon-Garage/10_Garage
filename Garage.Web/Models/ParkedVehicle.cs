using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Garage.Web.Models
{
    public class ParkedVehicle
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 5)]
        [Display(Name = "Registration Number")]
        public required string RegistrationNumber { get; set; }

        [EnumDataType(typeof(VehicleType))]
        [Display(Name = "Vehicle Type")]
        public required VehicleType VehicleType { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2)]
        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Range(1, 16)]
        [Display(Name = "Number of wheels")]
        public required int NumberOfWheels { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        [Display(Name = "Model")]
        public string? Model { get; set; }

        [EnumDataType(typeof(BrandType))]
        [Display(Name = "Brand")]
        public required BrandType BrandType { get; set; }
            
        [DataType(DataType.DateTime)]
        [Display(Name = "Arrived Time")]
        public required DateTime ArrivedTime { get; init; }
    }
}
