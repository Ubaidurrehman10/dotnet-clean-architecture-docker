using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities
{
    public class Prescription : BaseEntity
    {
        public Guid AppointmentId { get; private set; }
        public DateTime IssuedAtUtc { get; private set; }
        public string Diagnosis { get; private set; }
        public string? DoctorNotes { get; private set; }
        private readonly List<MedicationItem> _medications = [];
        public IReadOnlyCollection<MedicationItem> Medications => _medications.AsReadOnly();
        public Prescription(
            Guid appointmentId,
            string diagnosis,
            string? doctorNotes = null)
        {
            if (string.IsNullOrWhiteSpace(diagnosis))
                throw new ArgumentException("Diagnosis is required to issue a prescription.", nameof(diagnosis));
            AppointmentId = appointmentId;
            Diagnosis = diagnosis.Trim();
            DoctorNotes = doctorNotes?.Trim();
            IssuedAtUtc = DateTime.UtcNow;
        }
        public void AddMedication(MedicationItem medication)
        {
            ArgumentNullException.ThrowIfNull(medication);
            _medications.Add(medication);
        }
    }
}