namespace HealthCare.Domain.Common
{
    /// <summary>
    /// Base class for Aggregate Roots. Encapsulates business invariants
    /// and records domain events raised by state mutations.
    /// </summary>
    public abstract class AggregateRoot : AuditableEntity
    {
        private readonly List<IDomainEvent> _domainEvents = [];
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        protected AggregateRoot(Guid id) : base(id) { }
        protected AggregateRoot() : base() { }
        protected void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}