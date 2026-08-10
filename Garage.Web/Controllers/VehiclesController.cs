using Garage.Web.Data;
using Garage.Web.Models;
using Garage.Web.ViewModels;
using Garage.Web.ViewModels.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[Authorize]
public class VehiclesController : Controller
{
	private readonly AppDbConext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public VehiclesController(AppDbConext context, UserManager<ApplicationUser> userManager)
	{
		_context = context;
        _userManager = userManager;
    }

    // GET: Vehicles
    public async Task<IActionResult> Index(string? searchString, string? sortColumn, string? sortDir)
	{

        var query = _context.Vehicles
            .Include(v => v.VehicleType)
            .AsQueryable();

        if (!User.IsInRole("Admin"))
        {
            var userId = _userManager.GetUserId(User);
            query = query.Where(v => v.OwnerId == userId);
        }

        // --- Search (extended) ---
        if (!string.IsNullOrWhiteSpace(searchString))
		{
			var term = searchString.Trim();

            query = query.Where(v =>
           v.RegistrationNumber.Contains(term) ||
           v.Color.Contains(term) ||
           v.Model.Contains(term) ||
           v.VehicleType!.Name.Contains(term));
        }

		// --- Sort ---
		bool desc = sortDir == "desc";
		query = sortColumn switch
		{
            "type" => desc ? query.OrderByDescending(v => v.VehicleType!.Name)
             : query.OrderBy(v => v.VehicleType!.Name),
            "reg" => desc ? query.OrderByDescending(v => v.RegistrationNumber)
							  : query.OrderBy(v => v.RegistrationNumber),
            _ => query.OrderBy(v => v.Id)

            //"arrived" => desc ? query.OrderByDescending(v => v.ArrivedTime)
            //				  : query.OrderBy(v => v.ArrivedTime),
            //_ => query.OrderBy(v => v.ArrivedTime)
        };

		var vehicles = await query
			.Select(v => new VehicleOverviewViewModel
			{
				Id = v.Id,
				VehicleType = v.VehicleType!.Name,
				RegistrationNumber = v.RegistrationNumber,
				//ArrivedTime = v.ArrivedTime
			})
			.ToListAsync();

		ViewData["SearchString"] = searchString;
		ViewData["SortColumn"] = sortColumn;
		ViewData["SortDir"] = desc ? "desc" : "asc";

		return View(vehicles);
	}
	// GET: Vehicles/Details/5
	public async Task<IActionResult> Details(int? id)
	{
		if (id == null)
		{
			return NotFound();
		}
        var vehicle = await GetAuthorizedVehicleAsync(id.Value);

        if (vehicle == null)
		{
			return NotFound();
		}

        var viewModel = new VehicleDetailsViewModel
		{
			Id = vehicle.Id,
			RegistrationNumber = vehicle.RegistrationNumber,
			VehicleType = vehicle.VehicleType!.Name,
			Color = vehicle.Color,
			NumberOfWheels = vehicle.NumberOfWheels,
			Model = vehicle.Model,
			Brand = vehicle.BrandType!.Name,
			//ArrivedTime = vehicle.ArrivedTime
		};

		return View(viewModel);
	}

    // GET: Vehicles/Create
    public async Task<IActionResult> Create()
    {
		var vm = new CreateParkVehicleViewModel

		{
			VehicleTypes = await _context.VehicleTypes
				.Select(v => new SelectListItem
				{
					Value = v.Id.ToString(),
					Text = v.Name
				})
				.ToListAsync(),

			BrandTypes = await _context.BrandTypes
				.Select(v => new SelectListItem
				{
					Value = v.Id.ToString(),
					Text = v.Name
				})
				.ToListAsync()
		};

        return View(vm);
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
	 

	// POST: Vehicles/Create
	// To protect from overposting attacks, enable the specific properties you want to bind to.
	// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CreateParkVehicleViewModel viewModel)
	{
		NormalizeInput(viewModel);

		bool isAlreadyParked = await _context.Vehicles
		.AnyAsync(v => v.RegistrationNumber == viewModel.RegistrationNumber);

		if (isAlreadyParked)
			ModelState.AddModelError("RegistrationNumber", "A vehicle with this registration number is already exists.");
		
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        if (ModelState.IsValid)
		{
			try
			{
                var vehicle = new Vehicle
				{
					VehicleTypeId = viewModel.VehicleTypeId,
					RegistrationNumber = viewModel.RegistrationNumber,
					Color = viewModel.Color,
					BrandTypeId = viewModel.BrandTypeId,
					Model = viewModel.Model,
					NumberOfWheels = viewModel.WheelsCount,
                    OwnerId = userId

                    //ArrivedTime = DateTime.Now
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

	// GET: Vehicles/Edit/5
	public async Task<IActionResult> Edit(int? id)
	{
        if (id == null)
        {
            return NotFound();
        }
     
        var vehicle = await GetAuthorizedVehicleAsync(id.Value);


        if (vehicle == null)
        {
            return NotFound();
        }

        var vehicleEditViewModel = new VehicleEditViewModel
		{
			Id = vehicle.Id,
			RegistrationNumber = vehicle.RegistrationNumber,
			VehicleTypes =  await _context.VehicleTypes
            .Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.Name,
                Selected = v.Id == vehicle.VehicleTypeId
            })
            .ToListAsync(),
            BrandTypes = await _context.BrandTypes
            .Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.Name,
                Selected = v.Id == vehicle.BrandTypeId
            })
            .ToListAsync(),
            Color = vehicle.Color,
			NumberOfWheels = vehicle.NumberOfWheels,
			Model = vehicle.Model,
			BrandTypeId = vehicle.BrandTypeId,
            VehicleTypeId = vehicle.VehicleTypeId,
            //ArrivedTime = vehicle.ArrivedTime,
            //BrandTypes = EnumHelper.ToSelectList<BrandType>(),
            //VehicleTypes = EnumHelper.ToSelectList<VehicleType>()
        };

		return View(vehicleEditViewModel);
	}

	// POST: Vehicles/CheckOut/5
	// To protect from overposting attacks, enable the specific properties you want to bind to.
	// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int? id, VehicleEditViewModel vehicleEditViewModel)
	{
		if (!ModelState.IsValid)
		{
            vehicleEditViewModel.VehicleTypes = await _context.VehicleTypes
            .Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.Name,
                Selected = v.Id == vehicleEditViewModel.VehicleTypeId
            })
            .ToListAsync();

            return View(vehicleEditViewModel);
		}

