using Garage.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.ViewModels
{
	public class CreateParkVehicleViewModel
	{
		[Required]
		[Display(Name = "Vehicle Type")]
		public VehicleType VehicleType { get; set; }

		[Required(ErrorMessage = "Registration number is required.")]
		[RegularExpression(@"^[A-Za-z0-9]{2,10}$",
		 ErrorMessage = "2–10 letters or digits, no special characters.")]
		[Display(Name = "Registration Number")]
		public string RegistrationNumber { get; set; } = string.Empty;

		[Required]
		[StringLength(20, MinimumLength = 2)]
		public string Color { get; set; } = string.Empty;

		[Required]
		public BrandType Brand { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 1)]
		public string Model { get; set; } = string.Empty;

		[Required]
		[Range(1, 16)]
		[Display(Name = "Number of Wheels")]
		public int WheelsCount { get; set; }
	}
}
