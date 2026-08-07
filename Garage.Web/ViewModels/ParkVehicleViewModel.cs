using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
public class ParkVehicleViewModel
{
    [Required] public int VehicleId { get; set; }
    [Required] public int ParkingSpotId { get; set; }
    public string? LocationFilter { get; set; }

    public IEnumerable<SelectListItem> Vehicles { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> ParkingSpots { get; set; } = Array.Empty<SelectListItem>();
}