		NormalizeInput(vehicleEditViewModel);

        if (id == null)
        {
            return NotFound();
        }

        var vehicle = await GetAuthorizedVehicleAsync(id.Value);

        if (vehicle == null)
        {
            return NotFound();
        }


		bool exists = _context.Vehicles.Any(v =>
			v.RegistrationNumber == vehicleEditViewModel.RegistrationNumber && v.Id != id.Value);

		if (exists)
		{
			ModelState.AddModelError("RegistrationNumber",
				$"A vehicle with registration number {vehicleEditViewModel.RegistrationNumber} is already exists.");
			vehicleEditViewModel.BrandTypeId = vehicle.BrandTypeId;
			vehicleEditViewModel.VehicleTypes = await _context.VehicleTypes
			.Select(v => new SelectListItem
			{
				Value = v.Id.ToString(),
				Text = v.Name,
				Selected = v.Id == vehicleEditViewModel.VehicleTypeId
			})
			.ToListAsync();
            //vehicleEditViewModel.VehicleTypes = EnumHelper.ToSelectList<VehicleType>();
            return View(vehicleEditViewModel);
		}
		try
		{
			vehicle.RegistrationNumber = vehicleEditViewModel.RegistrationNumber;
			vehicle.VehicleTypeId = vehicleEditViewModel.VehicleTypeId;
			vehicle.Color = vehicleEditViewModel.Color;
			vehicle.NumberOfWheels = vehicleEditViewModel.NumberOfWheels;
			vehicle.Model = vehicleEditViewModel.Model;
			vehicle.BrandTypeId = vehicleEditViewModel.BrandTypeId;

            await _context.SaveChangesAsync(); 
			TempData["ValidationMessage"] = "The vehicle has been updated successfully.";
		}
		catch (DbUpdateException)
		{
			TempData["ValidationMessage"] = "Could not update the vehicle data. Please try again.";
		}


		return RedirectToAction(nameof(Index));
	}

	// GET: Vehicles/CheckOut/5
	[HttpGet]
	public async Task<IActionResult> CheckOut(int? id)
	{
        if (id == null)
        {
            return NotFound();
        }

        var vehicle = await GetAuthorizedVehicleAsync(id.Value);


        if (vehicle == null)
        {
            return NotFound();
        }
      
		return View(vehicle);
	}

	// POST: Vehicles/CheckOut/5
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> CheckOut(int id)
	{
        var vehicle = await GetAuthorizedVehicleAsync(id);

        if (vehicle == null)
        {
            return NotFound();
        }

        try
		{
			var receipt = new ReceiptViewModel
			{
				RegistrationNumber = vehicle.RegistrationNumber,
				VehicleType = vehicle.VehicleType!.Name,
				//CheckInTime = vehicle.ArrivedTime,
				CheckOutTime = DateTime.Now,

			};

			_context.Vehicles.Remove(vehicle);
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
    private async Task<Vehicle?> GetAuthorizedVehicleAsync(int id)
    {
        var query = _context.Vehicles.Include(v => v.VehicleType);

        if (User.IsInRole("Admin"))
        {
            return await query.FirstOrDefaultAsync(v => v.Id == id);
        }

        var userId = _userManager.GetUserId(User);

        return await query.FirstOrDefaultAsync(v =>
            v.Id == id &&
            v.OwnerId == userId);
    }
}
