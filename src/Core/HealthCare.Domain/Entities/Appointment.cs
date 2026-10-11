using HealthCare.Domain.Common;
using HealthCare.Domain.Enums;
using HealthCare.Domain.Events;
using HealthCare.Domain.Exceptions;
using HealthCare.Domain.ValueObjects;

namespace HealthCare.Domain.Entities
{
    public class Appointment : AggregateRoot
    {
        public Guid DoctorId { get; private set; }
        public Guid PatientId { get; private set; }
        public AppointmentTimeSlot TimeSlot { get; private set; }
        public AppointmentStatus Status { get; private set; }
        public BookingSource Source { get; private set; }
        // Actor context
        public Guid BookedByUserId { get; private set; }
        public Guid? OnBehalfOfUserId { get; private set; }
        public Guid? ParentAppointmentId { get; private set; }
        // State audit trail
        public string? CancellationReason { get; private set; }
        public DateTime? ConfirmedAtUtc { get; private set; }
        public DateTime? CheckedInAtUtc { get; private set; }
        public DateTime? StartedAtUtc { get; private set; }
        public DateTime? CompletedAtUtc { get; private set; }
        // Prescription child entity
        public Prescription? Prescription { get; private set; }
        // Private constructor for EF Core
        private Appointment() { }
        /// <summary>
        /// Factory method to book a new appointment in 'Scheduled' state.
        /// Invariant: Cannot book in the past.
        /// </summary>
        public static Appointment Book(
            Guid doctorId,
            Guid patientId,
            AppointmentTimeSlot timeSlot,
            BookingSource source,
            Guid bookedByUserId,
            Guid? onBehalfOfUserId = null,
            Guid? parentAppointmentId = null)
        {
            if (timeSlot.StartUtc <= DateTime.UtcNow)
                throw new InvalidTimeSlotException("Cannot schedule an appointment in the past.");
            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = patientId,
                TimeSlot = timeSlot,
                Status = AppointmentStatus.Scheduled,
                Source = source,
                BookedByUserId = bookedByUserId,
                OnBehalfOfUserId = onBehalfOfUserId,
                ParentAppointmentId = parentAppointmentId
            };
            appointment.RaiseDomainEvent(new AppointmentBookedEvent(
                appointment.Id,
                doctorId,
                patientId,
                timeSlot,
                source));
            return appointment;
        }
        /// <summary>
        /// Confirms a scheduled appointment.
        /// Transition: Scheduled -> Confirmed
        /// </summary>
        public void Confirm()
        {
            if (Status != AppointmentStatus.Scheduled)
                throw new InvalidStateTransitionException(Status, AppointmentStatus.Confirmed);
            Status = AppointmentStatus.Confirmed;
            ConfirmedAtUtc = DateTime.UtcNow;
            RaiseDomainEvent(new AppointmentConfirmedEvent(Id, ConfirmedAtUtc.Value));
        }
        /// <summary>
        /// Marks patient arrival at clinic.
        /// Transition: Confirmed -> CheckedIn
        /// </summary>
        public void CheckIn()
        {
            if (Status != AppointmentStatus.Confirmed)
                throw new InvalidStateTransitionException(Status, AppointmentStatus.CheckedIn);
            Status = AppointmentStatus.CheckedIn;
            CheckedInAtUtc = DateTime.UtcNow;
            RaiseDomainEvent(new AppointmentCheckedInEvent(Id, CheckedInAtUtc.Value));
        }
        /// <summary>
        /// Doctor begins consultation in exam room.
        /// Transition: CheckedIn -> InProgress
        /// </summary>
        public void StartConsultation()
        {
            if (Status != AppointmentStatus.CheckedIn)
                throw new InvalidStateTransitionException(Status, AppointmentStatus.InProgress);
            Status = AppointmentStatus.InProgress;
            StartedAtUtc = DateTime.UtcNow;
            RaiseDomainEvent(new AppointmentStartedEvent(Id, StartedAtUtc.Value));
        }
        /// <summary>
        /// Doctor concludes consultation.
        /// Transition: InProgress -> Completed
        /// Terminal state.
        /// </summary>
        public void Complete()
        {
            if (Status != AppointmentStatus.InProgress)
                throw new InvalidStateTransitionException(Status, AppointmentStatus.Completed);
            Status = AppointmentStatus.Completed;
            CompletedAtUtc = DateTime.UtcNow;
            RaiseDomainEvent(new AppointmentCompletedEvent(Id, CompletedAtUtc.Value));
        }
        /// <summary>
        /// Cancels appointment with required reason.
        /// Allowed ONLY from: Scheduled or Confirmed.
        /// Terminal state.
        /// </summary>
        public void Cancel(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("A cancellation reason is required.", nameof(reason));
            if (Status != AppointmentStatus.Scheduled && Status != AppointmentStatus.Confirmed)
                throw new InvalidStateTransitionException(Status, AppointmentStatus.Cancelled);
            Status = AppointmentStatus.Cancelled;
            CancellationReason = reason.Trim();
            RaiseDomainEvent(new AppointmentCancelledEvent(Id, CancellationReason, DateTime.UtcNow));
        }
        /// <summary>
        /// Marks patient as NoShow when slot passes with no check-in.
        /// Allowed ONLY from: Scheduled or Confirmed.
        /// Terminal state.
        /// </summary>
        public void MarkNoShow()
        {
            if (Status != AppointmentStatus.Scheduled && Status != AppointmentStatus.Confirmed)
                throw new InvalidStateTransitionException(Status, AppointmentStatus.NoShow);
            Status = AppointmentStatus.NoShow;
            RaiseDomainEvent(new AppointmentNoShowEvent(Id, DateTime.UtcNow));
        }
        /// <summary>
        /// Issues a clinical prescription.
        /// Business invariant: Prescriptions can ONLY be issued when status is 'Completed'.
        /// </summary>
        public void IssuePrescription(string diagnosis, string? doctorNotes = null, IEnumerable<MedicationItem>? medications = null)
        {
            if (Status != AppointmentStatus.Completed)
                throw new PrescriptionRuleViolationException(
                    $"Cannot issue a prescription for an appointment with status '{Status}'. Consultation must be Completed.");
            if (Prescription != null)
                throw new PrescriptionRuleViolationException("A prescription has already been issued for this appointment.");
            var prescription = new Prescription(Id, diagnosis, doctorNotes);
            if (medications != null)
            {
                foreach (var med in medications)
                {
                    prescription.AddMedication(med);
                }
            }
            Prescription = prescription;
            RaiseDomainEvent(new PrescriptionIssuedEvent(Id, prescription.Id, prescription.IssuedAtUtc));
        }
    }
}