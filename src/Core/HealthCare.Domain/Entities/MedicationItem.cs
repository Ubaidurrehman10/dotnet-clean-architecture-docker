using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities
{
    public class MedicationItem : BaseEntity
    {
        public string MedicineName { get; private set; }
        public string Dosage { get; private set; }
        public string Frequency { get; private set; }
        public int DurationInDays { get; private set; }
        public string? Instructions { get; private set; }
        public MedicationItem(
            string medicineName,
            string dosage,
            string frequency,
            int durationInDays,
            string? instructions = null)
        {
            if (string.IsNullOrWhiteSpace(medicineName))
                throw new ArgumentException("Medicine name is required.", nameof(medicineName));
            if (string.IsNullOrWhiteSpace(dosage))
                throw new ArgumentException("Dosage is required.", nameof(dosage));
            if (string.IsNullOrWhiteSpace(frequency))
                throw new ArgumentException("Frequency is required.", nameof(frequency));
            if (durationInDays <= 0)
                throw new ArgumentException("Duration must be greater than zero days.", nameof(durationInDays));
            MedicineName = medicineName.Trim();
            Dosage = dosage.Trim();
            Frequency = frequency.Trim();
            DurationInDays = durationInDays;
            Instructions = instructions?.Trim();
        }
    }
}