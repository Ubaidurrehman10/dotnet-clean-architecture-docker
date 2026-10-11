using FluentAssertions;
using HealthCare.Domain.Exceptions;
using HealthCare.Domain.ValueObjects;

namespace HealthCare.Domain.UnitTests.ValueObjects
{
    public class AppointmentTimeSlotTests
    {
        [Fact]
        public void Constructor_WithValidUtcTimes_ShouldCreateSlot()
        {
            // Arrange
            var start = DateTime.UtcNow.Date.AddHours(10);
            var end = start.AddMinutes(30);
            // Act
            var slot = new AppointmentTimeSlot(start, end);
            // Assert
            slot.StartUtc.Should().Be(start);
            slot.EndUtc.Should().Be(end);
            slot.Duration.Should().Be(TimeSpan.FromMinutes(30));
        }
        [Fact]
        public void Constructor_WhenStartIsAfterOrEqualEnd_ShouldThrowInvalidTimeSlotException()
        {
            // Arrange
            var start = DateTime.UtcNow.Date.AddHours(10);
            var end = start; // Same time (duration = 0)
                             // Act
            var act = () => new AppointmentTimeSlot(start, end);
            // Assert
            act.Should().Throw<InvalidTimeSlotException>()
                .WithMessage("*strictly before end time*");
        }
        [Fact]
        public void Constructor_WhenDurationIsLessThan15Minutes_ShouldThrowInvalidTimeSlotException()
        {
            // Arrange
            var start = DateTime.UtcNow.Date.AddHours(10);
            var end = start.AddMinutes(10); // Only 10 mins
                                            // Act
            var act = () => new AppointmentTimeSlot(start, end);
            // Assert
            act.Should().Throw<InvalidTimeSlotException>()
                .WithMessage("*at least 15 minutes*");
        }
        [Theory]
        [InlineData("10:00", "10:30", "10:15", "10:45", true)]  // Partial overlap
        [InlineData("10:00", "10:30", "10:00", "10:30", true)]  // Exact same slot
        [InlineData("10:00", "11:00", "10:15", "10:45", true)]  // Slot B completely inside Slot A
        [InlineData("10:00", "10:30", "10:30", "11:00", false)] // Adjacent (Back-to-back: no overlap)
        [InlineData("10:00", "10:30", "11:00", "11:30", false)] // Completely separate
        public void OverlapsWith_ShouldCorrectlyDetectCollisions(
            string startA, string endA, string startB, string endB, bool expectedOverlap)
        {
            // Arrange
            var baseDate = DateTime.UtcNow.Date;
            var slotA = new AppointmentTimeSlot(baseDate.Add(TimeSpan.Parse(startA)), baseDate.Add(TimeSpan.Parse(endA)));
            var slotB = new AppointmentTimeSlot(baseDate.Add(TimeSpan.Parse(startB)), baseDate.Add(TimeSpan.Parse(endB)));
            // Act
            var overlaps = slotA.OverlapsWith(slotB);
            // Assert
            overlaps.Should().Be(expectedOverlap);
        }
        [Fact]
        public void Equality_WhenSlotsHaveSameStartAndEnd_ShouldBeEqual()
        {
            // Arrange
            var start = DateTime.UtcNow.Date.AddHours(9);
            var end = start.AddMinutes(45);
            var slot1 = new AppointmentTimeSlot(start, end);
            var slot2 = new AppointmentTimeSlot(start, end);
            // Act & Assert (Equality by Value)
            slot1.Should().Be(slot2);
            (slot1 == slot2).Should().BeTrue();
        }
    }
}
