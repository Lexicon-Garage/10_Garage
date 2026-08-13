using Garage.Web.Configuration;
using Garage.Web.Data;
using Garage.Web.Models;
using Garage.Web.Services;
using Garage.Web.ViewModels;
using Garage.Web.ViewModels.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
namespace Garage.Web.Controllers
{
	[Authorize]
	public class ParkedVehiclesController : Controller
	{
		private readonly AppDbContext _context;
		private readonly PricingOptions _pricing;

		private readonly UserManager<ApplicationUser> _userManager;

		public ParkedVehiclesController(AppDbContext context, IOptions<PricingOptions> pricing, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_pricing = pricing.Value;
			_userManager = userManager;
		}

		// GET: ParkedVehicles
		public async Task<IActionResult> Index(string? searchString, string? sortColumn, string? sortDir)
		{
			var userId = _userManager.GetUserId(User);

			var query = _context.ParkingSessions
				.Where(s => s.CheckOutTime == null)
				.Where(s => s.Vehicle!.OwnerId == userId)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(searchString))
			{
				var term = searchString.Trim().ToUpper();

				query = query.Where(s =>
					s.Vehicle!.RegistrationNumber.ToUpper().Contains(term) ||
					s.Vehicle.Color.ToUpper().Contains(term) ||
					s.Vehicle.Model.ToUpper().Contains(term) ||
					s.Vehicle.VehicleType!.Name.ToUpper().Contains(term));
			}

			bool desc = sortDir == "desc";

			query = sortColumn switch
			{
				"type" => desc ? query.OrderByDescending(s => s.Vehicle!.VehicleType!.Name)
							   : query.OrderBy(s => s.Vehicle!.VehicleType!.Name),
				"reg" => desc ? query.OrderByDescending(s => s.Vehicle!.RegistrationNumber)
							  : query.OrderBy(s => s.Vehicle!.RegistrationNumber),
				_ => desc ? query.OrderByDescending(s => s.CheckInTime)
						  : query.OrderBy(s => s.CheckInTime)
			};

			var sessions = await query
				.Select(s => new ParkingOverviewViewModel
				{
					SessionId = s.Id,
					VehicleId = s.VehicleId,
					RegistrationNumber = s.Vehicle!.RegistrationNumber,
					VehicleTypeName = s.Vehicle.VehicleType!.Name,
					CheckInTime = s.CheckInTime,
					HourlyRateAtCheckIn = s.HourlyRateAtCheckIn,        // ← add
					SpotNumbers = s.ParkingAllocations.Select(a => a.ParkingSpot!.SpotNumber).ToList()
				})
				.ToListAsync();

			ViewData["SearchString"] = searchString;
			ViewData["SortColumn"] = sortColumn;
			ViewData["SortDir"] = desc ? "desc" : "asc";

			return View(sessions);
		}

		// GET: ParkedVehicles/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			var userId = _userManager.GetUserId(User);

			if (id == null) return NotFound();

			var session = await _context.ParkingSessions
				.Include(s => s.Vehicle)!.ThenInclude(v => v!.VehicleType)
				.Include(s => s.Vehicle)!.ThenInclude(v => v!.BrandType)
				.Include(s => s.ParkingAllocations)!.ThenInclude(a => a.ParkingSpot)
				.FirstOrDefaultAsync(s => s.Id == id && s.Vehicle!.OwnerId == userId);
				
			if (session == null) return NotFound();

			var viewModel = new ParkingDetailsViewModel
			{
				SessionId = session.Id,
				VehicleId = session.VehicleId,
				RegistrationNumber = session.Vehicle!.RegistrationNumber,
				VehicleTypeName = session.Vehicle.VehicleType!.Name,
				BrandName = session.Vehicle.BrandType!.Name,
				Model = session.Vehicle.Model,
				Color = session.Vehicle.Color,
				NumberOfWheels = session.Vehicle.NumberOfWheels,
				CheckInTime = session.CheckInTime,
				CheckOutTime = session.CheckOutTime,
				SpotNumbers = session.ParkingAllocations
					.Select(a => a.ParkingSpot!.SpotNumber)
					.ToList()
			};

