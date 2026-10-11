namespace HealthCare.Domain.Common
{
    /// <summary>
    /// Adds immutable audit metadata to entities.
    /// Automatically populated by EF Core interceptors on SaveChanges.
    /// </summary>
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAtUtc { get; set; }
        public string? LastModifiedBy { get; set; }
        protected AuditableEntity(Guid id) : base(id) { }
        protected AuditableEntity() : base() { }
    }
}