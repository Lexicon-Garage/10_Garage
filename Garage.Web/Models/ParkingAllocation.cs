using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Garage.Web.Models
{
	[PrimaryKey(nameof(ParkingSessionId), nameof(ParkingSpotId))]
	public class ParkingAllocation
	{
		[Required]
		public required int ParkingSessionId { get; set; }

		[ForeignKey(nameof(ParkingSessionId))]
		public ParkingSession? ParkingSession { get; set; }

		[Required]
		public required int ParkingSpotId { get; set; }

		[ForeignKey(nameof(ParkingSpotId))]
		public ParkingSpot? ParkingSpot { get; set; }
	}
}
