using Garage.Web.Data;
using Garage.Web.Models;
using Garage.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Garage.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class ParkingSpotsController : Controller
    {
        private readonly AppDbContext _context;

        public ParkingSpotsController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var spots = await _context.ParkingSpots
                .Include(p => p.VehicleType)
                .Include(p => p.ParkingAllocations)
                    .ThenInclude(a => a.ParkingSession)
                .OrderBy(p => p.SpotNumber)
                .ToListAsync();

            var model = spots.Select(p => new ParkingSpotListViewModel
            {
                Id = p.Id,
                SpotNumber = p.SpotNumber,
                Location = p.Location,
                IsOutOfService = p.IsOutOfService,

                VehicleTypeName = p.VehicleType?.Name ?? "",

                // Occupied status is calculated.
                // It is NOT stored in ParkingSpot.
                IsOccupied = p.ParkingAllocations
                    .Any(a =>
                        a.ParkingSession != null &&
                        a.ParkingSession.CheckOutTime == null)

            }).ToList();

            return View(model);
        }


        // ============================================================
        // CREATE - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadVehicleTypes();

            return View();
        }


        // ============================================================
        // CREATE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ParkingSpotCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadVehicleTypes(model.VehicleTypeId);

                return View(model);
            }


            // --------------------------------------------------------
            // Check unique SpotNumber
            // --------------------------------------------------------

            var spotExists = await _context.ParkingSpots
                .AnyAsync(p => p.SpotNumber == model.SpotNumber);

            if (spotExists)
            {
                ModelState.AddModelError(
                    nameof(model.SpotNumber),
                    "En parkeringsplats med detta platsnummer finns redan."
                );

                await LoadVehicleTypes(model.VehicleTypeId);

                return View(model);
            }


            // --------------------------------------------------------
            // Check VehicleType exists
            // --------------------------------------------------------

            var vehicleTypeExists = await _context.VehicleTypes
                .AnyAsync(v => v.Id == model.VehicleTypeId);

            if (!vehicleTypeExists)
            {
                ModelState.AddModelError(
                    nameof(model.VehicleTypeId),
                    "Den valda fordonstypen finns inte."
                );

                await LoadVehicleTypes(model.VehicleTypeId);

                return View(model);
            }


            // --------------------------------------------------------
            // Create ParkingSpot
            // --------------------------------------------------------

            var parkingSpot = new ParkingSpot
            {
                SpotNumber = model.SpotNumber,
                Location = model.Location,
                IsOutOfService = model.IsOutOfService,
                VehicleTypeId = model.VehicleTypeId
            };

            _context.ParkingSpots.Add(parkingSpot);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Parkeringsplatsen har skapats.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // EDIT - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var spot = await _context.ParkingSpots
                .FirstOrDefaultAsync(p => p.Id == id);

            if (spot == null)
            {
                return NotFound();
            }


            var model = new ParkingSpotEditViewModel
            {
                Id = spot.Id,
                SpotNumber = spot.SpotNumber,
                Location = spot.Location,
                IsOutOfService = spot.IsOutOfService,
                VehicleTypeId = spot.VehicleTypeId
            };


            await LoadVehicleTypes(model.VehicleTypeId);

            return View(model);
        }


        // ============================================================
        // EDIT - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ParkingSpotEditViewModel model)
        {
            // --------------------------------------------------------
            // Make sure route ID and model ID match
            // --------------------------------------------------------

            if (id != model.Id)
            {
                return BadRequest();
            }


            // --------------------------------------------------------
            // Basic validation
            // --------------------------------------------------------

            if (!ModelState.IsValid)
            {
                await LoadVehicleTypes(model.VehicleTypeId);

                return View(model);
            }


            // --------------------------------------------------------
            // Find parking spot
            // --------------------------------------------------------

            var spot = await _context.ParkingSpots
                .FirstOrDefaultAsync(p => p.Id == id);

            if (spot == null)
            {
                return NotFound();
            }


            // --------------------------------------------------------
            // Check unique SpotNumber
            // --------------------------------------------------------

            var duplicateNumber = await _context.ParkingSpots
                .AnyAsync(p =>
                    p.SpotNumber == model.SpotNumber &&
                    p.Id != id);

            if (duplicateNumber)
            {
                ModelState.AddModelError(
                    nameof(model.SpotNumber),
                    "En annan parkeringsplats har redan detta platsnummer."
                );

                await LoadVehicleTypes(model.VehicleTypeId);

                return View(model);
            }


            // --------------------------------------------------------
            // Check VehicleType exists
            // --------------------------------------------------------

            var vehicleTypeExists = await _context.VehicleTypes
                .AnyAsync(v => v.Id == model.VehicleTypeId);

            if (!vehicleTypeExists)
            {
                ModelState.AddModelError(
                    nameof(model.VehicleTypeId),
                    "Den valda fordonstypen finns inte."
                );

                await LoadVehicleTypes(model.VehicleTypeId);

                return View(model);
            }


            // --------------------------------------------------------
            // Check if parking spot is currently occupied
            // --------------------------------------------------------

            var isOccupied = await _context.ParkingAllocations
                .AnyAsync(a =>
                    a.ParkingSpotId == id &&
                    a.ParkingSession != null &&
                    a.ParkingSession.CheckOutTime == null);


            // --------------------------------------------------------
            // Don't allow VehicleType to change while occupied
            // --------------------------------------------------------

            if (isOccupied &&
                spot.VehicleTypeId != model.VehicleTypeId)
            {
                ModelState.AddModelError(
                    nameof(model.VehicleTypeId),
                    "Fordonstypen kan inte ändras medan parkeringsplatsen är upptagen."
                );

                await LoadVehicleTypes(model.VehicleTypeId);

                return View(model);
            }


            // --------------------------------------------------------
            // Update ParkingSpot
            // --------------------------------------------------------

            spot.SpotNumber = model.SpotNumber;
            spot.Location = model.Location;
            spot.IsOutOfService = model.IsOutOfService;
            spot.VehicleTypeId = model.VehicleTypeId;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Parkeringsplatsen har uppdaterats.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // DETAILS - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var spot = await _context.ParkingSpots
                .Include(p => p.VehicleType)
                .Include(p => p.ParkingAllocations)
                    .ThenInclude(a => a.ParkingSession)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (spot == null)
            {
                return NotFound();
            }

            return View(spot);
        }


        // ============================================================
        // DELETE - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var spot = await _context.ParkingSpots
                .Include(p => p.VehicleType)
                .Include(p => p.ParkingAllocations)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (spot == null)
            {
                return NotFound();
            }

            return View(spot);
        }


        // ============================================================
        // DELETE - POST
        // ============================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var spot = await _context.ParkingSpots
                .Include(p => p.ParkingAllocations)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (spot == null)
            {
                return NotFound();
            }


            // --------------------------------------------------------
            // A spot that has been used cannot be deleted.
            // --------------------------------------------------------

            if (spot.ParkingAllocations.Any())
            {
                TempData["Error"] =
                    "Parkeringsplatsen kan inte tas bort eftersom den används av en eller flera parkeringar.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Delete
            // --------------------------------------------------------

            _context.ParkingSpots.Remove(spot);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Parkeringsplatsen har tagits bort.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // LOAD VEHICLE TYPES
        // ============================================================

        private async Task LoadVehicleTypes(int? selectedId = null)
        {
            var vehicleTypes = await _context.VehicleTypes
                .OrderBy(v => v.Name)
                .ToListAsync();

            ViewBag.VehicleTypes = new SelectList(
                vehicleTypes,
                "Id",
                "Name",
                selectedId
            );
        }
    }
}