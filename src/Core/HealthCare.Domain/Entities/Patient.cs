using HealthCare.Domain.Common;
using HealthCare.Domain.Events;
using HealthCare.Domain.ValueObjects;

namespace HealthCare.Domain.Entities;

public class Patient : AggregateRoot
{
    public MedicalRecordNumber Mrn { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public DateOnly DateOfBirth { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; } = default!;

    // Optional and NOT globally unique (family members can share email)
    public Email? Email { get; private set; }
    public string? BloodGroup { get; private set; }
    public string? EmergencyContact { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    // Private constructor for EF Core
    private Patient() { }

    public static Patient Register(
        MedicalRecordNumber mrn,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        PhoneNumber phoneNumber,
        Email? email = null,
        string? bloodGroup = null,
        string? emergencyContact = null)
    {
        ArgumentNullException.ThrowIfNull(mrn);

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));

        ArgumentNullException.ThrowIfNull(phoneNumber);

        var patient = new Patient
        {
            Mrn = mrn,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            DateOfBirth = dateOfBirth,
            PhoneNumber = phoneNumber,
            Email = email,
            BloodGroup = bloodGroup?.Trim(),
            EmergencyContact = emergencyContact?.Trim()
        };

        patient.RaiseDomainEvent(new PatientRegisteredEvent(
            patient.Id,
            patient.Mrn,
            patient.FullName,
            patient.PhoneNumber.Value,
            patient.Email?.Value));

        return patient;
    }

    public void UpdateContactInfo(PhoneNumber phoneNumber, Email? email, string? emergencyContact = null)
    {
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Email = email;
        EmergencyContact = emergencyContact?.Trim();
    }
}