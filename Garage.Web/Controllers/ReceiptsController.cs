using Garage.Web.Services;
using Garage.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Garage.Web.Controllers
{
    public class ReceiptsController : Controller
    {
        private readonly IFileHandler<Stream, ReceiptViewModel> _iFileHandler;
        public ReceiptsController(IFileHandler <Stream, ReceiptViewModel> IFileHandler)
        {
            _iFileHandler = IFileHandler;
        }
        public IActionResult Index()
        {
            var json = TempData.Peek("Receipt") as string;

            if (json == null)
            {
                return RedirectToAction("Index", "Vehicles");
            }

            var receiptViewModel = JsonSerializer.Deserialize<ReceiptViewModel>(json);

            return View(receiptViewModel);
        }
        public async Task<IActionResult> ShowPDF()
        {
            var json = TempData.Peek("Receipt") as string;

            if (json == null)
            {
                return RedirectToAction("Index", "Vehicles");
            }

            var receipt = JsonSerializer.Deserialize<ReceiptViewModel>(json);

            var stream = new MemoryStream();

            await _iFileHandler.WriteAsync(stream, receipt!);

            stream.Position = 0;

            return File(stream, "application/pdf");
        }
    }
}
