namespace Garage.Web.ViewModels.Admin
{
    public class VehicleTypeListViewModel
    {
        public int Id { get; set; }
        public List<string> VehicleTypeNames { get; set; } = new List<string>();
    }
}
