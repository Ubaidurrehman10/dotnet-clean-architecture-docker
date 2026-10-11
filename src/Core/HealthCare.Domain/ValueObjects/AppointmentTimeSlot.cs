using HealthCare.Domain.Common;
using HealthCare.Domain.Exceptions;

namespace HealthCare.Domain.ValueObjects
{
    public class AppointmentTimeSlot : ValueObject
    {
        public DateTime StartUtc { get; }
        public DateTime EndUtc { get; }
        public TimeSpan Duration => EndUtc - StartUtc;
        public AppointmentTimeSlot(DateTime startUtc, DateTime endUtc)
        {
            if (startUtc >= endUtc)
                throw new InvalidTimeSlotException("Appointment start time must be strictly before end time.");
            if (endUtc - startUtc < TimeSpan.FromMinutes(15))
                throw new InvalidTimeSlotException("Appointment duration must be at least 15 minutes.");
            if (endUtc - startUtc > TimeSpan.FromHours(2))
                throw new InvalidTimeSlotException("Appointment duration cannot exceed 2 hours.");
            StartUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
            EndUtc = DateTime.SpecifyKind(endUtc, DateTimeKind.Utc);
        }
        // Factory method for creating slots with standard duration (e.g. 30 mins)
        public static AppointmentTimeSlot Create(DateTime startUtc, TimeSpan duration)
        {
            return new AppointmentTimeSlot(startUtc, startUtc.Add(duration));
        }
        /// <summary>
        /// Checks whether two appointment slots overlap in time.
        /// Overlap formula: Slot A starts before Slot B ends AND Slot A ends after Slot B starts.
        /// </summary>
        public bool OverlapsWith(AppointmentTimeSlot other)
        {
            return StartUtc < other.EndUtc && EndUtc > other.StartUtc;
        }
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return StartUtc;
            yield return EndUtc;
        }
        public override string ToString() => $"{StartUtc:yyyy-MM-dd HH:mm} - {EndUtc:HH:mm} UTC";
    }
}