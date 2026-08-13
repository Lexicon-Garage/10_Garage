using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.ViewModels.Admin
{
    public class ParkingSpotEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string SpotNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Location { get; set; } = string.Empty;

        public bool IsOutOfService { get; set; }

        [Required]
        public int VehicleTypeId { get; set; }
    }
}
