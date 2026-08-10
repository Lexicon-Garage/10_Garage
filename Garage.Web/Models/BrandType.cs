using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.Models
{
	[Index(nameof(Name), IsUnique = true)]
	public class BrandType
	{
		public int Id { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 1)]
		public required string Name { get; set; }

		public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
	}
}
