using Garage.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class ParkedVehicleDetailsViewModel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(60, MinimumLength = 5)]
    public string RegistrationNumber { get; set; } = string.Empty;
    
    [EnumDataType(typeof(VehicleType))]
    public VehicleType VehicleType { get; set; }
    
    [Required]
    [StringLength(20, MinimumLength = 2)]
    public string? Color { get; set; }
    
    [Range(1, 16)]
    public int NumberOfWheels { get; set; }
    
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string? Model { get; set; }
    
    [EnumDataType(typeof(BrandType))]
    public BrandType BrandType { get; set; }
    
    public DateTime ArrivedTime { get; set; }
    
}