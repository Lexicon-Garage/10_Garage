using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Garage.Web.Models
{
	[Microsoft.EntityFrameworkCore.Index(nameof(RegistrationNumber), IsUnique = true)]
	public class Vehicle
	{
		public int Id { get; set; }

		[Required]
		[StringLength(20, MinimumLength = 5)]
		public required string RegistrationNumber { get; set; }

		[Required]
		[StringLength(20, MinimumLength = 2)]
		public required string Color { get; set; }

		[Range(1, 16)]
		public required int NumberOfWheels { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 1)]
		public required string Model { get; set; }
				
		[Required]
		public required string OwnerId { get; set; }

		[ForeignKey(nameof(OwnerId))]
		public ApplicationUser? Owner { get; set; }

		[Required]
		public required int VehicleTypeId { get; set; }

		[ForeignKey(nameof(VehicleTypeId))]
		public VehicleType? VehicleType { get; set; }

		[Required]
		public required int BrandTypeId { get; set; }

		[ForeignKey(nameof(BrandTypeId))]
		public BrandType? BrandType { get; set; }

		public ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();
	}
}
