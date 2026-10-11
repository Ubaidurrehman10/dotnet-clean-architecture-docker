namespace HealthCare.Domain.Common
{
    /// <summary>
    /// Base class for all domain entities. Identity is determined solely by Id.
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }

        protected BaseEntity(Guid id)
        {
            Id = id;
        }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not BaseEntity other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(BaseEntity? left, BaseEntity? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaseEntity? left, BaseEntity? right)
        {
            return !Equals(left, right);
        }
    }
}