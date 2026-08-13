using Garage.Web.Constants;
using Garage.Web.Models;
using Garage.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Garage.Web.Controllers
{
	[Authorize(Roles = RoleNames.Admin)]
	public class UsersController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public async Task<IActionResult> Index()
		{
			var users = await _userManager.Users.ToListAsync();
			return View(users);
		}

		public async Task<IActionResult> Details(string id)
		{
			if (string.IsNullOrEmpty(id))
				return NotFound();

			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
				return NotFound();

			var userRoles = await _userManager.GetRolesAsync(user);

			var viewModel = new UserDetailsViewModel
			{
				UserId = user.Id,
				Email = user.Email ?? string.Empty,
				UserName = user.UserName ?? string.Empty,
				FirstName = user.FirstName,
				LastName = user.LastName,
				CurrentRoles = userRoles,				
				AvailableRoles = RoleNames.All.ToList()
			};

			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateRoles(string id, List<string>? selectedRoles)
		{
			if (string.IsNullOrEmpty(id))
				return NotFound();

			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
				return NotFound();

			// A missing checkbox group posts no values at all, not an empty list -
			// guard against that before calling .Contains()/passing it on.
			selectedRoles ??= new List<string>();

			// Extra safeguard beyond the AC: prevents an admin from locking themselves
			// out by removing their own Admin role.
			var currentLoggedInUser = await _userManager.GetUserAsync(User);

			if (currentLoggedInUser != null && currentLoggedInUser.Id == user.Id)
			{
				if (!selectedRoles.Contains(RoleNames.Admin))
				{
					TempData["ErrorMessage"] = "You cannot remove the Admin role from your own account.";
					return RedirectToAction(nameof(Details), new { id });
				}
			}

			var currentRoles = await _userManager.GetRolesAsync(user);

			var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
			if (!removeResult.Succeeded)
			{
				TempData["ErrorMessage"] = "Failed to reset current user roles.";
				return RedirectToAction(nameof(Details), new { id });
			}

			var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);
			if (!addResult.Succeeded)
			{
				TempData["ErrorMessage"] = "Failed to add selected roles to the user.";
				return RedirectToAction(nameof(Details), new { id });
			}

			TempData["SuccessMessage"] = "User roles updated successfully.";
			return RedirectToAction(nameof(Details), new { id });
		}
	}
}
