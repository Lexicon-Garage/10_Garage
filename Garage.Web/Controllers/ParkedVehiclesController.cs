
using Garage.Web.Data;
using Garage.Web.Helper;
using Garage.Web.Models;
using Garage.Web.ViewModels;
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
    public async Task<IActionResult> Index(string? searchString)    
    {

    var query = _context.ParkedVehicles.AsQueryable();

    if (!string.IsNullOrEmpty(searchString))
    {
        var term = searchString.Trim();
        query = query.Where(v => v.RegistrationNumber.Contains(term));
    }

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

        return View(parkedvehicle);
    }

    // GET: PARKEDVEHICLES/Create
    public IActionResult Create()
    {
		return View();
    }

	private void NormalizeInput(CreateParkVehicleViewModel viewModel)
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
			var vehicle = new ParkedVehicle
			{
				VehicleType = viewModel.VehicleType,
				RegistrationNumber = viewModel.RegistrationNumber!,
				Color = viewModel.Color,
				BrandType = viewModel.Brand,
                Model = viewModel.Model,
				NumberOfWheels = viewModel.WheelsCount,
				ArrivedTime = DateTime.Now
            };

			_context.Add(vehicle);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
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
            BrandTypes= EnumHelper.ToSelectList<BrandType>(),
            VehicleTypes=EnumHelper.ToSelectList<VehicleType>()
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
            return View(parkedViewvehicle);

        var vehicle = _context.ParkedVehicles.Find(parkedViewvehicle.Id);
        if (vehicle == null)
            return NotFound();

        bool exists = _context.ParkedVehicles.Any(v =>
            v.RegistrationNumber == parkedViewvehicle.RegistrationNumber && v.Id != parkedViewvehicle.Id);

        if (exists)
        {
            ModelState.AddModelError("RegistrationNumber", "Already exists");
            return View(parkedViewvehicle);
        }

        vehicle.RegistrationNumber = parkedViewvehicle.RegistrationNumber;
        vehicle.VehicleType = parkedViewvehicle.VehicleType;
        vehicle.Color = parkedViewvehicle.Color;
        vehicle.NumberOfWheels = parkedViewvehicle.NumberOfWheels;
        vehicle.Model = parkedViewvehicle.Model;
        vehicle.BrandType = parkedViewvehicle.BrandType;

        _context.SaveChanges();

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

        return RedirectToAction("Index", "Receipts");
    }
}
