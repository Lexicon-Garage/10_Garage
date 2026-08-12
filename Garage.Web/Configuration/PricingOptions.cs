using System.ComponentModel.DataAnnotations;

namespace Garage.Web.Configuration;

public class PricingOptions : IValidatableObject
{
    public const string SectionName = "Pricing";

    [Range(0.01, 10000)]
    public decimal DefaultHourlyRate { get; set; }

    public Dictionary<string, decimal> HourlyRates { get; set; } = new();

    public decimal RateFor(string? vehicleTypeName) =>
        vehicleTypeName is not null && HourlyRates.TryGetValue(vehicleTypeName, out var rate)
            ? rate
            : DefaultHourlyRate;

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        foreach (var (type, rate) in HourlyRates)
            if (rate <= 0)
                yield return new ValidationResult(
                    $"Hourly rate for '{type}' must be greater than 0.",
                    new[] { nameof(HourlyRates) });
    }

    public decimal CalculatePrice(TimeSpan duration, decimal hourlyRate)
    {
        if (duration <= TimeSpan.Zero) return 0m;
        var startedHours = (int)Math.Ceiling(duration.TotalHours);
        return startedHours * hourlyRate;
    }
}