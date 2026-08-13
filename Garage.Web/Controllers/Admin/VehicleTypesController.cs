using Garage.Web.Data;
using Garage.Web.Models;
using Garage.Web.ViewModels.Admin;
using Garage.Web.Data;
using Garage.Web.Models;
using Garage.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Garage.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class VehicleTypesController : Controller
    {
        private readonly AppDbContext _context;

        public VehicleTypesController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var vehicleTypes = await _context.VehicleTypes
                .Include(v => v.Vehicles)
                .Include(v => v.ParkingSpots)
                .OrderBy(v => v.Name)
                .ToListAsync();

            return View(vehicleTypes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleTypeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var nameExists = await _context.VehicleTypes
                .AnyAsync(v => v.Name == model.Name);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "En fordonstyp med detta namn finns redan."
                );

                return View(model);
            }

            var vehicleType = new VehicleType
            {
                Name = model.Name
            };

            _context.VehicleTypes.Add(vehicleType);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vehicleType = await _context.VehicleTypes.FindAsync(id);

            if (vehicleType == null)
            {
                return NotFound();
            }

            var model = new VehicleTypeEditViewModel
            {
                Id = vehicleType.Id,
                Name = vehicleType.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int id,
    VehicleTypeEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var vehicleType = await _context.VehicleTypes.FindAsync(id);

            if (vehicleType == null)
            {
                return NotFound();
            }

            var nameExists = await _context.VehicleTypes
                .AnyAsync(v => v.Name == model.Name && v.Id != id);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "En annan fordonstyp med detta namn finns redan."
                );

                return View(model);
            }

            vehicleType.Name = model.Name;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicleType = await _context.VehicleTypes
                .Include(v => v.Vehicles)
                .Include(v => v.ParkingSpots)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicleType == null)
            {
                return NotFound();
            }

            return View(vehicleType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicleType = await _context.VehicleTypes
                .Include(v => v.Vehicles)
                .Include(v => v.ParkingSpots)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicleType == null)
            {
                return NotFound();
            }

            if (vehicleType.Vehicles.Any())
            {
                TempData["Error"] =
                    "Fordonstypen kan inte tas bort eftersom den används av ett eller flera fordon.";

                return RedirectToAction(nameof(Index));
            }

            if (vehicleType.ParkingSpots.Any())
            {
                TempData["Error"] =
                    "Fordonstypen kan inte tas bort eftersom den används av en eller flera parkeringsplatser.";

                return RedirectToAction(nameof(Index));
            }

            _context.VehicleTypes.Remove(vehicleType);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Fordonstypen har tagits bort.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var vehicleType = await _context.VehicleTypes
                .Include(v => v.Vehicles)
                .Include(v => v.ParkingSpots)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicleType == null)
            {
                return NotFound();
            }

            return View(vehicleType);
        }

    }
}
