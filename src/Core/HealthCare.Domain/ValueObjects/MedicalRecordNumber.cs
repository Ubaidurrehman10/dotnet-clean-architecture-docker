using HealthCare.Domain.Common;
using HealthCare.Domain.Exceptions;

namespace HealthCare.Domain.ValueObjects;

public class MedicalRecordNumber : ValueObject
{
    public string Value { get; }

    public MedicalRecordNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidMedicalRecordNumberException("Medical Record Number (MRN) cannot be empty.");

        var trimmed = value.Trim().ToUpperInvariant();

        if (trimmed.Length < 5 || trimmed.Length > 30)
            throw new InvalidMedicalRecordNumberException("Medical Record Number must be between 5 and 30 characters.");

        Value = trimmed;
    }

    public static MedicalRecordNumber Generate(int year, int sequenceNumber)
    {
        return new MedicalRecordNumber($"MRN-{year}-{sequenceNumber:D6}");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(MedicalRecordNumber mrn) => mrn.Value;
}

public class InvalidMedicalRecordNumberException : DomainException
{
    public InvalidMedicalRecordNumberException(string message) : base(message) { }
}
