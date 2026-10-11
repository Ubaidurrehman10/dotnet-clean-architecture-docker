using HealthCare.Domain.Common;
using HealthCare.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace HealthCare.Domain.ValueObjects
{
    public class Email : ValueObject
    {
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public string Value { get; }
        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidEmailException("Email address cannot be empty.");
            var trimmed = value.Trim().ToLowerInvariant();
            if (trimmed.Length > 256)
                throw new InvalidEmailException("Email address cannot exceed 256 characters.");
            if (!EmailRegex.IsMatch(trimmed))
                throw new InvalidEmailException($"'{value}' is not a valid email address.");
            Value = trimmed;
        }
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
        public override string ToString() => Value;
        public static implicit operator string(Email email) => email.Value;
    }
    public class InvalidEmailException : DomainException
    {
        public InvalidEmailException(string message) : base(message) { }
    }
}