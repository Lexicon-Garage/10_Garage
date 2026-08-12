using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
namespace Garage.Web.Areas.Identity.Validation
{
    public class SwedishPersonalNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext)
        {
            if (value is not string personalNumber ||
                string.IsNullOrWhiteSpace(personalNumber))
            {
                return new ValidationResult(
                    ErrorMessage ?? "Enter a valid Swedish personal number.");
            }

            personalNumber = personalNumber.Trim();

            // Accept:
            // YYMMDD-XXXX
            // YYYYMMDD-XXXX
            if (!Regex.IsMatch(personalNumber, @"^(\d{6}|\d{8})-\d{4}$"))
            {
                return new ValidationResult(
                    ErrorMessage ?? "Enter a valid Swedish personal number.");
            }

            var number = personalNumber.Replace("-", "");

            // Convert YYYYMMDDXXXX -> YYMMDDXXXX
            if (number.Length == 12)
            {
                number = number.Substring(2);
            }

            // Must be exactly 10 digits
            if (!Regex.IsMatch(number, @"^\d{10}$"))
            {
                return new ValidationResult(
                    ErrorMessage ?? "Enter a valid Swedish personal number.");
            }

            // YYMMDD
            var year = int.Parse(number.Substring(0, 2));
            var month = int.Parse(number.Substring(2, 2));
            var day = int.Parse(number.Substring(4, 2));

            // Validate month
            if (month < 1 || month > 12)
            {
                return new ValidationResult(
                    ErrorMessage ?? "Enter a valid Swedish personal number.");
            }

            // Validate day using a leap-year-independent approach.
            // For YY format, the century is unknown, so use a leap-year-safe
            // check that permits Feb 29. The actual century cannot be determined
            // from a 10-digit personnummer alone.
            if (day < 1 || day > DateTime.DaysInMonth(2000, month))
            {
                return new ValidationResult(
                    ErrorMessage ?? "Enter a valid Swedish personal number.");
            }

            // Luhn checksum.
            var sum = 0;

            for (var i = 0; i < 9; i++)
            {
                var digit = number[i] - '0';

                if (i % 2 == 0)
                {
                    digit *= 2;

                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                sum += digit;
            }

            var calculatedCheckDigit = (10 - (sum % 10)) % 10;
            var actualCheckDigit = number[9] - '0';

            if (calculatedCheckDigit != actualCheckDigit)
            {
                return new ValidationResult(
                    ErrorMessage ?? "Enter a valid Swedish personal number.");
            }

            return ValidationResult.Success;
        }
    }

}

