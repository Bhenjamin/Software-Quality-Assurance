using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudyRoomBooking.Application.DTOs;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Tests;

/// <summary>

/// </summary>
[TestClass]
public class SelfCreatedTests
{
    private TestFixture _fixture = null!;

    [TestInitialize]
    public void Setup() => _fixture = new TestFixture();

    // TC-01 Book an available room successfully
    [TestMethod]
    public void CreateBooking_Succeeds_ForValidRequest()
    {
        var room = _fixture.RoomOfType(RoomType.StudyPod);
        var student = _fixture.StudentInProgramme("Graphic Design");

        var result = _fixture.BookingService.CreateBooking(new BookingRequest
        {
            RoomId = room.Id,
            UserId = student.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Purpose = "Exam revision"
        });

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Booking);
        Assert.AreEqual(BookingStatus.Confirmed, result.Booking!.Status);
    }

    // TC-02 Reject a double-booking for the same room and time slot
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

    // TC-03 Reject a specialised room booking from an ineligible student
    [TestMethod]
    public void CanAccessRoom_ReturnsFalse_ForStudentOutsideAllowedProgramme()
    {
        var lab = _fixture.RoomOfType(RoomType.Laboratory); // allowed: Computer Science, Software Engineering
        var designStudent = _fixture.StudentInProgramme("Graphic Design");

        Assert.IsFalse(_fixture.AccessControlService.CanAccessRoom(designStudent, lab));
    }

    // TC-04 Allow a specialised room booking from an eligible student
    [TestMethod]
    public void CanAccessRoom_ReturnsTrue_ForStudentWithinAllowedProgramme()
    {
        var lab = _fixture.RoomOfType(RoomType.Laboratory); // allowed: Computer Science, Software Engineering
        var csStudent = _fixture.StudentInProgramme("Computer Science");

        Assert.IsTrue(_fixture.AccessControlService.CanAccessRoom(csStudent, lab));
    }

    // TC-05 Modify an existing future booking to a new available time
    [TestMethod]
    public void ModifyBooking_Succeeds_WhenNewTimeSlotIsAvailable()
    {
        var room = _fixture.RoomOfType(RoomType.StudyPod);
        var student = _fixture.StudentInProgramme("Computer Science");
        var originalStart = DateTime.UtcNow.AddHours(2);
        var originalEnd = originalStart.AddHours(1);

        // Create the original booking
        var createResult = _fixture.BookingService.CreateBooking(new BookingRequest
        {
            RoomId = room.Id,
            UserId = student.Id,
            StartTime = originalStart,
            EndTime = originalEnd,
            Purpose = "Study session"
        });

        Assert.IsTrue(createResult.Success);
        var bookingId = createResult.Booking!.Id;

        // Modify the booking to a new available time slot
        var newStart = DateTime.UtcNow.AddHours(5);
        var newEnd = newStart.AddHours(1);
        var modifyResult = _fixture.BookingService.ModifyBooking(new BookingModificationRequest
        {
            BookingId = bookingId,
            RequestingUserId = student.Id,
            NewStartTime = newStart,
            NewEndTime = newEnd
        });

        Assert.IsTrue(modifyResult.Success);
        Assert.IsNotNull(modifyResult.Booking);
        Assert.AreEqual(newStart, modifyResult.Booking!.StartTime);
        Assert.AreEqual(newEnd, modifyResult.Booking!.EndTime);
        Assert.AreEqual(BookingStatus.Modified, modifyResult.Booking!.Status);
    }

    // TC-06 Modify an existing future booking to a new available time

    
}