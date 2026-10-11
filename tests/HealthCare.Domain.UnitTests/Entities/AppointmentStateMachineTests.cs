using FluentAssertions;
using HealthCare.Domain.Entities;
using HealthCare.Domain.Enums;
using HealthCare.Domain.Events;
using HealthCare.Domain.Exceptions;
using HealthCare.Domain.ValueObjects;
using Xunit;

namespace HealthCare.Domain.UnitTests.Entities
{
    public class AppointmentStateMachineTests
    {
        private readonly Guid _doctorId = Guid.NewGuid();
        private readonly Guid _patientId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();
        private Appointment CreateValidScheduledAppointment()
        {
            var futureSlot = new AppointmentTimeSlot(
                DateTime.UtcNow.AddDays(2).Date.AddHours(10),
                DateTime.UtcNow.AddDays(2).Date.AddHours(10).AddMinutes(30));
            return Appointment.Book(
                _doctorId,
                _patientId,
                futureSlot,
                BookingSource.SelfBooking,
                _userId);
        }
        [Fact]
        public void Book_WithFutureSlot_ShouldBeInScheduledState_AndRaiseAppointmentBookedEvent()
        {
            // Act
            var appointment = CreateValidScheduledAppointment();
            // Assert
            appointment.Status.Should().Be(AppointmentStatus.Scheduled);
            appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentBookedEvent);
        }
        [Fact]
        public void Book_WithPastSlot_ShouldThrowInvalidTimeSlotException()
        {
            // Arrange
            var pastSlot = new AppointmentTimeSlot(
                DateTime.UtcNow.AddDays(-1).Date.AddHours(10),
                DateTime.UtcNow.AddDays(-1).Date.AddHours(10).AddMinutes(30));
            // Act
            var act = () => Appointment.Book(
                _doctorId,
                _patientId,
                pastSlot,
                BookingSource.SelfBooking,
                _userId);
            // Assert
            act.Should().Throw<InvalidTimeSlotException>()
                .WithMessage("*in the past*");
        }
        [Fact]
        public void HappyPath_FullLifecycle_ShouldTransitionSuccessfully()
        {
            // Arrange
            var appointment = CreateValidScheduledAppointment();
            // 1. Scheduled -> Confirmed
            appointment.Confirm();
            appointment.Status.Should().Be(AppointmentStatus.Confirmed);
            appointment.ConfirmedAtUtc.Should().NotBeNull();
            appointment.DomainEvents.Should().Contain(e => e is AppointmentConfirmedEvent);
            // 2. Confirmed -> CheckedIn
            appointment.CheckIn();
            appointment.Status.Should().Be(AppointmentStatus.CheckedIn);
            appointment.CheckedInAtUtc.Should().NotBeNull();
            appointment.DomainEvents.Should().Contain(e => e is AppointmentCheckedInEvent);
            // 3. CheckedIn -> InProgress
            appointment.StartConsultation();
            appointment.Status.Should().Be(AppointmentStatus.InProgress);
            appointment.StartedAtUtc.Should().NotBeNull();
            appointment.DomainEvents.Should().Contain(e => e is AppointmentStartedEvent);
            // 4. InProgress -> Completed
            appointment.Complete();
            appointment.Status.Should().Be(AppointmentStatus.Completed);
            appointment.CompletedAtUtc.Should().NotBeNull();
            appointment.DomainEvents.Should().Contain(e => e is AppointmentCompletedEvent);
            // 5. Issue Prescription (allowed in Completed state)
            appointment.IssuePrescription("Acute Bronchitis", "Drink warm fluids.");
            appointment.Prescription.Should().NotBeNull();
            appointment.Prescription!.Diagnosis.Should().Be("Acute Bronchitis");
            appointment.DomainEvents.Should().Contain(e => e is PrescriptionIssuedEvent);
        }
        [Theory]
        [InlineData(AppointmentStatus.Scheduled)]
        [InlineData(AppointmentStatus.Confirmed)]
        public void Cancel_WhenScheduledOrConfirmed_ShouldSucceed(AppointmentStatus initialStatus)
        {
            // Arrange
            var appointment = CreateValidScheduledAppointment();
            if (initialStatus == AppointmentStatus.Confirmed)
            {
                appointment.Confirm();
            }
            // Act
            appointment.Cancel("Patient unavailable.");
            // Assert
            appointment.Status.Should().Be(AppointmentStatus.Cancelled);
            appointment.CancellationReason.Should().Be("Patient unavailable.");
            appointment.DomainEvents.Should().Contain(e => e is AppointmentCancelledEvent);
        }
        [Fact]
        public void Cancel_WhenAlreadyCheckedIn_ShouldThrowInvalidStateTransitionException()
        {
            // Arrange
            var appointment = CreateValidScheduledAppointment();
            appointment.Confirm();
            appointment.CheckIn(); // Patient has arrived at clinic
                                   // Act
            var act = () => appointment.Cancel("Cannot cancel once arrived.");
            // Assert
            act.Should().Throw<InvalidStateTransitionException>();
        }
        [Fact]
        public void Cancel_WhenInProgress_ShouldThrowInvalidStateTransitionException()
        {
            // Arrange
            var appointment = CreateValidScheduledAppointment();
            appointment.Confirm();
            appointment.CheckIn();
            appointment.StartConsultation(); // Doctor is currently examining patient
                                             // Act
            var act = () => appointment.Cancel("Cancel mid-exam");
            // Assert
            act.Should().Throw<InvalidStateTransitionException>();
        }
        [Fact]
        public void IssuePrescription_WhenNotCompleted_ShouldThrowPrescriptionRuleViolationException()
        {
            // Arrange (Still in Scheduled state)
            var appointment = CreateValidScheduledAppointment();
            // Act
            var act = () => appointment.IssuePrescription("Flu", "Rest");
            // Assert
            act.Should().Throw<PrescriptionRuleViolationException>()
                .WithMessage("*must be Completed*");
        }
        [Fact]
        public void IssuePrescription_WhenAlreadyIssued_ShouldThrowPrescriptionRuleViolationException()
        {
            // Arrange
            var appointment = CreateValidScheduledAppointment();
            appointment.Confirm();
            appointment.CheckIn();
            appointment.StartConsultation();
            appointment.Complete();
            // First prescription
            appointment.IssuePrescription("Diagnosis 1");
            // Act: Attempt duplicate prescription
            var act = () => appointment.IssuePrescription("Diagnosis 2");
            // Assert
            act.Should().Throw<PrescriptionRuleViolationException>()
                .WithMessage("*already been issued*");
        }
    }
}