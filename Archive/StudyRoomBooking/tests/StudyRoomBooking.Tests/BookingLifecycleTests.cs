using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudyRoomBooking.Application.DTOs;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Tests;

/// <summary>
/// Starter tests covering one example each for double-booking prevention,
/// modification, and cancellation — the three remaining lifecycle
/// behaviours from the assessment scope.
///
/// TODO (Developer 1 - Core Booking & Validation): add cases for
///   - back-to-back bookings that touch but do not overlap (should succeed)
///   - modifying a booking to a time that conflicts with a *different* booking
///   - cancelling an already-cancelled booking
///   - admin overriding a conflicting booking (OverrideConflict = true)
/// </summary>
[TestClass]
public class BookingLifecycleTests
{
    private TestFixture _fixture = null!;

    [TestInitialize]
    public void Setup() => _fixture = new TestFixture();

    [TestMethod]
    public void CreateBooking_Fails_WhenTimeOverlapsExistingConfirmedBooking()
    {
        var room = _fixture.RoomOfType(RoomType.MeetingRoom);
        var studentA = _fixture.StudentInProgramme("Computer Science");
        var studentB = _fixture.StudentInProgramme("Graphic Design");
        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(1);

        var first = _fixture.BookingService.CreateBooking(new BookingRequest
        {
            RoomId = room.Id, UserId = studentA.Id, StartTime = start, EndTime = end
        });
        Assert.IsTrue(first.Success);

        var second = _fixture.BookingService.CreateBooking(new BookingRequest
        {
            RoomId = room.Id, UserId = studentB.Id, StartTime = start.AddMinutes(30), EndTime = end.AddMinutes(30)
        });

        Assert.IsFalse(second.Success);
        Assert.AreEqual("DOUBLE_BOOKING", second.ErrorCode);
    }

    [TestMethod]
    public void ModifyBooking_Succeeds_WhenOwnerMovesToFreeSlot()
    {
        var room = _fixture.RoomOfType(RoomType.MeetingRoom);
        var student = _fixture.StudentInProgramme("Computer Science");
        var start = DateTime.UtcNow.AddHours(1);

        var created = _fixture.BookingService.CreateBooking(new BookingRequest
        {
            RoomId = room.Id, UserId = student.Id, StartTime = start, EndTime = start.AddHours(1)
        });

        var modified = _fixture.BookingService.ModifyBooking(new BookingModificationRequest
        {
            BookingId = created.Booking!.Id,
            RequestingUserId = student.Id,
            NewStartTime = start.AddHours(3),
            NewEndTime = start.AddHours(4)
        });

        Assert.IsTrue(modified.Success);
        Assert.AreEqual(BookingStatus.Modified, modified.Booking!.Status);
    }

    [TestMethod]
    public void CancelBooking_Succeeds_ForOwner_AndFreesTheSlot()
    {
        var room = _fixture.RoomOfType(RoomType.MeetingRoom);
        var student = _fixture.StudentInProgramme("Computer Science");
        var start = DateTime.UtcNow.AddHours(1);

        var created = _fixture.BookingService.CreateBooking(new BookingRequest
        {
            RoomId = room.Id, UserId = student.Id, StartTime = start, EndTime = start.AddHours(1)
        });

        var cancelled = _fixture.BookingService.CancelBooking(created.Booking!.Id, student.Id, "Changed plans");
        Assert.IsTrue(cancelled.Success);
        Assert.AreEqual(BookingStatus.Cancelled, cancelled.Booking!.Status);

        // The slot should now be free for another student.
        var otherStudent = _fixture.StudentInProgramme("Graphic Design");
        var rebooked = _fixture.BookingService.CreateBooking(new BookingRequest
        {
            RoomId = room.Id, UserId = otherStudent.Id, StartTime = start, EndTime = start.AddHours(1)
        });
        Assert.IsTrue(rebooked.Success);
    }
}
