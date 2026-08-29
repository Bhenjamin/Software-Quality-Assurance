using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Interfaces;

namespace StudyRoomBooking.Tests;

// Test cases TC-01 to TC-08 from our test plan.
// These test BookingService directly (and AccessRuleService for the eligibility ones)
// using mocked repos so we don't need an actual database running.
[TestClass]
public class BookingServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IBookingRepository> _bookingRepoMock = null!;
    private Mock<IRoomRepository> _roomRepoMock = null!;
    private Mock<IUserRepository> _userRepoMock = null!;
    private Mock<INotificationService> _notificationServiceMock = null!;
    private BookingService _bookingService = null!;

    // Runs before every test so each one gets a fresh set of mocks
    [TestInitialize]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _roomRepoMock = new Mock<IRoomRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _notificationServiceMock = new Mock<INotificationService>();

        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Rooms).Returns(_roomRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);

        _bookingService = new BookingService(_unitOfWorkMock.Object, _notificationServiceMock.Object);
    }

    // TC-01: Book an available room successfully (FR2, FR3)
    // Just checks that if nobody else has the room at that time, the booking
    // actually goes through and comes back confirmed with a confirmation number.
    [TestMethod]
    public async Task TC01_BookAvailableRoom_Succeeds()
    {
        // no bookings exist yet for this room
        _bookingRepoMock.Setup(r => r.GetByRoomIdAsync(1)).ReturnsAsync(new List<Booking>());
        _userRepoMock.Setup(u => u.GetByIdAsync(10)).ReturnsAsync(new User { Id = 10, Email = "student@uni.edu" });
        _roomRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Room { Id = 1, Name = "Study Room A" });

        var bookingDate = DateTime.Today.AddDays(1);
        var newBooking = new Booking
        {
            RoomId = 1,
            UserId = 10,
            BookingDate = bookingDate,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0)
        };

        var (isValid, _) = await _bookingService.ValidateBookingAsync(
            newBooking.RoomId, newBooking.BookingDate, newBooking.StartTime, newBooking.EndTime);
        var result = await _bookingService.CreateBookingAsync(newBooking);

        Assert.IsTrue(isValid);
        Assert.AreEqual(BookingStatus.Confirmed, result.Status);
        Assert.IsFalse(string.IsNullOrEmpty(result.ConfirmationNumber));
        _bookingRepoMock.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
    }

    // TC-02: Reject a double-booking for the same room and time slot (FR3, NFR2)
    // Makes sure that if someone already has the room booked for an overlapping
    // time, a second person trying to book it gets blocked instead of it just
    // letting both bookings through.
    [TestMethod]
    public async Task TC02_DoubleBooking_IsRejected()
    {
        var bookingDate = DateTime.Today.AddDays(1);
        var existingBooking = new Booking
        {
            Id = 1,
            RoomId = 1,
            BookingDate = bookingDate,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0),
            Status = BookingStatus.Confirmed
        };
        _bookingRepoMock.Setup(r => r.GetByRoomIdAsync(1)).ReturnsAsync(new List<Booking> { existingBooking });

        // someone else tries to grab an overlapping slot on the same room
        var (isValid, errorMessage) = await _bookingService.ValidateBookingAsync(
            roomId: 1, bookingDate: bookingDate, startTime: new TimeSpan(9, 30, 0), endTime: new TimeSpan(10, 30, 0));

        Assert.IsFalse(isValid);
        StringAssert.Contains(errorMessage.ToLower(), "already booked");
    }

    // TC-03: Reject a specialised room booking from an ineligible student (FR4, NFR3)
    // This is what SHOULD happen when a student's programme doesn't match the room's
    // requirement (e.g. a Business student trying to book the Design studio).
    // Skipped for now, see note in Assert.Inconclusive below - AccessRuleService doesn't
    // actually check anything yet.
    [TestMethod]
    [Ignore("AccessRuleService.ValidateAccessAsync is just a placeholder right now and always " +
            "returns true no matter who/what room you pass in - the actual programme-matching " +
            "rule for FR4 hasn't been built yet. Un-ignore this once that's done.")]
    public async Task TC03_IneligibleStudent_SpecialisedRoomBooking_IsRejected()
    {
        var accessRuleService = new AccessRuleService();

        // in theory this is a Business student trying to book a Design-only room
        var hasAccess = await accessRuleService.ValidateAccessAsync(userId: 1, roomId: 1);

        Assert.IsFalse(hasAccess);
    }

    // TC-04: Allow a specialised room booking from an eligible student (FR4, NFR3)
    // Same deal as TC-03 but for the case where the student's programme DOES match.
    // Also skipped since there's no real rule to test against yet.
    [TestMethod]
    [Ignore("Same issue as TC-03 - AccessRuleService always returns true right now so this test " +
            "would pass without actually proving anything. Un-ignore once FR4 eligibility logic exists.")]
    public async Task TC04_EligibleStudent_SpecialisedRoomBooking_Succeeds()
    {
        var accessRuleService = new AccessRuleService();

        var hasAccess = await accessRuleService.ValidateAccessAsync(userId: 2, roomId: 1);

        Assert.IsTrue(hasAccess);
    }

    // TC-05: Modify an existing future booking to a new available time (FR6)
    // Checks that moving a booking to a different free slot works, and that the
    // booking doesn't get flagged as conflicting with its own original time slot.
    [TestMethod]
    public async Task TC05_ModifyFutureBooking_ToAvailableSlot_Succeeds()
    {
        var bookingDate = DateTime.Today.AddDays(2);
        var existingBooking = new Booking
        {
            Id = 5,
            RoomId = 1,
            UserId = 10,
            BookingDate = bookingDate,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0),
            Status = BookingStatus.Confirmed
        };
        _bookingRepoMock.Setup(r => r.GetByRoomIdAsync(1)).ReturnsAsync(new List<Booking> { existingBooking });

        // moving the booking to the afternoon instead
        var newStart = new TimeSpan(14, 0, 0);
        var newEnd = new TimeSpan(15, 0, 0);
        var (isValid, _) = await _bookingService.ValidateBookingAsync(
            roomId: 1, bookingDate: bookingDate, startTime: newStart, endTime: newEnd, bookingIdToExclude: existingBooking.Id);

        existingBooking.StartTime = newStart;
        existingBooking.EndTime = newEnd;
        var updated = await _bookingService.UpdateBookingAsync(existingBooking);

        Assert.IsTrue(isValid);
        Assert.AreEqual(newStart, updated.StartTime);
        Assert.AreEqual(newEnd, updated.EndTime);
        _bookingRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Booking>()), Times.Once);
    }

    // TC-06: Cancel a future booking (FR7)
    // Cancelling shouldn't delete the booking outright, it should just flip
    // the status to Cancelled so the room opens back up.
    [TestMethod]
    public async Task TC06_CancelFutureBooking_UpdatesStatusToCancelled()
    {
        var booking = new Booking
        {
            Id = 7,
            RoomId = 1,
            UserId = 10,
            BookingDate = DateTime.Today.AddDays(3),
            Status = BookingStatus.Confirmed
        };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(booking);

        await _bookingService.CancelBookingAsync(7);

        Assert.AreEqual(BookingStatus.Cancelled, booking.Status);
        _bookingRepoMock.Verify(r => r.UpdateAsync(It.Is<Booking>(b => b.Status == BookingStatus.Cancelled)), Times.Once);
    }

    // TC-07: Reject a booking request for a date/time in the past (FR3, edge case)
    // Basic sanity check - you shouldn't be able to book a room for yesterday.
    [TestMethod]
    public async Task TC07_PastDate_IsRejected()
    {
        var pastDate = DateTime.Today.AddDays(-1);
        var (isValid, errorMessage) = await _bookingService.ValidateBookingAsync(
            roomId: 1, bookingDate: pastDate, startTime: new TimeSpan(9, 0, 0), endTime: new TimeSpan(10, 0, 0));

        Assert.IsFalse(isValid);
        StringAssert.Contains(errorMessage.ToLower(), "past");
    }

    // TC-08: Reject a booking with a missing / invalid student ID (FR2, edge case)
    // This is what SHOULD happen if someone submits a booking with no student
    // attached to it. Skipped because right now nothing actually stops this.
    [TestMethod]
    [Ignore("BookingService.CreateBookingAsync doesn't check the UserId at all at the moment - it'll " +
            "just save the booking anyway (the only thing that happens is the confirmation email gets " +
            "skipped if the user lookup comes back null). Un-ignore once this validation gets added.")]
    public async Task TC08_MissingStudentId_IsRejected()
    {
        _userRepoMock.Setup(u => u.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);
        var bookingWithNoUser = new Booking
        {
            RoomId = 1,
            UserId = 0, // nobody attached to this booking
            BookingDate = DateTime.Today.AddDays(1),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0)
        };

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => _bookingService.CreateBookingAsync(bookingWithNoUser));
    }
}
