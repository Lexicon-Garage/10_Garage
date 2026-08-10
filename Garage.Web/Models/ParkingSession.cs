using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Garage.Web.Models
{
	public class ParkingSession
	{
		public int Id { get; set; }

		[Required]
		public required int VehicleId { get; set; }

		[ForeignKey(nameof(VehicleId))]
		public Vehicle? Vehicle { get; set; }
				
		[Required]
		[DataType(DataType.DateTime)]
		public required DateTime CheckInTime { get; set; }
				
		[DataType(DataType.DateTime)]
		public DateTime? CheckOutTime { get; set; }
				
		[Required]
		[Range(0.01, (double)decimal.MaxValue)]
		public required decimal HourlyRateAtCheckIn { get; set; }
				
		[Range(0, (double)decimal.MaxValue)]
		public decimal? TotalPrice { get; set; }

		public ICollection<ParkingAllocation> ParkingAllocations { get; set; } = new List<ParkingAllocation>();
	}
}
