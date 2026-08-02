using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudyRoomBooking.Application.DTOs;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Tests;

[TestClass]
public class BookingServiceCreationTests
{
    private TestFixture _fixture = null!;

    [TestInitialize]
    public void Setup() => _fixture = new TestFixture();

    [TestMethod]
    public void CreateBooking_Succeeds_ForValidRequest()
    {
        var room = _fixture.RoomOfType(RoomType.StudyPod);
        var student = _fixture.StudentInProgramme("Computer Science");

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

    [TestMethod]
    public void CreateBooking_Fails_WhenRoomDoesNotExist()
    {
        var student = _fixture.StudentInProgramme("Computer Science");

        var result = _fixture.BookingService.CreateBooking(new BookingRequest
        {
            RoomId = Guid.NewGuid(),
            UserId = student.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        });

        Assert.IsFalse(result.Success);
        Assert.AreEqual("ROOM_NOT_FOUND", result.ErrorCode);
    }

    [TestMethod]
    public void CreateBooking_Fails_WhenUserDoesNotExist()
    {
        var room = _fixture.RoomOfType(RoomType.StudyPod);

        var result = _fixture.BookingService.CreateBooking(new BookingRequest
        {
            RoomId = room.Id,
            UserId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        });

        Assert.IsFalse(result.Success);
        Assert.AreEqual("USER_NOT_FOUND", result.ErrorCode);
    }

    [TestMethod]
    public void CreateBooking_Fails_WhenEndTimeNotAfterStartTime()
    {
        var room = _fixture.RoomOfType(RoomType.StudyPod);
        var student = _fixture.StudentInProgramme("Computer Science");
        var time = DateTime.UtcNow.AddHours(1);

        var result = _fixture.BookingService.CreateBooking(new BookingRequest
        {
            RoomId = room.Id,
            UserId = student.Id,
            StartTime = time,
            EndTime = time
        });

        Assert.IsFalse(result.Success);
        Assert.AreEqual("INVALID_TIME_RANGE", result.ErrorCode);
    }

    [TestMethod]
    public void CreateBooking_Fails_WhenStartTimeIsInThePast()
    {
        var room = _fixture.RoomOfType(RoomType.StudyPod);
        var student = _fixture.StudentInProgramme("Computer Science");

        var result = _fixture.BookingService.CreateBooking(new BookingRequest
        {
            RoomId = room.Id,
            UserId = student.Id,
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1)
        });

        Assert.IsFalse(result.Success);
        Assert.AreEqual("INVALID_TIME_RANGE", result.ErrorCode);
    }
}
