using Garage.Web.ViewModels.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.ViewModels
{
    public class ParkedVehicleEditViewModel : IVehicleFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Registration number is required.")]
        [RegularExpression(@"^[A-Za-z0-9]{5,20}$",
            ErrorMessage = "5–20 letters or digits, no special characters.")]
        [Display(Name = "Registration Number")]
        public required string RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Choose a vehicle type.")]
        [Range(1, int.MaxValue, ErrorMessage = "Choose a vehicle type.")]
        [Display(Name = "Vehicle Type")]
        public int VehicleTypeId { get; set; }

        [Required(ErrorMessage = "Choose a brand.")]
        [Range(1, int.MaxValue, ErrorMessage = "Choose a brand.")]
        [Display(Name = "Brand")]
        public int BrandTypeId { get; set; }

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

        public IEnumerable<SelectListItem> VehicleTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> BrandTypes { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}