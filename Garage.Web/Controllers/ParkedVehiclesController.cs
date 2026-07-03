
using Garage.Web.Data;
using Garage.Web.Helper;
using Garage.Web.Models;
using Garage.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ParkedVehiclesController : Controller
{
    private readonly AppDbConext _context;

    public ParkedVehiclesController(AppDbConext context)
    {
        _context = context;
    }

    // GET: PARKEDVEHICLES
    public async Task<IActionResult> Index()    
    {
        return View(await _context.ParkedVehicles.ToListAsync());
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

    // POST: PARKEDVEHICLES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,RegistrationNumber,VehicleType,Color,NumberOfWheels,Model,BrandType,ArrivedTime")] ParkedVehicle parkedvehicle)
    {
        if (ModelState.IsValid)
        {
            _context.Add(parkedvehicle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(parkedvehicle);
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

    // POST: PARKEDVEHICLES/Edit/5
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

    // GET: PARKEDVEHICLES/Delete/5
    public async Task<IActionResult> Delete(int? id)
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

    // POST: PARKEDVEHICLES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var parkedvehicle = await _context.ParkedVehicles.FindAsync(id);
        if (parkedvehicle != null)
        {
            _context.ParkedVehicles.Remove(parkedvehicle);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ParkedVehicleExists(int? id)
    {
        return _context.ParkedVehicles.Any(e => e.Id == id);
    }
}
