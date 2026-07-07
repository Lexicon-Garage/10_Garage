using Garage.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class ParkedVehicleDetailsViewModel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(60, MinimumLength = 5)]
    [Display(Name = "Registration Number")]
    public string RegistrationNumber { get; set; } = string.Empty;
    
    [EnumDataType(typeof(VehicleType))]
    [Display(Name = "Vehicle Type")]
    public VehicleType VehicleType { get; set; }
    
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
    
    [EnumDataType(typeof(BrandType))]
    [Display(Name = "Brand")]
    public BrandType BrandType { get; set; }
    
    [Display(Name = "Arrived at")]
    public DateTime ArrivedTime { get; set; }
    
}