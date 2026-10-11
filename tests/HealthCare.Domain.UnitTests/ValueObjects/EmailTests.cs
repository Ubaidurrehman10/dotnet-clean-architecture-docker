using FluentAssertions;
using HealthCare.Domain.ValueObjects;

namespace HealthCare.Domain.UnitTests.ValueObjects
{
    public class EmailTests
    {
        [Theory]
        [InlineData("patient@example.com")]
        [InlineData("JOHN.DOE@CLINIC.ORG")]
        [InlineData("user+filter@domain.co.uk")]
        public void Constructor_WithValidEmail_ShouldCreateEmailAndNormalizeToLowercase(string input)
        {
            // Act
            var email = new Email(input);
            // Assert
            email.Value.Should().Be(input.Trim().ToLowerInvariant());
        }
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("invalid-email")]
        [InlineData("@missingusername.com")]
        [InlineData("missingdomain@")]
        public void Constructor_WithInvalidEmail_ShouldThrowInvalidEmailException(string input)
        {
            // Act
            var act = () => new Email(input);
            // Assert
            act.Should().Throw<InvalidEmailException>();
        }
        [Fact]
        public void Equality_WhenEmailsMatchCaseInsensitively_ShouldBeEqual()
        {
            // Arrange
            var email1 = new Email("dr.smith@hospital.com");
            var email2 = new Email("DR.SMITH@HOSPITAL.COM");
            // Act & Assert
            email1.Should().Be(email2);
            (email1 == email2).Should().BeTrue();
        }
    }
}