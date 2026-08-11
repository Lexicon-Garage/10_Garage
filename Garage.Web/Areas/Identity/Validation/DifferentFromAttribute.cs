using System.ComponentModel.DataAnnotations;

namespace Garage.Web.Areas.Identity.Validation
{
    public class DifferentFromAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;

        public DifferentFromAttribute(string otherProperty)
        {
            _otherProperty = otherProperty;
        }

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext)
        {
            var otherProperty = validationContext.ObjectType
                .GetProperty(_otherProperty);

            if (otherProperty == null)
            {
                return new ValidationResult(
                    $"Property {_otherProperty} was not found.");
            }

            var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

            if (value is string currentValue &&
                otherValue is string otherValueString &&
                string.Equals(
                    currentValue.Trim(),
                    otherValueString.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
