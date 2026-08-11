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
                return new ValidationResult("Personal number is required.");
            }

            var number = Regex.Replace(personalNumber, @"[\s+-]", "");

            if (number.Length != 10 && number.Length != 12)
            {
                return new ValidationResult(
                    "Enter a valid Swedish personal number.");
            }

            if (number.Length == 12)
            {
                number = number.Substring(2);
            }

            if (!Regex.IsMatch(number, @"^\d{10}$"))
            {
                return new ValidationResult(
                    "Enter a valid Swedish personal number.");
            }

            var month = int.Parse(number.Substring(2, 2));
            var day = int.Parse(number.Substring(4, 2));

            if (month < 1 || month > 12)
            {
                return new ValidationResult(
                    "Enter a valid Swedish personal number.");
            }

            try
            {
                _ = new DateTime(2000, month, day);
            }
            catch
            {
                return new ValidationResult(
                    "Enter a valid Swedish personal number.");
            }

            var digits = number.Substring(0, 9);
            var checkDigit = int.Parse(number[9].ToString());

            var sum = 0;

            for (var i = 0; i < digits.Length; i++)
            {
                var digit = int.Parse(digits[i].ToString());

                if (i % 2 == 0)
                {
                    digit *= 2;

                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
            }

            var calculatedCheckDigit = (10 - (sum % 10)) % 10;

            if (calculatedCheckDigit != checkDigit)
            {
                return new ValidationResult(
                    "Enter a valid Swedish personal number.");
            }

            return ValidationResult.Success;
        }
    }
}
