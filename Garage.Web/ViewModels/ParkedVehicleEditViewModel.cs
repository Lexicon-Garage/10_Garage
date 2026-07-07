using Garage.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.ViewModels
{
    public class ParkedVehicleEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 5)]
        [Display(Name = "Registration Number")]

        public string RegistrationNumber { get; set; } = string.Empty;
        [EnumDataType(typeof(VehicleType))]
        [Display(Name = "Vehicle Type")]

        public VehicleType VehicleType { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2)]
        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Range(1, 16)]
        [Display(Name = "Number of wheels")]
        public int NumberOfWheels { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        [Display(Name = "Model")]
        public string? Model { get; set; }

        [EnumDataType(typeof(BrandType))]
        [Display(Name = "Brand")]
        public BrandType BrandType { get; set; }

        public DateTime ArrivedTime { get; set; }
        public IEnumerable<SelectListItem> BrandTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> VehicleTypes { get; set; } = Enumerable.Empty<SelectListItem>();

    }
}
