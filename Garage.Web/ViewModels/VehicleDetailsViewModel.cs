using Garage.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.ViewModels
{
    public class VehicleDetailsViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 5)]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 2)]
        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Range(1, 16)]
        [Display(Name = "Number of wheels")]
        public int NumberOfWheels { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        [Display(Name = "Model")]
        public string? Model { get; set; }

        [StringLength(50, MinimumLength = 1)]
        [Display(Name = "Brand")]
        public string Brand { get; set; } = string.Empty;

    }

}