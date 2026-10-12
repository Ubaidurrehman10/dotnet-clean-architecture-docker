using FluentAssertions;
using HealthCare.Domain.Exceptions;
using HealthCare.Domain.ValueObjects;
using Xunit;

namespace HealthCare.Domain.UnitTests.ValueObjects;

public class MedicalRecordNumberTests
{
    [Theory]
    [InlineData("MRN-2026-000001")]
    [InlineData("mrn-2026-000001")]
    [InlineData("HOSP-98765")]
    public void Constructor_WithValidValue_ShouldCreateAndUppercase(string input)
    {
        // Act
        var mrn = new MedicalRecordNumber(input);

        // Assert
        mrn.Value.Should().Be(input.Trim().ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrNullValue_ShouldThrowInvalidMedicalRecordNumberException(string? input)
    {
        // Act
        var act = () => new MedicalRecordNumber(input!);

        // Assert
        act.Should().Throw<InvalidMedicalRecordNumberException>()
            .WithMessage("*cannot be empty*");
    }

    [Theory]
    [InlineData("M1")]
    [InlineData("1234")]
    public void Constructor_WithTooShortValue_ShouldThrowInvalidMedicalRecordNumberException(string input)
    {
        // Act
        var act = () => new MedicalRecordNumber(input);

        // Assert
        act.Should().Throw<InvalidMedicalRecordNumberException>()
            .WithMessage("*between 5 and 30 characters*");
    }

    [Fact]
    public void Generate_ShouldCreateFormattedMrn()
    {
        // Act
        var mrn = MedicalRecordNumber.Generate(2026, 42);

        // Assert
        mrn.Value.Should().Be("MRN-2026-000042");
    }

    [Fact]
    public void Equality_WhenValuesMatchCaseInsensitively_ShouldBeEqual()
    {
        // Arrange
        var mrn1 = new MedicalRecordNumber("MRN-2026-000001");
        var mrn2 = new MedicalRecordNumber("mrn-2026-000001");

        // Act & Assert
        mrn1.Should().Be(mrn2);
        (mrn1 == mrn2).Should().BeTrue();
    }
}
