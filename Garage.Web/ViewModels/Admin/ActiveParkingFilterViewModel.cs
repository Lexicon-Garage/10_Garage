using Microsoft.AspNetCore.Mvc.Rendering;

namespace Garage.Web.ViewModels.Admin
{
    public class ActiveParkingFilterViewModel
    {
        public string? SearchTerm { get; set; }

        public int? VehicleTypeId { get; set; }

        public IEnumerable<SelectListItem> VehicleTypes { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<ActiveParkingViewModel> Results { get; set; }
            = new List<ActiveParkingViewModel>();
    }
}
