using Garage.Web.ViewModels.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.ViewModels
{
	public class CreateParkVehicleViewModel : IVehicleFormModel
	{

		[Required(ErrorMessage = "Registration number is required.")]
		[RegularExpression(@"^[A-Za-z0-9]{5,60}$",
		 ErrorMessage = "5–60 letters or digits, no special characters.")]
		[Display(Name = "Registration Number")]
		public string RegistrationNumber { get; set; } = string.Empty;
		[Required]
		[StringLength(20, MinimumLength = 2)]
		public string Color { get; set; } = string.Empty;

		[Required]
		[StringLength(50, MinimumLength = 1)]
		public string Model { get; set; } = string.Empty;

        [Required]
		[Range(1, 16)]
		[Display(Name = "Number of Wheels")]
		public int WheelsCount { get; set; }

        [Required]
        [Display(Name = "Vehicle Type")]
        public int VehicleTypeId { get; set; }

        public IEnumerable<SelectListItem> VehicleTypes { get; set; }= Enumerable.Empty<SelectListItem>();

        [Required]
        [Display(Name = "Brand Type")]
        public int BrandTypeId { get; set; }

        public IEnumerable<SelectListItem> BrandTypes { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
