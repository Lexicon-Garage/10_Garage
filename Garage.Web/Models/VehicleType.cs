using System.ComponentModel.DataAnnotations;

namespace Garage.Web.Models
{
    public class VehicleType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public required string Name { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
