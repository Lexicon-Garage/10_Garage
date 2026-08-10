using Garage.Web.Configuration;
using Garage.Web.Data;
using Garage.Web.Models;
using Garage.Web.ViewModels;
using Garage.Web.ViewModels.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Garage.Web.Controllers
{
	public class ParkedVehiclesController : Controller
	{
		private readonly AppDbContext _context;
		private readonly PricingOptions _pricing;

		public ParkedVehiclesController(AppDbContext context, IOptions<PricingOptions> pricing)
		{
			_context = context;
			_pricing = pricing.Value;
		}

		// GET: ParkedVehicles
		public async Task<IActionResult> Index(string? searchString, string? sortColumn, string? sortDir)
		{
			var query = _context.ParkingSessions
				.Where(s => s.CheckOutTime == null)
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
					SpotNumbers = s.ParkingAllocations
						.Select(a => a.ParkingSpot!.SpotNumber)
						.ToList()
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
			if (id == null) return NotFound();

			var session = await _context.ParkingSessions
				.Include(s => s.Vehicle)!.ThenInclude(v => v!.VehicleType)
				.Include(s => s.Vehicle)!.ThenInclude(v => v!.BrandType)
				.Include(s => s.ParkingAllocations)!.ThenInclude(a => a.ParkingSpot)
				.FirstOrDefaultAsync(s => s.Id == id);

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
			if (!ModelState.IsValid)
			{
				await PopulateParkingListsAsync(viewModel);
				return View(viewModel);
			}

			var vehicle = await _context.Vehicles
				.Include(v => v.VehicleType)
				.FirstOrDefaultAsync(v => v.Id == viewModel.VehicleId);

			if (vehicle == null)
			{
				ModelState.AddModelError(string.Empty, "The selected vehicle does not exist.");
				await PopulateParkingListsAsync(viewModel);
				return View(viewModel);
			}

			bool alreadyParked = await _context.ParkingSessions
				.AnyAsync(s => s.VehicleId == viewModel.VehicleId && s.CheckOutTime == null);

			if (alreadyParked)
			{
				ModelState.AddModelError(string.Empty,
					"This vehicle already has an active parking session.");
				await PopulateParkingListsAsync(viewModel);
				return View(viewModel);
			}

			var spot = await _context.ParkingSpots
				.FirstOrDefaultAsync(p => p.Id == viewModel.ParkingSpotId);

			if (spot == null || spot.IsOutOfService)
			{
				ModelState.AddModelError(string.Empty, "That parking spot is not available.");
				await PopulateParkingListsAsync(viewModel);
				return View(viewModel);
			}

			bool spotTaken = await _context.ParkingAllocations
				.AnyAsync(a => a.ParkingSpotId == viewModel.ParkingSpotId
							&& a.ParkingSession!.CheckOutTime == null);

			if (spotTaken)
			{
				ModelState.AddModelError(string.Empty,
					"That parking spot was just taken. Please choose another.");
				await PopulateParkingListsAsync(viewModel);
				return View(viewModel);
			}

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
				ModelState.AddModelError(string.Empty,
					"The vehicle could not be parked. Please try again.");
				await PopulateParkingListsAsync(viewModel);
				return View(viewModel);
			}
		}

		// GET: ParkedVehicles/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
			if (vehicle == null) return NotFound();

			var viewModel = new ParkedVehicleEditViewModel
			{
				Id = vehicle.Id,
				RegistrationNumber = vehicle.RegistrationNumber,
				VehicleTypeId = vehicle.VehicleTypeId,
				BrandTypeId = vehicle.BrandTypeId,
				Color = vehicle.Color,
				NumberOfWheels = vehicle.NumberOfWheels,
				Model = vehicle.Model
			};

			await PopulateDropdownsAsync(viewModel);
			return View(viewModel);
		}

		// POST: ParkedVehicles/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int? id,
			[Bind("Id,RegistrationNumber,VehicleTypeId,Color,NumberOfWheels,Model,BrandTypeId")]
			ParkedVehicleEditViewModel viewModel)
		{
			if (id != null && id != viewModel.Id) return NotFound();

			if (!ModelState.IsValid)
			{
				await PopulateDropdownsAsync(viewModel);
				return View(viewModel);
			}

			NormalizeInput(viewModel);

			var vehicle = await _context.Vehicles.FindAsync(viewModel.Id);
			if (vehicle == null) return NotFound();

			var reg = viewModel.RegistrationNumber;

			bool exists = await _context.Vehicles
				.AnyAsync(v => v.RegistrationNumber == reg && v.Id != viewModel.Id);

			if (exists)
			{
				ModelState.AddModelError(nameof(viewModel.RegistrationNumber),
					$"A vehicle with registration number {reg} is already registered.");
				await PopulateDropdownsAsync(viewModel);
				return View(viewModel);
			}

			try
			{
				vehicle.RegistrationNumber = reg;
				vehicle.VehicleTypeId = viewModel.VehicleTypeId;
				vehicle.BrandTypeId = viewModel.BrandTypeId;
				vehicle.Color = viewModel.Color;
				vehicle.NumberOfWheels = viewModel.NumberOfWheels;
				vehicle.Model = viewModel.Model;
				// OwnerId is never bound or assigned from the form

				await _context.SaveChangesAsync();
				TempData["ValidationMessage"] = "The vehicle has been updated successfully.";
			}
			catch (DbUpdateException)
			{
				TempData["ValidationMessage"] =
					"Could not update the vehicle data. Please try again.";
			}

			return RedirectToAction(nameof(Index));
		}

		// GET: ParkedVehicles/CheckOut/5
		[HttpGet]
		public async Task<IActionResult> CheckOut(int? id)
		{
			if (id == null) return NotFound();

			var session = await _context.ParkingSessions
				.Include(s => s.Vehicle)!.ThenInclude(v => v!.VehicleType)
				.Include(s => s.Vehicle)!.ThenInclude(v => v!.BrandType)
				.Include(s => s.ParkingAllocations)!.ThenInclude(a => a.ParkingSpot)
				.FirstOrDefaultAsync(s => s.Id == id && s.CheckOutTime == null);

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
			var session = await _context.ParkingSessions
				.Include(s => s.Vehicle)!.ThenInclude(v => v!.VehicleType)
				.Include(s => s.ParkingAllocations)!.ThenInclude(a => a.ParkingSpot)
				.FirstOrDefaultAsync(s => s.Id == id && s.CheckOutTime == null);

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

		// ---------- helpers ----------

		private async Task PopulateDropdownsAsync(ParkedVehicleEditViewModel viewModel)
		{
			viewModel.VehicleTypes = await _context.VehicleTypes
				.OrderBy(t => t.Name)
				.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
				.ToListAsync();

			viewModel.BrandTypes = await _context.BrandTypes
				.OrderBy(b => b.Name)
				.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
				.ToListAsync();
		}

		private async Task PopulateParkingListsAsync(ParkVehicleViewModel viewModel)
		{
			viewModel.Vehicles = await _context.Vehicles
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
	}
}