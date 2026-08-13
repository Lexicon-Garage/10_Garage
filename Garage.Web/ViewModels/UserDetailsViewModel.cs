namespace Garage.Web.ViewModels
{
	public class UserDetailsViewModel
	{
		public required string UserId { get; set; }
		public required string Email { get; set; }
		public required string UserName { get; set; }
		public required string FirstName { get; set; }
		public required string LastName { get; set; }
		public IList<string> CurrentRoles { get; set; } = new List<string>();
		public List<string> AvailableRoles { get; set; } = new List<string>();
		public bool IsCurrentUser { get; set; }
	}
}
