using HealthCare.Domain.Common;
using HealthCare.Domain.ValueObjects;

namespace HealthCare.Domain.Events;

public record PatientRegisteredEvent(
    Guid PatientId,
    MedicalRecordNumber Mrn,
    string FullName,
    string PhoneNumber,
    string? Email) : IDomainEvent;