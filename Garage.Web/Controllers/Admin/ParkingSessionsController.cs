using Garage.Web.Data;
using Garage.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Garage.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ParkingSessionsController : Controller
    {
        private readonly AppDbContext _context;

        public ParkingSessionsController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchTerm,
            int? vehicleTypeId)
        {
            // --------------------------------------------------------
            // Start with ACTIVE parking sessions only.
            // CheckOutTime == null means the parking is active.
            // --------------------------------------------------------

            var query = _context.ParkingSessions
                .AsNoTracking()
                .Where(p => p.CheckOutTime == null);


            // --------------------------------------------------------
            // SEARCH
            // Search by whole or partial registration number.
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(p =>
                    p.Vehicle != null &&
                    p.Vehicle.RegistrationNumber.Contains(searchTerm));
            }


            // --------------------------------------------------------
            // FILTER BY VEHICLE TYPE
            // Can be combined with the registration search.
            // --------------------------------------------------------

            if (vehicleTypeId.HasValue)
            {
                query = query.Where(p =>
                    p.Vehicle != null &&
                    p.Vehicle.VehicleTypeId == vehicleTypeId.Value);
            }


            // --------------------------------------------------------
            // PROJECT DIRECTLY TO VIEW MODEL
            //
            // IMPORTANT:
            // Select() happens BEFORE ToListAsync().
            //
            // This means EF Core creates the SQL query and only
            // retrieves the data needed by the ViewModel.
            // --------------------------------------------------------

            var results = await query
                .OrderBy(p => p.CheckInTime)
                .Select(p => new ActiveParkingViewModel
                {
                    ParkingSessionId = p.Id,

                    OwnerName =
                        p.Vehicle != null &&
                        p.Vehicle.Owner != null
                            ? p.Vehicle.Owner.FirstName + " " +
                              p.Vehicle.Owner.LastName
                            : "Okänd",

                    VehicleTypeName =
                        p.Vehicle != null &&
                        p.Vehicle.VehicleType != null
                            ? p.Vehicle.VehicleType.Name
                            : "Okänd",

                    RegistrationNumber =
                        p.Vehicle != null
                            ? p.Vehicle.RegistrationNumber
                            : string.Empty,

                    SpotNumber =
                        p.ParkingAllocations
                            .OrderBy(a => a.ParkingSpotId)
                            .Select(a => a.ParkingSpot != null
                                ? a.ParkingSpot.SpotNumber
                                : string.Empty)
                            .FirstOrDefault()
                        ?? string.Empty,

                    Location =
                        p.ParkingAllocations
                            .OrderBy(a => a.ParkingSpotId)
                            .Select(a => a.ParkingSpot != null
                                ? a.ParkingSpot.Location
                                : string.Empty)
                            .FirstOrDefault()
                        ?? string.Empty,

                    CheckInTime = p.CheckInTime
                })
                .ToListAsync();


            // --------------------------------------------------------
            // VEHICLE TYPE FILTER OPTIONS
            // --------------------------------------------------------

            var vehicleTypes = await _context.VehicleTypes
                .AsNoTracking()
                .OrderBy(v => v.Name)
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = v.Name
                })
                .ToListAsync();


            // --------------------------------------------------------
            // BUILD VIEW MODEL
            // --------------------------------------------------------

            var model = new ActiveParkingFilterViewModel
            {
                SearchTerm = searchTerm,
                VehicleTypeId = vehicleTypeId,
                VehicleTypes = vehicleTypes,
                Results = results
            };


            return View(model);
        }
    }
}