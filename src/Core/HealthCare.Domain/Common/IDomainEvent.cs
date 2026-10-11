namespace HealthCare.Domain.Common
{
    /// <summary>
    /// Marker interface representing a domain event raised when state changes.
    /// Pure C# — zero external dependencies.
    /// </summary>
    public interface IDomainEvent
    {
        Guid EventId => Guid.NewGuid();
        DateTime OccurredOnUtc => DateTime.UtcNow;
    }
}