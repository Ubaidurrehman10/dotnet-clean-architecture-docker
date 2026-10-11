namespace HealthCare.Domain.Enums
{
    public enum BookingSource
    {
        SelfBooking = 1,     // Patient booked through web/app
        PhoneCall = 2,       // Receptionist booked on incoming call
        WalkIn = 3,          // Receptionist booked for walk-in patient
        FollowUp = 4         // Doctor ordered follow-up consultation
    }
}