using QuestPDF.Fluent;
using Garage.Web.ViewModels;


namespace Garage.Web.Services
{
    public class PdfFileHandler : IFileHandler<Stream, ReceiptViewModel>
    {
        //In case the requirement to read from a local file and extract data
        public Task<ReceiptViewModel> ReadAsync(
        Stream file,
        CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("Reading PDF files is not supported.");
        }
        public Task WriteAsync(
            Stream stream,
            ReceiptViewModel invoice,
            CancellationToken cancellationToken = default)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .AlignCenter()
                        .Text("PARKING RECEIPT")
                        .FontSize(26)
                        .Bold();

                    page.Content().Column(column =>
                    {
                        column.Spacing(12);

                        column.Item().PaddingTop(10).LineHorizontal(1);

                        column.Item().Text($"License Plate: {invoice.RegistrationNumber}")
                            .FontSize(14);
                        // I have to handle vehicletype as a string 
                        column.Item().Text($"Vehicle Type: {invoice.VehicleType}")
                            .FontSize(14);

                        column.Item().PaddingTop(10);

                        column.Item().Text($"Check-in Time: {invoice.CheckInTime:yyyy-MM-dd HH:mm}")
                            .FontSize(12);

                        column.Item().Text($"Check-out Time: {invoice.CheckOutTime:yyyy-MM-dd HH:mm}")
                            .FontSize(12);

                        column.Item().Text($"Parking Duration: {invoice.ParkingDuration}")
                            .FontSize(12);

                        column.Item().PaddingTop(10).LineHorizontal(1);

                        column.Item().Text($"TOTAL PRICE: {invoice.Price:C}")
                            .FontSize(18)
                            .Bold()
                            .FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

                        column.Item().PaddingTop(15);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Thank you for choosing our parking service!")
                        .FontSize(10)
                        .Italic();
                });
            })
            .GeneratePdf(stream);

            return Task.CompletedTask;
        }
    }
}
