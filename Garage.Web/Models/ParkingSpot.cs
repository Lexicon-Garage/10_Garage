using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Garage.Web.Models
{	
	[Microsoft.EntityFrameworkCore.Index(nameof(SpotNumber), IsUnique = true)]
	public class ParkingSpot
	{
		public int Id { get; set; }

		[Required]
		[StringLength(20, MinimumLength = 1)]
		public required string SpotNumber { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 1)]
		public required string Location { get; set; }

		public bool IsOutOfService { get; set; }
				
		[Required]
		public required int VehicleTypeId { get; set; }

		[ForeignKey(nameof(VehicleTypeId))]
		public VehicleType? VehicleType { get; set; }

		public ICollection<ParkingAllocation> ParkingAllocations { get; set; } = new List<ParkingAllocation>();
	}
}
