using HealthCare.Application.Common.Models;
using HealthCare.Domain.Enums;
using MediatR;

namespace HealthCare.Application.Appointments.Commands.BookAppointment
{
    public record BookAppointmentCommand(
        Guid DoctorId,
        Guid PatientId,
        DateTime StartUtc,
        DateTime EndUtc,
        BookingSource Source,
        Guid BookedByUserId,
        Guid? OnBehalfOfUserId = null,
        Guid? ParentAppointmentId = null) : IRequest<Result<Guid>>;
}