using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.ViewModels
{
    public class ParkVehicleViewModel
    {
        [Required(ErrorMessage = "Choose a vehicle.")]
        [Range(1, int.MaxValue, ErrorMessage = "Choose a vehicle.")]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Choose a parking spot.")]
        [Range(1, int.MaxValue, ErrorMessage = "Choose a parking spot.")]
        [Display(Name = "Parking spot")]
        public int ParkingSpotId { get; set; }

        [Display(Name = "Filter by location")]
        [StringLength(50)]
        public string? LocationFilter { get; set; }

        // Populated by the controller — not posted by the user
        public IEnumerable<SelectListItem> Vehicles { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> ParkingSpots { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}