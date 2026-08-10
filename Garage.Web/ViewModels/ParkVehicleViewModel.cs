using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Garage.Web.ViewModels;

public class ParkVehicleViewModel
{
    [Required(ErrorMessage = "Choose a vehicle.")]
    [Display(Name = "Vehicle")]
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "Choose a parking spot.")]
    [Display(Name = "Parking spot")]
    public int ParkingSpotId { get; set; }

    [Display(Name = "Filter by location")]
    [StringLength(50)]
    public string? LocationFilter { get; set; }

    // Populated by the controller (tasks 06.2 and 06.3)
    public IEnumerable<SelectListItem> Vehicles { get; set; } = [];
    public IEnumerable<SelectListItem> ParkingSpots { get; set; } = [];
}
