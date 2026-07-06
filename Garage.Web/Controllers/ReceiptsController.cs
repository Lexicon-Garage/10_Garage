using Garage.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Garage.Web.Controllers
{
    public class ReceiptsController : Controller
    {
        public IActionResult Index()
        {
            var json = TempData["Receipt"] as string;

            if (json == null)
            {
                return RedirectToAction("Index", "ParkedVehicles");
            }

            var receipt = JsonSerializer.Deserialize<ReceiptViewModel>(json);

            return View(receipt);
        }
        public async Task<IActionResult> ShowPDF()
        {
            // I have a custom implementation for generating PDF from the receipt view, but I will leave it empty for now.
            return View("");
        }
    }
}
