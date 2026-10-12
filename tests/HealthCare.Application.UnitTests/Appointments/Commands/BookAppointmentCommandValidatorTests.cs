using FluentValidation.TestHelper;
using HealthCare.Application.Appointments.Commands.BookAppointment;
using HealthCare.Domain.Enums;

namespace HealthCare.Application.UnitTests.Appointments.Commands
{
    public class BookAppointmentCommandValidatorTests
    {
        private readonly BookAppointmentCommandValidator _validator = new();
        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveAnyErrors()
        {
            // Arrange
            var command = new BookAppointmentCommand(
                DoctorId: Guid.NewGuid(),
                PatientId: Guid.NewGuid(),
                StartUtc: DateTime.UtcNow.AddDays(2),
                EndUtc: DateTime.UtcNow.AddDays(2).AddMinutes(30),
                Source: BookingSource.WalkIn,
                BookedByUserId: Guid.NewGuid());
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
        [Fact]
        public void Validate_WhenRequiredGuidsAreEmpty_ShouldHaveValidationErrors()
        {
            // Arrange
            var command = new BookAppointmentCommand(
                DoctorId: Guid.Empty,
                PatientId: Guid.Empty,
                StartUtc: DateTime.UtcNow.AddDays(1),
                EndUtc: DateTime.UtcNow.AddDays(1).AddMinutes(30),
                Source: BookingSource.SelfBooking,
                BookedByUserId: Guid.Empty);
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DoctorId);
            result.ShouldHaveValidationErrorFor(x => x.PatientId);
            result.ShouldHaveValidationErrorFor(x => x.BookedByUserId);
        }
        [Fact]
        public void Validate_WhenStartIsInThePast_ShouldHaveValidationError()
        {
            // Arrange
            var command = new BookAppointmentCommand(
                DoctorId: Guid.NewGuid(),
                PatientId: Guid.NewGuid(),
                StartUtc: DateTime.UtcNow.AddHours(-1),
                EndUtc: DateTime.UtcNow.AddMinutes(30),
                Source: BookingSource.SelfBooking,
                BookedByUserId: Guid.NewGuid());
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StartUtc);
        }
        [Fact]
        public void Validate_WhenEndIsBeforeStart_ShouldHaveValidationError()
        {
            // Arrange
            var command = new BookAppointmentCommand(
                DoctorId: Guid.NewGuid(),
                PatientId: Guid.NewGuid(),
                StartUtc: DateTime.UtcNow.AddDays(1),
                EndUtc: DateTime.UtcNow.AddDays(1).AddMinutes(-30),
                Source: BookingSource.SelfBooking,
                BookedByUserId: Guid.NewGuid());
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EndUtc);
        }
        [Fact]
        public void Validate_WhenDurationIsLessThan15Minutes_ShouldHaveValidationError()
        {
            // Arrange
            var command = new BookAppointmentCommand(
                DoctorId: Guid.NewGuid(),
                PatientId: Guid.NewGuid(),
                StartUtc: DateTime.UtcNow.AddDays(1),
                EndUtc: DateTime.UtcNow.AddDays(1).AddMinutes(10),
                Source: BookingSource.SelfBooking,
                BookedByUserId: Guid.NewGuid());
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x);
        }
        [Fact]
        public void Validate_WhenDurationExceedsTwoHours_ShouldHaveValidationError()
        {
            // Arrange
            var command = new BookAppointmentCommand(
                DoctorId: Guid.NewGuid(),
                PatientId: Guid.NewGuid(),
                StartUtc: DateTime.UtcNow.AddDays(1),
                EndUtc: DateTime.UtcNow.AddDays(1).AddHours(2).AddMinutes(15),
                Source: BookingSource.SelfBooking,
                BookedByUserId: Guid.NewGuid());
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x);
        }
        [Fact]
        public void Validate_WhenSourceIsInvalidEnum_ShouldHaveValidationError()
        {
            // Arrange
            var command = new BookAppointmentCommand(
                DoctorId: Guid.NewGuid(),
                PatientId: Guid.NewGuid(),
                StartUtc: DateTime.UtcNow.AddDays(1),
                EndUtc: DateTime.UtcNow.AddDays(1).AddMinutes(30),
                Source: (BookingSource)999,
                BookedByUserId: Guid.NewGuid());
            // Act
            var result = _validator.TestValidate(command);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Source);
        }
    }
}