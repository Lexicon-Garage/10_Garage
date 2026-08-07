using System.ComponentModel.DataAnnotations;

namespace Garage.Web.Configuration
{
    public class PricingOptions
    {
        public const string SectionName = "Pricing";

        [Range(0.01, 10000, ErrorMessage = "HourlyRate must be greater than 0.")]
        public decimal HourlyRate { get; set; }
    }
}