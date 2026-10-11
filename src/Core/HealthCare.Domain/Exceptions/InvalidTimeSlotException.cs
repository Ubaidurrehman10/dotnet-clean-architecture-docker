namespace HealthCare.Domain.Exceptions
{
    public class InvalidTimeSlotException : DomainException
    {
        public InvalidTimeSlotException(string message) : base(message) { }
    }
}