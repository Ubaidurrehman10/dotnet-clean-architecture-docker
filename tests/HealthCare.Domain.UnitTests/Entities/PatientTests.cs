using FluentAssertions;
using HealthCare.Domain.Entities;
using HealthCare.Domain.Events;
using HealthCare.Domain.ValueObjects;
using Xunit;

namespace HealthCare.Domain.UnitTests.Entities;

public class PatientTests
{
    private readonly MedicalRecordNumber _validMrn = new("MRN-2026-000100");
    private readonly PhoneNumber _validPhone = new("+12345678901");
    private readonly Email _validEmail = new("patient@example.com");
    private readonly DateOnly _validDob = new(1990, 5, 15);

    [Fact]
    public void Register_WithValidData_ShouldCreatePatient_AndRaiseEvent()
    {
        // Act
        var patient = Patient.Register(
            _validMrn,
            "John",
            "Doe",
            _validDob,
            _validPhone,
            _validEmail,
            "O+",
            "Jane Doe (+19876543210)");

        // Assert
        patient.Mrn.Should().Be(_validMrn);
        patient.FirstName.Should().Be("John");
        patient.LastName.Should().Be("Doe");
        patient.FullName.Should().Be("John Doe");
        patient.DateOfBirth.Should().Be(_validDob);
        patient.PhoneNumber.Should().Be(_validPhone);
        patient.Email.Should().Be(_validEmail);
        patient.BloodGroup.Should().Be("O+");
        patient.EmergencyContact.Should().Be("Jane Doe (+19876543210)");

        patient.DomainEvents.Should().ContainSingle(e => e is PatientRegisteredEvent);
        var registeredEvent = (PatientRegisteredEvent)patient.DomainEvents.First();
        registeredEvent.PatientId.Should().Be(patient.Id);
        registeredEvent.Mrn.Should().Be(_validMrn);
        registeredEvent.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void Register_WithFutureDateOfBirth_ShouldThrowArgumentException()
    {
        // Arrange
        var futureDob = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        var act = () => Patient.Register(
            _validMrn,
            "Baby",
            "Doe",
            futureDob,
            _validPhone);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be in the future*");
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData("   ", "Doe")]
    [InlineData("John", "")]
    [InlineData("John", "   ")]
    public void Register_WithInvalidNames_ShouldThrowArgumentException(string firstName, string lastName)
    {
        // Act
        var act = () => Patient.Register(
            _validMrn,
            firstName,
            lastName,
            _validDob,
            _validPhone);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateContactInfo_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var patient = Patient.Register(
            _validMrn,
            "John",
            "Doe",
            _validDob,
            _validPhone,
            _validEmail);

        var newPhone = new PhoneNumber("+19998887777");
        var newEmail = new Email("newemail@example.com");

        // Act
        patient.UpdateContactInfo(newPhone, newEmail, "New Emergency Contact");

        // Assert
        patient.PhoneNumber.Should().Be(newPhone);
        patient.Email.Should().Be(newEmail);
        patient.EmergencyContact.Should().Be("New Emergency Contact");
    }
}
