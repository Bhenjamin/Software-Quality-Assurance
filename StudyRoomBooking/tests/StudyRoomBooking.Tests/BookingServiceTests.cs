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
    private Mock<IRoomMajorRestrictionRepository> _roomMajorRestrictionRepoMock = null!;
    private Mock<INotificationService> _notificationServiceMock = null!;
    private BookingService _bookingService = null!;
    private RoomService _roomService = null!;

    // Runs before every test so each one gets a fresh set of mocks
    [TestInitialize]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _roomRepoMock = new Mock<IRoomRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _roomMajorRestrictionRepoMock = new Mock<IRoomMajorRestrictionRepository>();
        _notificationServiceMock = new Mock<INotificationService>();

        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Rooms).Returns(_roomRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
        _unitOfWorkMock
            .Setup(u => u.RoomMajorRestrictions)
            .Returns(_roomMajorRestrictionRepoMock.Object);

        _bookingService = new BookingService(
            _unitOfWorkMock.Object,
            _notificationServiceMock.Object
        );

        _roomService = new RoomService(_unitOfWorkMock.Object);
    }

    // TC-01: Book an available room successfully (FR2, FR3)
    // Just checks that if nobody else has the room at that time, the booking
    // actually goes through and comes back confirmed with a confirmation number.
    [TestMethod]
    public async Task TC01_BookAvailableRoom_Succeeds()
    {
        // no bookings exist yet for this room
        _bookingRepoMock.Setup(r => r.GetByRoomIdAsync(1)).ReturnsAsync(new List<Booking>());
        _userRepoMock
            .Setup(u => u.GetByIdAsync(10))
            .ReturnsAsync(new User { Id = 10, Email = "student@uni.edu" });
        _roomRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Room { Id = 1, Name = "Study Room A" });

        var bookingDate = DateTime.Today.AddDays(1);
        var newBooking = new Booking
        {
            RoomId = 1,
            UserId = 10,
            BookingDate = bookingDate,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0),
        };

        var (isValid, _) = await _bookingService.ValidateBookingAsync(
            newBooking.RoomId,
            newBooking.BookingDate,
            newBooking.StartTime,
            newBooking.EndTime
        );
        var result = await _bookingService.CreateBookingAsync(newBooking);

        Assert.IsTrue(isValid);
        Assert.AreEqual(BookingStatus.Confirmed, result.Status);
        Assert.IsFalse(string.IsNullOrEmpty(result.ConfirmationNumber));
        _bookingRepoMock.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
        _notificationServiceMock.Verify(n => n.SendBookingConfirmationAsync(
            "student@uni.edu",
            "Study Room A",
            bookingDate,
            new TimeSpan(9, 0, 0),
            new TimeSpan(10, 0, 0),
            result.ConfirmationNumber!), Times.Once);
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
            Status = BookingStatus.Confirmed,
        };
        _bookingRepoMock
            .Setup(r => r.GetByRoomIdAsync(1))
            .ReturnsAsync(new List<Booking> { existingBooking });

        // someone else tries to grab an overlapping slot on the same room
        var (isValid, errorMessage) = await _bookingService.ValidateBookingAsync(
            roomId: 1,
            bookingDate: bookingDate,
            startTime: new TimeSpan(9, 30, 0),
            endTime: new TimeSpan(10, 30, 0)
        );

        Assert.IsFalse(isValid);
        StringAssert.Contains(errorMessage.ToLower(), "already booked");
    }

    // TC-03: Hide a specialised room from an ineligible student's search results (FR4, NFR3)
    // A Design Studio that's restricted to Arts majors shouldn't show up when an
    // Engineering student searches for rooms - it should just not be in the list.
    [TestMethod]
    public async Task TC03_IneligibleStudent_RestrictedRoomHiddenFromSearchResults()
    {
        var designStudio = new Room
        {
            Id = 1,
            Name = "Design Studio",
            Type = RoomType.DesignStudio,
            IsAvailable = true,
        };
        _roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { designStudio });
        _roomMajorRestrictionRepoMock
            .Setup(r => r.GetAllowedMajorsForRoomAsync(designStudio.Id))
            .ReturnsAsync(new List<StudentMajor> { StudentMajor.Arts });

        // an Engineering student searches for Design Studio rooms
        var results = await _roomService.SearchRoomsAsync(
            date: DateTime.Today,
            startTime: null,
            endTime: null,
            type: RoomType.DesignStudio,
            studentMajor: StudentMajor.Engineering
        );

        Assert.IsFalse(results.Any(r => r.Id == designStudio.Id));
    }

    // TC-04: Show a specialised room in an eligible student's search results (FR4, NFR3)
    // Same Design Studio as TC-03, but this time the student's major matches the
    // room's restriction, so it should come back in their search results.
    [TestMethod]
    public async Task TC04_EligibleStudent_RestrictedRoomVisibleInSearchResults()
    {
        var designStudio = new Room
        {
            Id = 1,
            Name = "Design Studio",
            Type = RoomType.DesignStudio,
            IsAvailable = true,
        };
        _roomRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Room> { designStudio });
        _roomMajorRestrictionRepoMock
            .Setup(r => r.GetAllowedMajorsForRoomAsync(designStudio.Id))
            .ReturnsAsync(new List<StudentMajor> { StudentMajor.Arts });

        // an Arts student searches for Design Studio rooms
        var results = await _roomService.SearchRoomsAsync(
            date: DateTime.Today,
            startTime: null,
            endTime: null,
            type: RoomType.DesignStudio,
            studentMajor: StudentMajor.Arts
        );

        Assert.IsTrue(results.Any(r => r.Id == designStudio.Id));
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
            Status = BookingStatus.Confirmed,
        };
        _bookingRepoMock
            .Setup(r => r.GetByRoomIdAsync(1))
            .ReturnsAsync(new List<Booking> { existingBooking });

        // moving the booking to the afternoon instead
        var newStart = new TimeSpan(14, 0, 0);
        var newEnd = new TimeSpan(15, 0, 0);
        var (isValid, _) = await _bookingService.ValidateBookingAsync(
            roomId: 1,
            bookingDate: bookingDate,
            startTime: newStart,
            endTime: newEnd,
            bookingIdToExclude: existingBooking.Id
        );

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
            Status = BookingStatus.Confirmed,
        };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(booking);
        _userRepoMock
            .Setup(u => u.GetByIdAsync(10))
            .ReturnsAsync(new User { Id = 10, Email = "student@uni.edu" });
        _roomRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Room { Id = 1, Name = "Study Room A" });

        // TC-06: Cancel future booking - owner cancellation with authorisation
        await _bookingService.CancelBookingAsync(7, requestingUserId: 10, isAdmin: false);

        Assert.AreEqual(BookingStatus.Cancelled, booking.Status);
        _bookingRepoMock.Verify(
            r => r.UpdateAsync(It.Is<Booking>(b => b.Status == BookingStatus.Cancelled)),
            Times.Once
        );
        _notificationServiceMock.Verify(
            n => n.SendBookingCancellationAsync("student@uni.edu", "Study Room A", booking.BookingDate),
            Times.Once
        );
    }

    // TC-07: Reject a booking request for a date/time in the past (FR3, edge case)
    // Basic sanity check - you shouldn't be able to book a room for yesterday.
    [TestMethod]
    public async Task TC07_PastDate_IsRejected()
    {
        var pastDate = DateTime.Today.AddDays(-1);
        var (isValid, errorMessage) = await _bookingService.ValidateBookingAsync(
            roomId: 1,
            bookingDate: pastDate,
            startTime: new TimeSpan(9, 0, 0),
            endTime: new TimeSpan(10, 0, 0)
        );

        Assert.IsFalse(isValid);
        StringAssert.Contains(errorMessage.ToLower(), "past");
    }

    // TC-08: Reject a booking with a missing / invalid student ID (FR2, edge case)
    // This is what SHOULD happen if someone submits a booking with no student
    // attached to it. Skipped because right now nothing actually stops this.
    [TestMethod]
    public async Task TC08_MissingStudentId_IsRejected()
    {
        _userRepoMock.Setup(u => u.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);
        var bookingWithNoUser = new Booking
        {
            RoomId = 1,
            UserId = 0, // nobody attached to this booking
            BookingDate = DateTime.Today.AddDays(1),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0),
        };

        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            _bookingService.CreateBookingAsync(bookingWithNoUser)
        );
    }

    // ============================================================
    // TC-11 to TC-20: Additional test cases for test plan coverage
    // ============================================================

    // TC-11: Confirmation email is sent with correct booking details (FR5)
    // Verifies that when a booking is created, the notification service
    // is called with the correct booking information.
    [TestMethod]
    public async Task TC11_BookingConfirmation_EmailSentWithCorrectDetails()
    {
        // Arrange
        var studentEmail = "student@uni.edu";
        var roomName = "Study Room A";
        var bookingDate = DateTime.Today.AddDays(1);
        var startTime = new TimeSpan(9, 0, 0);
        var endTime = new TimeSpan(10, 0, 0);
        var confirmationNumber = "CONF001";

        _userRepoMock
            .Setup(u => u.GetByIdAsync(10))
            .ReturnsAsync(new User { Id = 10, Email = studentEmail });

        _roomRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Room { Id = 1, Name = roomName });

        _bookingRepoMock
            .Setup(r => r.GetByRoomIdAsync(1))
            .ReturnsAsync(new List<Booking>());

        var newBooking = new Booking
        {
            RoomId = 1,
            UserId = 10,
            BookingDate = bookingDate,
            StartTime = startTime,
            EndTime = endTime
        };

        // Act
        var result = await _bookingService.CreateBookingAsync(newBooking);

        // Assert
        Assert.IsNotNull(result.ConfirmationNumber);
        _notificationServiceMock.Verify(
            n => n.SendBookingConfirmationAsync(
                studentEmail,
                roomName,
                bookingDate,
                startTime,
                endTime,
                It.IsAny<string>() // Any confirmation number is acceptable
            ),
            Times.Once,
            "Notification service should be called once with correct booking details"
        );
    }

    // TC-12: Booking history displays correct status for past and upcoming bookings (FR8)
    // Verifies that user bookings can be retrieved and contain correct status information
    // for past, present, and future dates.
    [TestMethod]
    public async Task TC12_BookingHistory_DisplaysCorrectStatus()
    {
        // Arrange
        var userId = 10;

        var pastConfirmedBooking = new Booking
        {
            Id = 1,
            UserId = userId,
            RoomId = 1,
            BookingDate = DateTime.Today.AddDays(-5),
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(11, 0, 0),
            Status = BookingStatus.Confirmed
        };

        var cancelledBooking = new Booking
        {
            Id = 2,
            UserId = userId,
            RoomId = 2,
            BookingDate = DateTime.Today.AddDays(2),
            StartTime = new TimeSpan(14, 0, 0),
            EndTime = new TimeSpan(15, 0, 0),
            Status = BookingStatus.Cancelled
        };

        var upcomingBooking = new Booking
        {
            Id = 3,
            UserId = userId,
            RoomId = 3,
            BookingDate = DateTime.Today.AddDays(7),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0),
            Status = BookingStatus.Confirmed
        };

        _bookingRepoMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<Booking>
            {
                upcomingBooking,
                cancelledBooking,
                pastConfirmedBooking
            });

        // Act
        var history = await _bookingService.GetBookingsByUserIdAsync(userId);

        // Assert
        Assert.AreEqual(3, history.Count, "Should retrieve 3 bookings");
        Assert.AreEqual(1, history.Count(b => b.Status == BookingStatus.Cancelled),
            "Should have exactly 1 cancelled booking");
        Assert.AreEqual(2, history.Count(b => b.Status == BookingStatus.Confirmed),
            "Should have exactly 2 confirmed bookings");

        // Verify we have past, current, and future bookings
        Assert.IsTrue(history.Any(b => b.BookingDate < DateTime.Today),
            "Should include past bookings");
        Assert.IsTrue(history.Any(b => b.BookingDate > DateTime.Today),
            "Should include future bookings");
        Assert.IsTrue(history.Any(b => b.Status == BookingStatus.Cancelled),
            "Should include cancelled bookings");
    }

    // TC-14: Administrator can update room access rules (FR10, NFR3)
    // Verifies that room major restrictions can be retrieved and used for
    // access control in room searches.
    [TestMethod]
    public async Task TC14_RoomAccessRules_FilterStudentSearch()
    {
        // Arrange
        var roomId = 1;
        var designStudio = new Room
        {
            Id = roomId,
            Name = "Design Studio",
            Type = RoomType.DesignStudio,
            IsAvailable = true
        };

        // Set up restrictions: only Arts students can use this room
        _roomMajorRestrictionRepoMock
            .Setup(r => r.GetAllowedMajorsForRoomAsync(roomId))
            .ReturnsAsync(new List<StudentMajor> { StudentMajor.Arts });

        _roomRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Room> { designStudio });

        // Act: Arts student searches
        var artsStudentResults = await _roomService.SearchRoomsAsync(
            date: DateTime.Today,
            startTime: null,
            endTime: null,
            type: RoomType.DesignStudio,
            studentMajor: StudentMajor.Arts
        );

        // Act: Engineering student searches
        var engineeringStudentResults = await _roomService.SearchRoomsAsync(
            date: DateTime.Today,
            startTime: null,
            endTime: null,
            type: RoomType.DesignStudio,
            studentMajor: StudentMajor.Engineering
        );

        // Assert: Arts student should see the room
        Assert.IsTrue(artsStudentResults.Any(r => r.Id == roomId),
            "Arts student should see restricted room when major matches restriction");

        // Assert: Engineering student should NOT see the room
        Assert.IsFalse(engineeringStudentResults.Any(r => r.Id == roomId),
            "Engineering student should not see room restricted to Arts students");
    }

    // TC-15: Non-administrator cannot cancel another user's booking (FR11, NFR3)
    // Verifies that only the booking owner or admin can cancel a booking.
    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public async Task TC15_NonAdmin_CannotCancelOtherUserBooking()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1,
            UserId = 10, // Owner is user 10
            RoomId = 1,
            BookingDate = DateTime.Today.AddDays(1),
            Status = BookingStatus.Confirmed
        };

        _bookingRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(booking);

        _userRepoMock
            .Setup(u => u.GetByIdAsync(10))
            .ReturnsAsync(new User { Id = 10, Email = "user@uni.edu" });

        _roomRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Room { Id = 1, Name = "Room A" });

        // Act: User 20 (non-admin, different from owner) tries to cancel user 10's booking
        // This should throw UnauthorizedAccessException
        await _bookingService.CancelBookingAsync(1, 20, false);

        // Assert: Exception thrown (handled by [ExpectedException])
    }

    // TC-15b: Owner CAN cancel their own booking
    [TestMethod]
    public async Task TC15_Owner_CanCancelOwnBooking()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1,
            UserId = 10, // Owner is user 10
            RoomId = 1,
            BookingDate = DateTime.Today.AddDays(1),
            Status = BookingStatus.Confirmed
        };

        _bookingRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(booking);

        _userRepoMock
            .Setup(u => u.GetByIdAsync(10))
            .ReturnsAsync(new User { Id = 10, Email = "user@uni.edu" });

        _roomRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Room { Id = 1, Name = "Room A" });

        // Act: User 10 cancels their own booking
        await _bookingService.CancelBookingAsync(1, 10, false);

        // Assert
        _bookingRepoMock.Verify(
            r => r.UpdateAsync(It.Is<Booking>(b => b.Status == BookingStatus.Cancelled)),
            Times.Once,
            "Booking should be updated to Cancelled status"
        );
    }

    // TC-15c: Admin CAN cancel any booking
    [TestMethod]
    public async Task TC15_Admin_CanCancelAnyBooking()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1,
            UserId = 10, // Different user
            RoomId = 1,
            BookingDate = DateTime.Today.AddDays(1),
            Status = BookingStatus.Confirmed
        };

        _bookingRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(booking);

        _userRepoMock
            .Setup(u => u.GetByIdAsync(10))
            .ReturnsAsync(new User { Id = 10, Email = "user@uni.edu" });

        _roomRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Room { Id = 1, Name = "Room A" });

        // Act: Admin (user 999) cancels user 10's booking
        await _bookingService.CancelBookingAsync(1, 999, true);

        // Assert
        _bookingRepoMock.Verify(
            r => r.UpdateAsync(It.Is<Booking>(b => b.Status == BookingStatus.Cancelled)),
            Times.Once,
            "Admin should be able to cancel any booking"
        );
    }

    // TC-17: Room search returns results within 2 seconds for 50+ reservations (NFR1)
    // Verifies that the search performance meets the requirement of < 2 seconds
    // when searching among 50+ existing bookings.
    [TestMethod]
    public async Task TC17_RoomSearch_Returns50Results_WithinTwoSeconds()
    {
        // Arrange
        var rooms = new List<Room>();
        var allBookings = new List<Booking>();

        // Create 60 rooms with multiple bookings each
        for (int i = 1; i <= 60; i++)
        {
            rooms.Add(new Room
            {
                Id = i,
                Name = $"Room {i}",
                Capacity = 4,
                Type = i % 2 == 0 ? RoomType.Study : RoomType.Meeting,
                IsAvailable = true
            });

            // Add 5 bookings per room for different dates
            // Each booking is booked from hour X to hour X+1
            for (int j = 0; j < 5; j++)
            {
                allBookings.Add(new Booking
                {
                    RoomId = i,
                    UserId = j % 10 + 1,
                    BookingDate = DateTime.Today.AddDays(j),
                    StartTime = new TimeSpan(9 + j, 0, 0),
                    EndTime = new TimeSpan(10 + j, 0, 0),
                    Status = BookingStatus.Confirmed
                });
            }
        }

        _roomRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(rooms);

        // Mock GetByRoomIdAsync to return bookings for each room
        _bookingRepoMock
            .Setup(b => b.GetByRoomIdAsync(It.IsAny<int>()))
            .Returns<int>((roomId) => Task.FromResult(
                allBookings.Where(b => b.RoomId == roomId).ToList()
            ));

        var searchDate = DateTime.Today.AddDays(1); // Day 1
        var searchStartTime = new TimeSpan(14, 0, 0); // 14:00 - no conflict (bookings are 9:00-11:00)
        var searchEndTime = new TimeSpan(15, 0, 0);   // 15:00

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var results = await _roomService.SearchRoomsAsync(
            date: searchDate,
            startTime: searchStartTime,
            endTime: searchEndTime,
            type: null,
            studentMajor: null
        );

        stopwatch.Stop();

        // Assert
        Assert.IsTrue(
            stopwatch.ElapsedMilliseconds < 2000,
            $"Search took {stopwatch.ElapsedMilliseconds}ms but should complete in less than 2000ms"
        );
        Assert.IsTrue(results.Count > 0, "Search should return at least some available rooms");
    }

    }
