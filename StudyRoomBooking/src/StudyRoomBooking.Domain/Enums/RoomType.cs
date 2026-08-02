namespace StudyRoomBooking.Domain.Enums;

/// <summary>
/// Categories of bookable spaces. Laboratory and DesignStudio are treated
/// as "specialised" rooms that require restricted access checks.
/// </summary>
public enum RoomType
{
    Classroom,
    MeetingRoom,
    StudyPod,
    Laboratory,
    DesignStudio
}
