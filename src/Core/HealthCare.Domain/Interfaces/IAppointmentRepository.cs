using HealthCare.Domain.Entities;
using HealthCare.Domain.ValueObjects;

namespace HealthCare.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Appointment?> GetWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetDoctorAppointmentsForDateAsync(Guid doctorId, DateOnly date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetPatientAppointmentsAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<bool> HasConflictAsync(Guid doctorId, AppointmentTimeSlot slot, CancellationToken cancellationToken = default);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    void Update(Appointment appointment);
}