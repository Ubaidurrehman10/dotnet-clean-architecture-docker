using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Exceptions
{
    public class InvalidStateTransitionException : DomainException
    {
        public InvalidStateTransitionException(AppointmentStatus current, AppointmentStatus target)
            : base($"Cannot transition appointment status from '{current}' to '{target}'.")
        {
        }
        public InvalidStateTransitionException(string message) : base(message)
        {
        }
    }
}