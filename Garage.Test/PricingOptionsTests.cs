using System.ComponentModel.DataAnnotations;
using Garage.Web.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Garage.Test;

public class PricingOptionsTests
{
    // ---------- helpers ----------

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

    private static bool IsValid(PricingOptions options, out List<ValidationResult> errors)
    {
        errors = new List<ValidationResult>();
        return Validator.TryValidateObject(
            options, new ValidationContext(options), errors, validateAllProperties: true);
    }

    // ---------- binding ----------

    [Fact]
    public void Bind_ReadsHourlyRateFromConfiguration()
    {
        var options = Bind(BuildConfig(("Pricing:HourlyRate", "20")));

        Assert.Equal(20m, options.HourlyRate);
    }

    [Fact]
    public void Bind_SupportsDecimalValues()
    {
        var options = Bind(BuildConfig(("Pricing:HourlyRate", "20.50")));

        Assert.Equal(20.50m, options.HourlyRate);
    }

    [Fact]
    public void Bind_MissingSection_LeavesRateAtZero()
    {
        var options = Bind(BuildConfig(("SomethingElse:Value", "1")));

        Assert.Equal(0m, options.HourlyRate);   // and therefore invalid — see below
    }

    [Fact]
    public void Bind_EnvironmentStyleKey_Overrides_AppSettings()
    {
        // Azure App Service uses Pricing__HourlyRate; ':' and '__' are equivalent
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Pricing:HourlyRate"] = "20"
            })
            .AddEnvironmentVariables()   // real env vars would win here
            .Build();

        var options = Bind(config);

        Assert.Equal(20m, options.HourlyRate);
    }

    // ---------- validation ----------

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(999.99)]
    public void Validation_AcceptsPositiveRates(decimal rate)
    {
        var options = new PricingOptions { HourlyRate = rate };

        Assert.True(IsValid(options, out _));
    }

    [Theory]
    [InlineData(0)]        // missing config binds to 0 — must not silently mean "free"
    [InlineData(-1)]
    [InlineData(-20.5)]
    public void Validation_RejectsZeroOrNegativeRates(decimal rate)
    {
        var options = new PricingOptions { HourlyRate = rate };

        Assert.False(IsValid(options, out var errors));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(PricingOptions.HourlyRate)));
    }

    // ---------- DI wiring (what Program.cs actually does) ----------

    [Fact]
    public void DI_ValidConfiguration_ResolvesOptions()
    {
        var services = new ServiceCollection();
        services.AddOptions<PricingOptions>()
                .Bind(BuildConfig(("Pricing:HourlyRate", "20"))
                        .GetSection(PricingOptions.SectionName))
                .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PricingOptions>>().Value;

        Assert.Equal(20m, options.HourlyRate);
    }

    [Fact]
    public void DI_InvalidConfiguration_ThrowsOnResolve()
    {
        var services = new ServiceCollection();
        services.AddOptions<PricingOptions>()
                .Bind(BuildConfig(("Pricing:HourlyRate", "0"))
                        .GetSection(PricingOptions.SectionName))
                .ValidateDataAnnotations();

        var provider = services.BuildServiceProvider();

        var ex = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<PricingOptions>>().Value);

        Assert.Contains("HourlyRate", ex.Message);
    }
}