			return View(viewModel);
		}

		// GET: ParkedVehicles/Create  (park a registered vehicle)
		public async Task<IActionResult> Create(string? locationFilter)
		{
			var viewModel = new ParkVehicleViewModel { LocationFilter = locationFilter };
			await PopulateParkingListsAsync(viewModel);
			return View(viewModel);
		}

		// POST: ParkedVehicles/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ParkVehicleViewModel viewModel)
		{
			if (!ModelState.IsValid)                       // ← unchanged
			{
				await PopulateParkingListsAsync(viewModel);
				return View(viewModel);
			}

			var userId = _userManager.GetUserId(User)!;

			var vehicle = await _context.Vehicles
				.Include(v => v.VehicleType)
				.Include(v => v.Owner)                     // needed for the age check
				.FirstOrDefaultAsync(v => v.Id == viewModel.VehicleId);

			var spot = await _context.ParkingSpots
				.FirstOrDefaultAsync(p => p.Id == viewModel.ParkingSpotId);

			if (vehicle is null || spot is null)
				return await FailAsync(viewModel, "The selected vehicle or parking spot does not exist.");

			if (!ParkingRules.TryGetBirthDate(vehicle.Owner!.PersonalNumber, out var birthDate))
				return await FailAsync(viewModel, "The owner's personal number is invalid.");

			var request = new ParkingRequest(
				VehicleId: vehicle.Id,
				VehicleOwnerId: vehicle.OwnerId,
				OwnerDateOfBirth: birthDate,
				VehicleHasActiveSession: await _context.ParkingSessions
					.AnyAsync(s => s.VehicleId == vehicle.Id && s.CheckOutTime == null),
				SpotId: spot.Id,
				SpotIsOutOfService: spot.IsOutOfService,
				SpotHasActiveSession: await _context.ParkingAllocations
					.AnyAsync(a => a.ParkingSpotId == spot.Id
								&& a.ParkingSession!.CheckOutTime == null));


			var error = ParkingRules.Validate(request, userId, DateTime.Now);

			if (error != ParkingError.None)
				return await FailAsync(viewModel, error.ToUserMessage());

			await using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var session = new ParkingSession
				{
					VehicleId = vehicle.Id,
					CheckInTime = DateTime.Now,
					HourlyRateAtCheckIn = _pricing.RateFor(vehicle.VehicleType!.Name)
				};

				_context.ParkingSessions.Add(session);
				await _context.SaveChangesAsync();

				_context.ParkingAllocations.Add(new ParkingAllocation
				{
					ParkingSessionId = session.Id,
					ParkingSpotId = spot.Id
				});

				await _context.SaveChangesAsync();
				await transaction.CommitAsync(); 

				TempData["ValidationMessage"] =
					$"{vehicle.RegistrationNumber} is now parked at spot {spot.SpotNumber}.";

				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateException)
			{
				await transaction.RollbackAsync();
				return await FailAsync(viewModel, "The vehicle could not be parked. Please try again.");
			}
		}
		
		// GET: ParkedVehicles/CheckOut/5
		[HttpGet]
		public async Task<IActionResult> CheckOut(int? id)
		{
			if (id == null) return NotFound();

			var userId = _userManager.GetUserId(User)!;

			  var session = await _context.ParkingSessions
				.Include(s => s.Vehicle)!.ThenInclude(v => v!.VehicleType)
				.Include(s => s.ParkingAllocations)!.ThenInclude(a => a.ParkingSpot)
				.FirstOrDefaultAsync(s => s.Id == id
									&& s.CheckOutTime == null
                               		&& s.Vehicle!.OwnerId == userId);

			if (session == null) return NotFound();

			var viewModel = new CheckOutViewModel
			{
				SessionId = session.Id,
				RegistrationNumber = session.Vehicle!.RegistrationNumber,
				VehicleTypeName = session.Vehicle.VehicleType!.Name,
				BrandName = session.Vehicle.BrandType!.Name,
				Model = session.Vehicle.Model,
				Color = session.Vehicle.Color,
				CheckInTime = session.CheckInTime,
				SpotNumbers = session.ParkingAllocations
					.Select(a => a.ParkingSpot!.SpotNumber)
					.ToList()
			};

			return View(viewModel);
		}

		// POST: ParkedVehicles/CheckOut/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CheckOut(int id)
		{
			var userId = _userManager.GetUserId(User)!;

			  var session = await _context.ParkingSessions
				.Include(s => s.Vehicle)!.ThenInclude(v => v!.VehicleType)
				.Include(s => s.ParkingAllocations)!.ThenInclude(a => a.ParkingSpot)
				.FirstOrDefaultAsync(s => s.Id == id
									&& s.CheckOutTime == null
                               		&& s.Vehicle!.OwnerId == userId);

			if (session == null)
			{
				TempData["ValidationMessage"] =
					"The parking session is already closed or does not exist.";
				return RedirectToAction(nameof(Index));
			}

			try
			{
				var checkOutTime = DateTime.Now;

				session.CheckOutTime = checkOutTime;
				session.TotalPrice = _pricing.CalculatePrice(
					checkOutTime - session.CheckInTime,
					session.HourlyRateAtCheckIn);   // the rate stamped at check-in

				await _context.SaveChangesAsync();

				var receipt = new ReceiptViewModel
				{
					RegistrationNumber = session.Vehicle!.RegistrationNumber,
					VehicleType = session.Vehicle.VehicleType!.Name,
					CheckInTime = session.CheckInTime,
					CheckOutTime = checkOutTime,
					Price = session.TotalPrice.Value,
					SpotNumbers = session.ParkingAllocations
						.Select(a => a.ParkingSpot!.SpotNumber)
						.ToList()
				};

				TempData["Receipt"] = JsonSerializer.Serialize(receipt);
				TempData["ValidationMessage"] =
					"The vehicle has been checked out successfully.";
			}
			catch (DbUpdateException)
			{
				TempData["ValidationMessage"] =
					"Could not check out the vehicle. Please try again.";
				return RedirectToAction(nameof(Index));
			}

			return RedirectToAction("Index", "Receipts");
		}

		private async Task PopulateParkingListsAsync(ParkVehicleViewModel viewModel)
		{
			var userId = _userManager.GetUserId(User);

			viewModel.Vehicles = await _context.Vehicles
				.Where(v => v.OwnerId == userId)
				.Where(v => !v.ParkingSessions.Any(s => s.CheckOutTime == null))
				.OrderBy(v => v.RegistrationNumber)
				.Select(v => new SelectListItem
				{
					Value = v.Id.ToString(),
					Text = v.RegistrationNumber + " — " + v.VehicleType!.Name
				})
				.ToListAsync();

			var spotsQuery = _context.ParkingSpots
				.Where(p => !p.IsOutOfService)
				.Where(p => !p.ParkingAllocations
					.Any(a => a.ParkingSession!.CheckOutTime == null));

			if (!string.IsNullOrWhiteSpace(viewModel.LocationFilter))
			{
				var location = viewModel.LocationFilter.Trim();
				spotsQuery = spotsQuery.Where(p => p.Location.Contains(location));
			}

			viewModel.ParkingSpots = await spotsQuery
				.OrderBy(p => p.SpotNumber)
				.Select(p => new SelectListItem
				{
					Value = p.Id.ToString(),
					Text = p.SpotNumber + " (" + p.Location + ")"
				})
				.ToListAsync();
		}

		private static void NormalizeInput(IVehicleFormModel viewModel)
		{
			viewModel.RegistrationNumber = viewModel.RegistrationNumber.Trim().ToUpper();
			viewModel.Color = viewModel.Color.Trim();
			viewModel.Model = viewModel.Model.Trim();
		}
		private async Task<IActionResult> FailAsync(ParkVehicleViewModel viewModel, string message)
		{
			ModelState.AddModelError(string.Empty, message);
			await PopulateParkingListsAsync(viewModel);
			return View(viewModel);
		}
	}
}