using System.ComponentModel.DataAnnotations;
using Garage.Web.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Garage.Test;

public class PricingOptionsTests
{
    private static IConfiguration BuildConfig(params (string Key, string? Value)[] settings) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(settings.ToDictionary(s => s.Key, s => s.Value))
            .Build();

    private static PricingOptions Bind(IConfiguration config)
    {
        var options = new PricingOptions();
        config.GetSection(PricingOptions.SectionName).Bind(options);
        return options;
    }

    private static bool IsValid(PricingOptions options) =>
        Validator.TryValidateObject(options, new ValidationContext(options),
            new List<ValidationResult>(), validateAllProperties: true);

    // ---------- binding ----------

    [Fact]
    public void Bind_ReadsDefaultRate()
    {
        var options = Bind(BuildConfig(("Pricing:DefaultHourlyRate", "20")));
        Assert.Equal(20m, options.DefaultHourlyRate);
    }

    [Fact]
    public void Bind_ReadsPerTypeOverrides()
    {
        var options = Bind(BuildConfig(
            ("Pricing:DefaultHourlyRate", "20"),
            ("Pricing:HourlyRates:Bus", "40")));

        Assert.Equal(40m, options.RateFor("Bus"));
        Assert.Equal(20m, options.RateFor("Car"));
    }

    [Fact]
    public void Bind_SupportsDecimalValues()
    {
        var options = Bind(BuildConfig(("Pricing:DefaultHourlyRate", "20.50")));
        Assert.Equal(20.50m, options.DefaultHourlyRate);
    }

    // ---------- rate resolution ----------

    [Fact]
    public void RateFor_KnownType_ReturnsOverride()
    {
        var options = new PricingOptions
        {
            DefaultHourlyRate = 20,
            HourlyRates = { ["Bus"] = 40 }
        };

        Assert.Equal(40m, options.RateFor("Bus"));
    }

    [Theory]
    [InlineData("Motorcycle")]
    [InlineData("Unknown")]
    [InlineData(null)]
    public void RateFor_UnknownOrMissingType_FallsBackToDefault(string? type)
    {
        var options = new PricingOptions { DefaultHourlyRate = 20 };
        Assert.Equal(20m, options.RateFor(type));
    }

    // ---------- validation ----------

    [Theory]
    [InlineData(0.01)]
    [InlineData(20)]
    public void Validation_AcceptsPositiveDefaultRate(decimal rate)
        => Assert.True(IsValid(new PricingOptions { DefaultHourlyRate = rate }));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validation_RejectsNonPositiveDefaultRate(decimal rate)
        => Assert.False(IsValid(new PricingOptions { DefaultHourlyRate = rate }));

    [Fact]
    public void Validation_RejectsNonPositiveOverride()
    {
        var options = new PricingOptions
        {
            DefaultHourlyRate = 20,
            HourlyRates = { ["Car"] = 0 }
        };

        Assert.False(IsValid(options));
    }

    // ---------- DI wiring ----------

    [Fact]
    public void DI_ValidConfiguration_ResolvesOptions()
    {
        var services = new ServiceCollection();
        services.AddOptions<PricingOptions>()
                .Bind(BuildConfig(("Pricing:DefaultHourlyRate", "20"))
                        .GetSection(PricingOptions.SectionName))
                .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();

        Assert.Equal(20m, provider.GetRequiredService<IOptions<PricingOptions>>().Value.DefaultHourlyRate);
    }

    [Fact]
    public void DI_InvalidConfiguration_ThrowsOnResolve()
    {
        var services = new ServiceCollection();
        services.AddOptions<PricingOptions>()
                .Bind(BuildConfig(("Pricing:DefaultHourlyRate", "0"))
                        .GetSection(PricingOptions.SectionName))
                .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<PricingOptions>>().Value);
    }
    [Fact]
    public void CalculatePrice_WithExplicitRate_IgnoresConfiguredRates()
    {
        var options = new PricingOptions { DefaultHourlyRate = 20, HourlyRates = { ["Bus"] = 40 } };

        // 90 min → 2 started hours × 15 = 30
        Assert.Equal(30m, options.CalculatePrice(TimeSpan.FromMinutes(90), 15m));
    }
}