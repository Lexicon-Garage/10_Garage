using Garage.Web.Models;
using Garage.Web.ViewModels.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.ViewModels
{
	public class CreateParkVehicleViewModel : IVehicleFormModel
	{
		[Required]
		[Display(Name = "Vehicle Type")]
		public VehicleType VehicleType { get; set; }

		[Required(ErrorMessage = "Registration number is required.")]
		[RegularExpression(@"^[A-Za-z0-9]{2,60}$",
		 ErrorMessage = "5–60 letters or digits, no special characters.")]
		[Display(Name = "Registration Number")]
		public required string RegistrationNumber { get; set; }

		[Required]
		[StringLength(20, MinimumLength = 2)]
		public required string Color { get; set; }

		[Required]
		public BrandType Brand { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 1)]
		public required string Model { get; set; }

		[Required]
		[Range(1, 16)]
		[Display(Name = "Number of Wheels")]
		public int WheelsCount { get; set; }
	}
}
