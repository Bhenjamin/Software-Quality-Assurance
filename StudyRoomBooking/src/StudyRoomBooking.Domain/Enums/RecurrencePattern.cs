using System.ComponentModel.DataAnnotations;

namespace StudyRoomBooking.Domain.Enums;

public enum RecurrencePattern
{
    None,
    Daily,
    Weekly,
    BiWeekly,
    [Display(Name = "Four Weeks")]
    FourWeeks
}
