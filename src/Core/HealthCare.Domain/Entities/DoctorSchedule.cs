using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities
{
    public class DoctorSchedule : BaseEntity
    {
        public Guid DoctorId { get; private set; }
        public DayOfWeek DayOfWeek { get; private set; }
        public TimeOnly StartTime { get; private set; }
        public TimeOnly EndTime { get; private set; }
        public int SlotDurationMinutes { get; private set; }
        public DoctorSchedule(
            Guid doctorId,
            DayOfWeek dayOfWeek,
            TimeOnly startTime,
            TimeOnly endTime,
            int slotDurationMinutes = 30)
        {
            if (startTime >= endTime)
                throw new ArgumentException("Schedule start time must be before end time.");
            if (slotDurationMinutes < 15 || slotDurationMinutes > 120)
                throw new ArgumentException("Slot duration must be between 15 and 120 minutes.");
            DoctorId = doctorId;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            SlotDurationMinutes = slotDurationMinutes;
        }
    }
}