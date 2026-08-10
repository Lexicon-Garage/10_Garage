using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Garage.Web.Models
{
    [Index(nameof(RegistrationNumber), IsUnique = true)]
    public class Vehicle
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public required string RegistrationNumber { get; set; }

        [Required]
        [StringLength(50)]
        public required string Brand { get; set; }

        [Required]
        [StringLength(50)]
        public required string Model { get; set; }

        [Required]
        [StringLength(20)]
        public required string Color { get; set; }

        [Range(1, 20)]
        public int NumberOfWheels { get; set; }

        [Required]
        public int VehicleTypeId { get; set; }

        public VehicleType VehicleType { get; set; } = default!;

       [Required]
        public string OwnerId { get; set; } = default!;

        public IdentityUser Owner { get; set; } = default!;

        //public ICollection<ParkingSession> ParkingSessions { get; set; }
        //    = new List<ParkingSession>();
    }
}
