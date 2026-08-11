using Garage.Web.ViewModels.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.ViewModels
{
    public class VehicleEditViewModel : IVehicleFormModel
	{
        public int Id { get; set; }

        [Required(ErrorMessage = "Registration number is required.")]
        [RegularExpression(@"^[A-Za-z0-9]{5,20}$",
            ErrorMessage = "5–20 letters or digits, no special characters.")]
        [Display(Name = "Registration Number")]
        public required string RegistrationNumber { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2)]
        [Display(Name = "Color")]
        public required string Color { get; set; }

        [Range(0, 16)]
        [Display(Name = "Number of wheels")]
        public int NumberOfWheels { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        [Display(Name = "Model")]
        public required string Model { get; set; }

        [Required]
        [Display(Name = "Vehicle Type")]
        public int VehicleTypeId { get; set; }
        public IEnumerable<SelectListItem> VehicleTypes { get; set; } = Enumerable.Empty<SelectListItem>();

        [Required]
        [Display(Name = "Brand Type")]
        public int BrandTypeId { get; set; }
        public IEnumerable<SelectListItem> BrandTypes { get; set; } = Enumerable.Empty<SelectListItem>();

    }

}
