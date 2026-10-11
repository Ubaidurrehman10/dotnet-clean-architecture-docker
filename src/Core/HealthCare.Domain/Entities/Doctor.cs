using HealthCare.Domain.Common;
using HealthCare.Domain.Enums;
using HealthCare.Domain.ValueObjects;

namespace HealthCare.Domain.Entities
{
    public class Doctor : AggregateRoot
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Email Email { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; }
        public Specialization Specialization { get; private set; }
        public string LicenseNumber { get; private set; }
        public bool IsActive { get; private set; }
        private readonly List<DoctorSchedule> _schedules = [];
        public IReadOnlyCollection<DoctorSchedule> Schedules => _schedules.AsReadOnly();
        public string FullName => $"Dr. {FirstName} {LastName}";
        // Private constructor for EF Core
        private Doctor() { }
        public static Doctor Create(
            string firstName,
            string lastName,
            Email email,
            PhoneNumber phoneNumber,
            Specialization specialization,
            string licenseNumber)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required.", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required.", nameof(lastName));
            if (string.IsNullOrWhiteSpace(licenseNumber))
                throw new ArgumentException("Medical license number is required.", nameof(licenseNumber));
            return new Doctor
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email,
                PhoneNumber = phoneNumber,
                Specialization = specialization,
                LicenseNumber = licenseNumber.Trim(),
                IsActive = true
            };
        }
        public void AddSchedule(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime, int slotDurationMinutes = 30)
        {
            // Check for schedule overlap on the same day
            var hasOverlap = _schedules.Any(s =>
                s.DayOfWeek == dayOfWeek &&
                s.StartTime < endTime &&
                s.EndTime > startTime);
            if (hasOverlap)
                throw new InvalidOperationException($"Doctor already has a conflicting schedule on {dayOfWeek}.");
            _schedules.Add(new DoctorSchedule(Id, dayOfWeek, startTime, endTime, slotDurationMinutes));
        }
        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}