using HealthCare.Domain.Entities;
using HealthCare.Domain.ValueObjects;

namespace HealthCare.Domain.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Patient?> GetByMrnAsync(MedicalRecordNumber mrn, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Patient>> SearchByFullNameAsync(string fullName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Patient>> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithCompositeIdentityAsync(string firstName, string lastName, DateOnly dateOfBirth, PhoneNumber phoneNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
    void Update(Patient patient);
}