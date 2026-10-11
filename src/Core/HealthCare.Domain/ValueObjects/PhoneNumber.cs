using HealthCare.Domain.Common;
using HealthCare.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace HealthCare.Domain.ValueObjects
{
    public class PhoneNumber : ValueObject
    {
        private static readonly Regex PhoneRegex = new(
            @"^\+?[1-9]\d{7,14}$",
            RegexOptions.Compiled);
        public string Value { get; }
        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidPhoneNumberException("Phone number cannot be empty.");
            var cleaned = Regex.Replace(value.Trim(), @"[\s\-()]", "");
            if (!PhoneRegex.IsMatch(cleaned))
                throw new InvalidPhoneNumberException($"'{value}' is not a valid international phone number.");
            Value = cleaned;
        }
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
        public override string ToString() => Value;
        public static implicit operator string(PhoneNumber phone) => phone.Value;
    }
    public class InvalidPhoneNumberException : DomainException
    {
        public InvalidPhoneNumberException(string message) : base(message) { }
    }
}