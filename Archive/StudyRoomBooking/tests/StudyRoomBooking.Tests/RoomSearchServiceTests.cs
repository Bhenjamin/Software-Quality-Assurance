using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudyRoomBooking.Application.DTOs;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Tests;

[TestClass]
public class RoomSearchServiceTests
{
    private TestFixture _fixture = null!;

    [TestInitialize]
    public void Setup() => _fixture = new TestFixture();

    [TestMethod]
    public void Search_ReturnsRoomsMatchingMinCapacity()
    {
        var results = _fixture.RoomSearchService.SearchAvailableRooms(new RoomSearchCriteria
        {
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            MinCapacity = 20
        });

        Assert.IsTrue(results.All(r => r.Capacity >= 20));
        Assert.IsTrue(results.Any(), "Expected at least one seeded room with capacity >= 20.");
    }

    [TestMethod]
    public void Search_FiltersByRoomType()
    {
        var results = _fixture.RoomSearchService.SearchAvailableRooms(new RoomSearchCriteria
        {
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Type = RoomType.StudyPod
        });

        Assert.IsTrue(results.All(r => r.Type == RoomType.StudyPod));
        Assert.AreEqual(2, results.Count());
    }

    [TestMethod]
    public void Search_ExcludesRoomsWithOverlappingConfirmedBooking()
    {
        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(1);
        var room = _fixture.RoomOfType(RoomType.MeetingRoom);
        var student = _fixture.StudentInProgramme("Computer Science");

        // Book the room directly through the booking service to set up overlap.
        var booking = _fixture.BookingService.CreateBooking(new BookingRequest
        {
            RoomId = room.Id,
            UserId = student.Id,
            StartTime = start,
            EndTime = end,
            Purpose = "Setup"
        });
        Assert.IsTrue(booking.Success);

        var results = _fixture.RoomSearchService.SearchAvailableRooms(new RoomSearchCriteria
        {
            StartTime = start.AddMinutes(30),
            EndTime = end.AddMinutes(30),
        });

        Assert.IsFalse(results.Any(r => r.Id == room.Id));
    }

    [TestMethod]
    public void Search_ThrowsArgumentException_WhenEndTimeNotAfterStartTime()
    {
        var now = DateTime.UtcNow.AddHours(1);
        Assert.ThrowsException<ArgumentException>(() =>
            _fixture.RoomSearchService.SearchAvailableRooms(new RoomSearchCriteria
            {
                StartTime = now,
                EndTime = now
            }).ToList());
    }

    [TestMethod]
    public void Search_FiltersByLocation()
    {
        var results = _fixture.RoomSearchService.SearchAvailableRooms(new RoomSearchCriteria
        {
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Location = "Library"
        });

        Assert.IsTrue(results.All(r => r.Location.Contains("Library")));
    }
}
