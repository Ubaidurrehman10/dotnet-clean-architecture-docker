namespace HealthCare.Domain.Common
{
    /// <summary>
    /// Base class for Value Objects. Enforces equality based on the values of its components.
    /// Value objects are immutable.
    /// </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        protected abstract IEnumerable<object?> GetEqualityComponents();
        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;
            var other = (ValueObject)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }
        public bool Equals(ValueObject? other)
        {
            return Equals((object?)other);
        }
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(default(int), (hash, component) =>
                    HashCode.Combine(hash, component?.GetHashCode() ?? 0));
        }
        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            return Equals(left, right);
        }
        public static bool operator !=(ValueObject? left, ValueObject? right)
        {
            return !Equals(left, right);
        }
    }
}
