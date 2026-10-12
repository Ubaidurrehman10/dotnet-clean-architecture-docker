using FluentAssertions;
using HealthCare.Application.Appointments.Commands.BookAppointment;
using HealthCare.Application.Common.Enums;
using HealthCare.Domain.Entities;
using HealthCare.Domain.Enums;
using HealthCare.Domain.Interfaces;
using HealthCare.Domain.ValueObjects;
using Moq;

namespace HealthCare.Application.UnitTests.Appointments.Commands
{
    public class BookAppointmentCommandHandlerTests
    {
        private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
        private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
        private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly BookAppointmentCommandHandler _handler;
        public BookAppointmentCommandHandlerTests()
        {
            _handler = new BookAppointmentCommandHandler(
                _doctorRepositoryMock.Object,
                _patientRepositoryMock.Object,
                _appointmentRepositoryMock.Object,
                _unitOfWorkMock.Object);
        }
        [Fact]
        public async Task Handle_WhenDoctorDoesNotExist_ShouldReturnNotFoundError_AndNotPersist()
        {
            // Arrange
            var command = CreateValidCommand();
            _doctorRepositoryMock
                .Setup(r => r.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.NotFound);
            result.Error.Code.Should().Be("Doctor.NotFound");
            _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task Handle_WhenDoctorIsInactive_ShouldReturnConflictError_AndNotPersist()
        {
            // Arrange
            var doctor = CreateDoctor();
            doctor.Deactivate();
            var command = CreateValidCommand(doctorId: doctor.Id);
            _doctorRepositoryMock
                .Setup(r => r.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.Conflict);
            result.Error.Code.Should().Be("Doctor.Inactive");
            _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task Handle_WhenPatientDoesNotExist_ShouldReturnNotFoundError_AndNotPersist()
        {
            // Arrange
            var doctor = CreateDoctor();
            var command = CreateValidCommand(doctorId: doctor.Id);
            _doctorRepositoryMock
                .Setup(r => r.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);
            _patientRepositoryMock
                .Setup(r => r.GetByIdAsync(command.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.NotFound);
            result.Error.Code.Should().Be("Patient.NotFound");
            _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task Handle_WhenSlotHasConflict_ShouldReturnConflictError_AndNotPersist()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var command = CreateValidCommand(doctorId: doctor.Id, patientId: patient.Id);
            _doctorRepositoryMock
                .Setup(r => r.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);
            _patientRepositoryMock
                .Setup(r => r.GetByIdAsync(command.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);
            _appointmentRepositoryMock
                .Setup(r => r.HasConflictAsync(command.DoctorId, It.IsAny<AppointmentTimeSlot>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Type.Should().Be(ErrorType.Conflict);
            result.Error.Code.Should().Be("Appointment.SlotUnavailable");
            _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task Handle_WhenValidBooking_ShouldReturnSuccess_AndPersistAppointment()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var command = CreateValidCommand(doctorId: doctor.Id, patientId: patient.Id);
            _doctorRepositoryMock
                .Setup(r => r.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);
            _patientRepositoryMock
                .Setup(r => r.GetByIdAsync(command.PatientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);
            _appointmentRepositoryMock
                .Setup(r => r.HasConflictAsync(command.DoctorId, It.IsAny<AppointmentTimeSlot>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            Appointment? savedAppointment = null;
            _appointmentRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                .Callback<Appointment, CancellationToken>((apt, _) => savedAppointment = apt)
                .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
            savedAppointment.Should().NotBeNull();
            savedAppointment!.DoctorId.Should().Be(command.DoctorId);
            savedAppointment.PatientId.Should().Be(command.PatientId);
            savedAppointment.Status.Should().Be(AppointmentStatus.Scheduled);
            savedAppointment.Source.Should().Be(command.Source);
            _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        private static BookAppointmentCommand CreateValidCommand(Guid? doctorId = null, Guid? patientId = null) =>
            new(
                DoctorId: doctorId ?? Guid.NewGuid(),
                PatientId: patientId ?? Guid.NewGuid(),
                StartUtc: DateTime.UtcNow.AddDays(1).Date.AddHours(9),
                EndUtc: DateTime.UtcNow.AddDays(1).Date.AddHours(9).AddMinutes(30),
                Source: BookingSource.SelfBooking,
                BookedByUserId: Guid.NewGuid());
        private static Doctor CreateDoctor() =>
            Doctor.Create(
                "Jane",
                "Smith",
                new Email("doctor.smith@hospital.org"),
                new PhoneNumber("+12345678901"),
                Specialization.GeneralPractice,
                "MD-123456");
        private static Patient CreatePatient() =>
            Patient.Register(
                new MedicalRecordNumber("MRN-2026-000100"),
                "John",
                "Doe",
                new DateOnly(1985, 4, 12),
                new PhoneNumber("+19876543210"));
    }
}