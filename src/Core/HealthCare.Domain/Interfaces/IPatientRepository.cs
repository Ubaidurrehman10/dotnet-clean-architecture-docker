using HealthCare.Domain.Entities;

namespace HealthCare.Domain.Interfaces
{
    public interface IPatientRepository
    {
        Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithIdentityAsync(string firstName, string lastName, DateOnly dateOfBirth, string phone, CancellationToken cancellationToken = default);
        Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
    }
}