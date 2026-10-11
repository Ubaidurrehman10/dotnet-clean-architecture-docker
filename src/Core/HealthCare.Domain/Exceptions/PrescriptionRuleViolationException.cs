namespace HealthCare.Domain.Exceptions
{
    public class PrescriptionRuleViolationException : DomainException
    {
        public PrescriptionRuleViolationException(string message) : base(message) { }
    }
}