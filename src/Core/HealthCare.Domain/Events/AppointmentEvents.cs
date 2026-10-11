using HealthCare.Domain.Common;
using HealthCare.Domain.Enums;
using HealthCare.Domain.ValueObjects;

namespace HealthCare.Domain.Events
{
    public record AppointmentBookedEvent(
        Guid AppointmentId,
        Guid DoctorId,
        Guid PatientId,
        AppointmentTimeSlot TimeSlot,
        BookingSource Source) : IDomainEvent;
    public record AppointmentConfirmedEvent(
        Guid AppointmentId,
        DateTime ConfirmedAtUtc) : IDomainEvent;
    public record AppointmentCheckedInEvent(
        Guid AppointmentId,
        DateTime CheckedInAtUtc) : IDomainEvent;
    public record AppointmentStartedEvent(
        Guid AppointmentId,
        DateTime StartedAtUtc) : IDomainEvent;
    public record AppointmentCompletedEvent(
        Guid AppointmentId,
        DateTime CompletedAtUtc) : IDomainEvent;
    public record AppointmentCancelledEvent(
        Guid AppointmentId,
        string CancellationReason,
        DateTime CancelledAtUtc) : IDomainEvent;
    public record AppointmentNoShowEvent(
        Guid AppointmentId,
        DateTime MarkedNoShowAtUtc) : IDomainEvent;
    public record PrescriptionIssuedEvent(
        Guid AppointmentId,
        Guid PrescriptionId,
        DateTime IssuedAtUtc) : IDomainEvent;
}