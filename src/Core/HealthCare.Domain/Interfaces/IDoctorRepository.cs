using HealthCare.Domain.Entities;
using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Interfaces
{
    public interface IDoctorRepository
    {
        Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Doctor>> GetBySpecializationAsync(Specialization specialization, CancellationToken cancellationToken = default);
        Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default);
    }
}