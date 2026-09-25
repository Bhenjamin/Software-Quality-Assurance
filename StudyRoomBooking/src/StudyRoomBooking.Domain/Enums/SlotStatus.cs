namespace StudyRoomBooking.Domain.Enums;

/// <summary>
/// Represents the status of a time slot across the entire recurrence period.
/// Priority (high to low): Unavailable > Mixed > Booked > YourBooking > Available
/// </summary>
public enum SlotStatus
{
    /// <summary>Grey - Slot is in the past (only when StartDate is today and hour has passed) or falls outside search range</summary>
    Unavailable,

    /// <summary>Red + Blue - Mixed booking: both current user and other users have bookings across recurrence dates</summary>
    Mixed,

    /// <summary>Red - Slot is booked by another user on one or more recurrence dates</summary>
    Booked,

    /// <summary>Blue - Slot has a booking by the current user on one or more recurrence dates</summary>
    YourBooking,

    /// <summary>Green - Slot is free on every recurrence date and can be booked</summary>
    Available
}
