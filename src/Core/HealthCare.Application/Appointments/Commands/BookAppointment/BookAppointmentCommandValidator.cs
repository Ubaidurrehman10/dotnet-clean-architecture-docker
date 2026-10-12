using FluentValidation;

namespace HealthCare.Application.Appointments.Commands.BookAppointment
{
    public class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
    {
        public BookAppointmentCommandValidator()
        {
            RuleFor(x => x.DoctorId)
                .NotEmpty().WithMessage("Doctor ID is required.");
            RuleFor(x => x.PatientId)
                .NotEmpty().WithMessage("Patient ID is required.");
            RuleFor(x => x.BookedByUserId)
                .NotEmpty().WithMessage("BookedBy user ID is required.");
            RuleFor(x => x.StartUtc)
                .GreaterThan(DateTime.UtcNow).WithMessage("Appointment start time must be in the future.");
            RuleFor(x => x.EndUtc)
                .GreaterThan(x => x.StartUtc).WithMessage("Appointment end time must be after start time.");
            RuleFor(x => x)
                .Must(x => (x.EndUtc - x.StartUtc) >= TimeSpan.FromMinutes(15))
                .WithMessage("Appointment duration must be at least 15 minutes.")
                .Must(x => (x.EndUtc - x.StartUtc) <= TimeSpan.FromHours(2))
                .WithMessage("Appointment duration cannot exceed 2 hours.");
            RuleFor(x => x.Source)
                .IsInEnum().WithMessage("A valid booking source must be specified.");
        }
    }
}