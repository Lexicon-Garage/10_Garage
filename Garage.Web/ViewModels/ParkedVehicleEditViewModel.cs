using Garage.Web.Models;
using Garage.Web.ViewModels.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.ViewModels
{
    public class ParkedVehicleEditViewModel : IVehicleFormModel
	{
        public int Id { get; set; }

        [Required(ErrorMessage = "Registration number is required.")]
        [RegularExpression(@"^[A-Za-z0-9]{5,60}$",
         ErrorMessage = "5–60 letters or digits, no special characters.")]
        [Display(Name = "Registration Number")]
        public required string RegistrationNumber { get; set; }

        [EnumDataType(typeof(VehicleType))]
        [Display(Name = "Vehicle Type")]
        public VehicleType VehicleType { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2)]
        [Display(Name = "Color")]
        public required string Color { get; set; }

		[Range(1, 16)]
        [Display(Name = "Number of wheels")]
        public int NumberOfWheels { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        [Display(Name = "Model")]
        public required string Model { get; set; }

		[EnumDataType(typeof(BrandType))]
        [Display(Name = "Brand")]
        public BrandType BrandType { get; set; }

        public DateTime ArrivedTime { get; set; }
        public IEnumerable<SelectListItem> BrandTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> VehicleTypes { get; set; } = Enumerable.Empty<SelectListItem>();

    }
}
