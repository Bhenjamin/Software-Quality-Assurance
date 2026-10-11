using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Interfaces;
using StudyRoomBooking.Infrastructure.Persistence;
using StudyRoomBooking.Infrastructure.Repositories;

namespace StudyRoomBooking.Tests;

// Test cases TC-01 to TC-08 from our test plan.
// These test BookingService directly (and AccessRuleService for the eligibility ones)
// against an isolated relational database.
[TestClass]
public class BookingServiceTests
{
    private SqliteConnection _connection = null!;
    private StudyRoomBookingDbContext _dbContext = null!;
    private IUnitOfWork _unitOfWork = null!;
    private RecordingNotificationService _notificationService = null!;
    private BookingService _bookingService = null!;
    private RoomService _roomService = null!;

    // Each test gets a fresh database, isolated from application data and other tests.
    [TestInitialize]
    public async Task Setup()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<StudyRoomBookingDbContext>()
            .UseSqlite(_connection)
            .Options;
        _dbContext = new StudyRoomBookingDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        _unitOfWork = new EfUnitOfWork(_dbContext);
        _notificationService = new RecordingNotificationService();

        _bookingService = new BookingService(
            _unitOfWork,
            _notificationService
        );

        _roomService = new RoomService(_unitOfWork);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private async Task<User> AddUserAsync(int id = 10, string email = "student@uni.edu")
    {
        var user = new User
        {
            Id = id,
            UserId = $"test-user-{id}",
            Name = $"Test User {id}",
            Email = email,
            Role = UserRole.Student
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    private async Task<Room> AddRoomAsync(
        int id = 1,
        string name = "Study Room A",
        RoomType type = RoomType.Study
    )
    {
        var room = new Room
        {
            Id = id,
            Code = $"TEST-{id}",
            Name = name,
            Type = type,
            IsAvailable = true
        };
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();
        return room;
    }

    private async Task AddBookingAsync(Booking booking)
    {
        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync();
    }

    private sealed class RecordingNotificationService : INotificationService
    {
        public TaskCompletionSource<BookingConfirmation> ConfirmationSent { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public BookingCancellation? CancellationSent { get; private set; }

        public Task SendBookingConfirmationAsync(
            string email,
            string roomName,
            DateTime bookingDate,
            TimeSpan startTime,
            TimeSpan endTime,
            string confirmationNumber
        )
        {
            ConfirmationSent.TrySetResult(new BookingConfirmation(
                email,
                roomName,
                bookingDate,
                startTime,
                endTime,
                confirmationNumber
            ));
            return Task.CompletedTask;
        }

        public Task SendBookingCancellationAsync(string email, string roomName, DateTime bookingDate)
        {
            CancellationSent = new BookingCancellation(email, roomName, bookingDate);
            return Task.CompletedTask;
        }

        public Task SendBookingModificationAsync(string email, string roomName, DateTime oldDate, DateTime newDate)
        {
            return Task.CompletedTask;
        }
    }

    private sealed record BookingConfirmation(
        string Email,
        string RoomName,
        DateTime BookingDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string ConfirmationNumber
    );

    private sealed record BookingCancellation(string Email, string RoomName, DateTime BookingDate);

    private static async Task<BookingConfirmation> WaitForConfirmationAsync(
        RecordingNotificationService notificationService
    )
    {
        var completed = await Task.WhenAny(
            notificationService.ConfirmationSent.Task,
            Task.Delay(TimeSpan.FromSeconds(5))
        );
        Assert.AreSame(
            notificationService.ConfirmationSent.Task,
            completed,
            "Booking confirmation notification was not sent."
        );
        return await notificationService.ConfirmationSent.Task;
    }

    // TC-01: Book an available room successfully (FR2, FR3)
    // Just checks that if nobody else has the room at that time, the booking
    // actually goes through and comes back confirmed with a confirmation number.
    [TestMethod]
    public async Task TC01_BookAvailableRoom_Succeeds()
    {
        await AddUserAsync();
        await AddRoomAsync();

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
        var storedBooking = await _unitOfWork.Bookings.GetByIdAsync(result.Id);
        Assert.IsNotNull(storedBooking);
        Assert.AreEqual(BookingStatus.Confirmed, storedBooking.Status);
        var notification = await WaitForConfirmationAsync(_notificationService);
        Assert.AreEqual("student@uni.edu", notification.Email);
        Assert.AreEqual("Study Room A", notification.RoomName);
        Assert.AreEqual(bookingDate, notification.BookingDate);
        Assert.AreEqual(new TimeSpan(9, 0, 0), notification.StartTime);
        Assert.AreEqual(new TimeSpan(10, 0, 0), notification.EndTime);
        Assert.AreEqual(result.ConfirmationNumber, notification.ConfirmationNumber);
    }

    // TC-02: Reject a double-booking for the same room and time slot (FR3, NFR2)
    // Makes sure that if someone already has the room booked for an overlapping
    // time, a second person trying to book it gets blocked instead of it just
    // letting both bookings through.
    [TestMethod]
    public async Task TC02_DoubleBooking_IsRejected()
    {
        await AddUserAsync();
        await AddRoomAsync();
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
        existingBooking.UserId = 10;
        await AddBookingAsync(existingBooking);

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
        var designStudio = await AddRoomAsync(type: RoomType.DesignStudio, name: "Design Studio");
        _dbContext.RoomMajorRestrictions.Add(new RoomMajorRestriction
        {
            RoomId = designStudio.Id,
            Major = StudentMajor.Arts
        });
        await _dbContext.SaveChangesAsync();

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
        var designStudio = await AddRoomAsync(type: RoomType.DesignStudio, name: "Design Studio");
        _dbContext.RoomMajorRestrictions.Add(new RoomMajorRestriction
        {
            RoomId = designStudio.Id,
            Major = StudentMajor.Arts
        });
        await _dbContext.SaveChangesAsync();

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
        await AddUserAsync();
        await AddRoomAsync();
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
        await AddBookingAsync(existingBooking);

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
        var storedBooking = await _unitOfWork.Bookings.GetByIdAsync(existingBooking.Id);
        Assert.IsNotNull(storedBooking);
        Assert.AreEqual(newStart, storedBooking.StartTime);
        Assert.AreEqual(newEnd, storedBooking.EndTime);
    }

    // TC-06: Cancel a future booking (FR7)
    // Cancelling shouldn't delete the booking outright, it should just flip
    // the status to Cancelled so the room opens back up.
    [TestMethod]
    public async Task TC06_CancelFutureBooking_UpdatesStatusToCancelled()
    {
        await AddUserAsync();
        await AddRoomAsync();
        var booking = new Booking
        {
            Id = 7,
            RoomId = 1,
            UserId = 10,
            BookingDate = DateTime.Today.AddDays(3),
            Status = BookingStatus.Confirmed,
        };
        await AddBookingAsync(booking);

        // TC-06: Cancel future booking - owner cancellation with authorisation
        await _bookingService.CancelBookingAsync(7, requestingUserId: 10, isAdmin: false);

        var storedBooking = await _unitOfWork.Bookings.GetByIdAsync(7);
        Assert.IsNotNull(storedBooking);
        Assert.AreEqual(BookingStatus.Cancelled, storedBooking.Status);
        Assert.AreEqual(
            new BookingCancellation("student@uni.edu", "Study Room A", booking.BookingDate),
            _notificationService.CancellationSent
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
    // attached to it. 
    [TestMethod]
    public async Task TC08_MissingStudentId_IsRejected()
    {
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

    // TC-12: Booking history displays correct status for past and upcoming bookings (FR8)
    // Verifies that user bookings can be retrieved and contain correct status information
    // for past, present, and future dates.
    [TestMethod]
    public async Task TC12_BookingHistory_DisplaysCorrectStatus()
    {
        // Arrange
        var userId = 10;
        await AddUserAsync(userId);
        await AddRoomAsync(1);
        await AddRoomAsync(2, "Study Room B");
        await AddRoomAsync(3, "Study Room C");

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

        await AddBookingAsync(pastConfirmedBooking);
        await AddBookingAsync(cancelledBooking);
        await AddBookingAsync(upcomingBooking);

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
        var designStudio = await AddRoomAsync(
            name: "Design Studio",
            type: RoomType.DesignStudio
        );
        var roomId = designStudio.Id;
        _dbContext.RoomMajorRestrictions.Add(new RoomMajorRestriction
        {
            RoomId = roomId,
            Major = StudentMajor.Arts
        });
        await _dbContext.SaveChangesAsync();

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
        await AddUserAsync();
        await AddRoomAsync();
        var booking = new Booking
        {
            Id = 1,
            UserId = 10, // Owner is user 10
            RoomId = 1,
            BookingDate = DateTime.Today.AddDays(1),
            Status = BookingStatus.Confirmed
        };
        await AddBookingAsync(booking);

        // Act: User 20 (non-admin, different from owner) tries to cancel user 10's booking
        // This should throw UnauthorizedAccessException
        await _bookingService.CancelBookingAsync(1, 20, false);

        // Assert: Exception thrown (handled by [ExpectedException])
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
        for (int i = 1; i <= 10; i++)
        {
            await AddUserAsync(i);
        }

        // Create 60 rooms with multiple bookings each
        for (int i = 1; i <= 60; i++)
        {
            rooms.Add(new Room
            {
                Id = i,
                Code = $"TEST-{i}",
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

        _dbContext.Rooms.AddRange(rooms);
        _dbContext.Bookings.AddRange(allBookings);
        await _dbContext.SaveChangesAsync();

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
