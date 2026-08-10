using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.Models
{
	[Index(nameof(Name), IsUnique = true)]
	public class VehicleType
	{
		public int Id { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 2)]
		public required string Name { get; set; }

		public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

		public ICollection<ParkingSpot> ParkingSpots { get; set; } = new List<ParkingSpot>();
	}
}
