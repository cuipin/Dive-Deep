using System.ComponentModel.DataAnnotations;

namespace Dive_Deep.Models.Validations
{
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime > DateTime.Now;
            }

            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} cannot be in the past";
        }
    }
}