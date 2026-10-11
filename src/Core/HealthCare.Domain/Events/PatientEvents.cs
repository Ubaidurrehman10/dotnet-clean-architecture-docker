using HealthCare.Domain.Common;

namespace HealthCare.Domain.Events
{
    public record PatientRegisteredEvent(
        Guid PatientId,
        string FullName,
        string PhoneNumber,
        string? Email) : IDomainEvent;
}