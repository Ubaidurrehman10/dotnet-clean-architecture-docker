using HealthCare.Application.Common.Models;
using HealthCare.Domain.Entities;
using HealthCare.Domain.Interfaces;
using HealthCare.Domain.ValueObjects;
using MediatR;

namespace HealthCare.Application.Appointments.Commands.BookAppointment
{
    public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, Result<Guid>>
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        public BookAppointmentCommandHandler(
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IAppointmentRepository appointmentRepository,
            IUnitOfWork unitOfWork)
        {
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _appointmentRepository = appointmentRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<Guid>> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify Doctor exists & is active
            var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);
            if (doctor is null)
            {
                return Result.Failure<Guid>(Error.NotFound("Doctor.NotFound", $"Doctor with ID '{request.DoctorId}' was not found."));
            }
            if (!doctor.IsActive)
            {
                return Result.Failure<Guid>(Error.Conflict("Doctor.Inactive", "The doctor is currently not accepting appointments."));
            }
            // 2. Verify Patient exists
            var patient = await _patientRepository.GetByIdAsync(request.PatientId, cancellationToken);
            if (patient is null)
            {
                return Result.Failure<Guid>(Error.NotFound("Patient.NotFound", $"Patient with ID '{request.PatientId}' was not found."));
            }
            // 3. Construct the Appointment Time Slot Value Object
            var timeSlot = new AppointmentTimeSlot(request.StartUtc, request.EndUtc);
            // 4. Clinical Double-Booking Check
            var hasConflict = await _appointmentRepository.HasConflictAsync(request.DoctorId, timeSlot, cancellationToken);
            if (hasConflict)
            {
                return Result.Failure<Guid>(Error.Conflict("Appointment.SlotUnavailable", "The selected time slot is already booked for this doctor."));
            }
            // 5. Create Domain Aggregate (enforces domain invariants & raises AppointmentBookedEvent)
            var appointment = Appointment.Book(
                request.DoctorId,
                request.PatientId,
                timeSlot,
                request.Source,
                request.BookedByUserId,
                request.OnBehalfOfUserId,
                request.ParentAppointmentId);
            // 6. Persist changes
            await _appointmentRepository.AddAsync(appointment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            // 7. Return Result with generated Appointment GUID
            return Result.Success(appointment.Id);
        }
    }
}