using FluentAssertions;
using HealthCare.Application.Common.Enums;
using HealthCare.Application.Common.Models;

namespace HealthCare.Application.UnitTests.Common
{
    public class ResultTests
    {
        [Fact]
        public void Success_ShouldCreateSuccessfulResult_WithNoError()
        {
            // Act
            var result = Result.Success();
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Error.Should().Be(Error.None);
        }
        [Fact]
        public void SuccessWithValue_ShouldHoldValue()
        {
            // Arrange
            var testGuid = Guid.NewGuid();
            // Act
            var result = Result.Success(testGuid);
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(testGuid);
            result.Error.Should().Be(Error.None);
        }
        [Fact]
        public void Failure_ShouldHoldError_AndIsFailureBeTrue()
        {
            // Arrange
            var error = Error.NotFound("Patient.NotFound", "Patient was not found.");
            // Act
            var result = Result.Failure<string>(error);
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(error);
            result.Error.Type.Should().Be(ErrorType.NotFound);
        }
        [Fact]
        public void AccessingValue_OnFailureResult_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var result = Result.Failure<string>(Error.Conflict("Conflict.Code", "Conflict description"));
            // Act
            var act = () => result.Value;
            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Cannot access the value of a failed result*");
        }
        [Fact]
        public void FailureWithValidationErrors_ShouldPopulateValidationList()
        {
            // Arrange
            var validationErrors = new[] { "Slot is required.", "DoctorId cannot be empty." };
            // Act
            var result = Result.Failure<Guid>(validationErrors);
            // Assert
            result.IsFailure.Should().BeTrue();
            result.ValidationErrors.Should().BeEquivalentTo(validationErrors);
            result.Error.Type.Should().Be(ErrorType.Validation);
        }
    }
}