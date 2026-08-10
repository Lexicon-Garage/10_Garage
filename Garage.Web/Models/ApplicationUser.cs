using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Garage.Web.Models
{
	[Index(nameof(PersonalNumber), IsUnique = true)]
	public class ApplicationUser : IdentityUser 
	{
		[Required]
		[StringLength(50, MinimumLength = 1)]
		public required string FirstName { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 1)]
		public required string LastName { get; set; }
				
		[Required]
		[StringLength(13, MinimumLength = 13)]
		[RegularExpression(@"^\d{8}-\d{4}$", ErrorMessage = "Personal number must follow the format ÅÅÅÅMMDD-XXXX.")]
		public required string PersonalNumber { get; set; }

		[Required]
		[DataType(DataType.DateTime)]
		public required DateTime ProMembershipStart { get; set; }

		[Required]
		[DataType(DataType.DateTime)]
		public required DateTime ProMembershipEnd { get; set; }

		public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
	}
}
