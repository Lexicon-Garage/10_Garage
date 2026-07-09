
using Garage.Web.Data;
using Garage.Web.Helper;
using Garage.Web.Models;
using Garage.Web.ViewModels;
using Garage.Web.ViewModels.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class ParkedVehiclesController : Controller
{
	private readonly AppDbConext _context;

	public ParkedVehiclesController(AppDbConext context)
	{
		_context = context;
	}

	// GET: PARKEDVEHICLES
	public async Task<IActionResult> Index(string? searchString, string? sortColumn, string? sortDir)
	{

		var query = _context.ParkedVehicles.AsQueryable();

		// --- Search (extended) ---
		if (!string.IsNullOrWhiteSpace(searchString))
		{
			var term = searchString.Trim();
			var isType = Enum.TryParse<VehicleType>(term, true, out var type);

			query = query.Where(v =>
				v.RegistrationNumber.Contains(term) ||
				(v.Color != null && v.Color.Contains(term)) ||
				(v.Model != null && v.Model.Contains(term)) ||
				(isType && v.VehicleType == type));
		}

		// --- Sort ---
		bool desc = sortDir == "desc";
		query = sortColumn switch
		{
			"type" => desc ? query.OrderByDescending(v => v.VehicleType)
							  : query.OrderBy(v => v.VehicleType),
			"reg" => desc ? query.OrderByDescending(v => v.RegistrationNumber)
							  : query.OrderBy(v => v.RegistrationNumber),
			"arrived" => desc ? query.OrderByDescending(v => v.ArrivedTime)
							  : query.OrderBy(v => v.ArrivedTime),
			_ => query.OrderBy(v => v.ArrivedTime)
		};

		var vehicles = await query
			.Select(v => new ParkedVehicleOverviewViewModel
			{
				Id = v.Id,
				VehicleType = v.VehicleType,
				RegistrationNumber = v.RegistrationNumber,
				ArrivedTime = v.ArrivedTime
			})
			.ToListAsync();

		ViewData["SearchString"] = searchString;
		ViewData["SortColumn"] = sortColumn;
		ViewData["SortDir"] = desc ? "desc" : "asc";

		return View(vehicles);
	}
	// GET: PARKEDVEHICLES/Details/5
	public async Task<IActionResult> Details(int? id)
	{
		if (id == null)
		{
			return NotFound();
		}

		var parkedvehicle = await _context.ParkedVehicles
			.FirstOrDefaultAsync(m => m.Id == id);
		if (parkedvehicle == null)
		{
			return NotFound();
		}

		var viewModel = new ParkedVehicleDetailsViewModel
		{
			Id = parkedvehicle.Id,
			RegistrationNumber = parkedvehicle.RegistrationNumber,
			VehicleType = parkedvehicle.VehicleType,
			Color = parkedvehicle.Color,
			NumberOfWheels = parkedvehicle.NumberOfWheels,
			Model = parkedvehicle.Model,
			BrandType = parkedvehicle.BrandType,
			ArrivedTime = parkedvehicle.ArrivedTime
		};

		return View(viewModel);
	}

	// GET: PARKEDVEHICLES/Create
	public IActionResult Create()
	{
		return View();
	}

	private void NormalizeInput(IVehicleFormModel viewModel)
	{
		if (viewModel.RegistrationNumber != null)
			viewModel.RegistrationNumber = viewModel.RegistrationNumber.Trim().ToUpper();

		if (viewModel.Color != null)
			viewModel.Color = viewModel.Color.Trim();

		if (viewModel.Model != null)
			viewModel.Model = viewModel.Model.Trim();
	}

	// POST: PARKEDVEHICLES/Create
	// To protect from overposting attacks, enable the specific properties you want to bind to.
	// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CreateParkVehicleViewModel viewModel)
	{

		NormalizeInput(viewModel);

		bool isAlreadyParked = await _context.ParkedVehicles
		.AnyAsync(v => v.RegistrationNumber == viewModel.RegistrationNumber);

		if (isAlreadyParked)
			ModelState.AddModelError("RegistrationNumber", "A vehicle with this registration number is already parked in the garage.");


		if (ModelState.IsValid)
		{
			try
			{
				var vehicle = new ParkedVehicle
				{
					VehicleType = viewModel.VehicleType,
					RegistrationNumber = viewModel.RegistrationNumber,
					Color = viewModel.Color,
					BrandType = viewModel.Brand,
					Model = viewModel.Model,
					NumberOfWheels = viewModel.WheelsCount,
					ArrivedTime = DateTime.Now
				};

				_context.Add(vehicle);
				await _context.SaveChangesAsync();
				TempData["ValidationMessage"] = "The vehicle has been successfully parked.";

				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateException)
			{
				TempData["ValidationMessage"] = "Could not save the vehicle data. Please try again.";
			}
		}


		return View(viewModel);
	}

	// GET: PARKEDVEHICLES/Edit/5
	public async Task<IActionResult> Edit(int? id)
	{
		if (id == null)
		{
			return NotFound();
		}

		var parkedvehicle = await _context.ParkedVehicles.FindAsync(id);

		if (parkedvehicle == null)
		{
			return NotFound();
		}

		var parkedVehicleEditViewModel = new ParkedVehicleEditViewModel
		{
			Id = parkedvehicle.Id,
			RegistrationNumber = parkedvehicle.RegistrationNumber,
			VehicleType = parkedvehicle.VehicleType,
			Color = parkedvehicle.Color,
			NumberOfWheels = parkedvehicle.NumberOfWheels,
			Model = parkedvehicle.Model,
			BrandType = parkedvehicle.BrandType,
			ArrivedTime = parkedvehicle.ArrivedTime,
			BrandTypes = EnumHelper.ToSelectList<BrandType>(),
			VehicleTypes = EnumHelper.ToSelectList<VehicleType>()
		};

		return View(parkedVehicleEditViewModel);
	}

	// POST: PARKEDVEHICLES/CheckOut/5
	// To protect from overposting attacks, enable the specific properties you want to bind to.
	// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int? id, [Bind("Id,RegistrationNumber,VehicleType,Color,NumberOfWheels,Model,BrandType,ArrivedTime")] ParkedVehicleEditViewModel parkedViewvehicle)
	{
		if (!ModelState.IsValid)
		{
			parkedViewvehicle.BrandTypes = EnumHelper.ToSelectList<BrandType>();
			parkedViewvehicle.VehicleTypes = EnumHelper.ToSelectList<VehicleType>();
			return View(parkedViewvehicle);
		}
		NormalizeInput(parkedViewvehicle);

		var vehicle = _context.ParkedVehicles.Find(parkedViewvehicle.Id);
		if (vehicle == null)
			return NotFound();

		var reg = parkedViewvehicle.RegistrationNumber.Trim().ToUpper();

		bool exists = _context.ParkedVehicles.Any(v =>
			v.RegistrationNumber == reg && v.Id != parkedViewvehicle.Id);

		if (exists)
		{
			ModelState.AddModelError("RegistrationNumber",
				$"A vehicle with registration number {reg} is already parked.");
			parkedViewvehicle.BrandTypes = EnumHelper.ToSelectList<BrandType>();
			parkedViewvehicle.VehicleTypes = EnumHelper.ToSelectList<VehicleType>();
			return View(parkedViewvehicle);
		}
		try
		{
			vehicle.RegistrationNumber = parkedViewvehicle.RegistrationNumber;
			vehicle.VehicleType = parkedViewvehicle.VehicleType;
			vehicle.Color = parkedViewvehicle.Color;
			vehicle.NumberOfWheels = parkedViewvehicle.NumberOfWheels;
			vehicle.Model = parkedViewvehicle.Model;
			vehicle.BrandType = parkedViewvehicle.BrandType;

			_context.SaveChanges();
			TempData["ValidationMessage"] = "The vehicle has been updated successfully.";
		}
		catch (DbUpdateException)
		{
			TempData["ValidationMessage"] = "Could not update the vehicle data. Please try again.";
		}


		return RedirectToAction(nameof(Index));
	}

	// GET: PARKEDVEHICLES/CheckOut/5
	[HttpGet]
	public async Task<IActionResult> CheckOut(int? id)
	{
		if (id == null)
		{
			return NotFound();
		}

		var parkedvehicle = await _context.ParkedVehicles
			.FirstOrDefaultAsync(m => m.Id == id);
		if (parkedvehicle == null)
		{
			return NotFound();
		}

		return View(parkedvehicle);
	}

	// POST: PARKEDVEHICLES/CheckOut/5
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> CheckOut(int id)
	{
		var vehicle = await _context.ParkedVehicles.FindAsync(id);

		if (vehicle == null)
			return NotFound();

		try
		{
			var receipt = new ReceiptViewModel
			{
				RegistrationNumber = vehicle.RegistrationNumber,
				VehicleType = vehicle.VehicleType,
				CheckInTime = vehicle.ArrivedTime,
				CheckOutTime = DateTime.Now,
			};

			_context.ParkedVehicles.Remove(vehicle);
			await _context.SaveChangesAsync();

			TempData["Receipt"] = JsonSerializer.Serialize(receipt);
			TempData["ValidationMessage"] = "The vehicle has been checked out successfully.";
		}
		catch (DbUpdateException)
		{
			TempData["ValidationMessage"] = "Could not check out the vehicle. Please try again.";
		}

		return RedirectToAction("Index", "Receipts");
	}
}
