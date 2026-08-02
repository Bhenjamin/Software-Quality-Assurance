using StudyRoomBooking.Application.DTOs;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Infrastructure.Repositories;
using StudyRoomBooking.Infrastructure.SeedData;

// Manual composition root — no DI container needed for a prototype this size.
IRoomRepository roomRepository = new InMemoryRoomRepository();
IUserRepository userRepository = new InMemoryUserRepository();
IBookingRepository bookingRepository = new InMemoryBookingRepository();
SampleDataSeeder.Seed(roomRepository, userRepository);

IAccessControlService accessControlService = new AccessControlService();
IRoomSearchService roomSearchService = new RoomSearchService(roomRepository, bookingRepository);
IBookingService bookingService = new BookingService(roomRepository, userRepository, bookingRepository, accessControlService);

var student = userRepository.GetAll().First(u => u.Role == UserRole.Student && u.Programme == "Computer Science");
var otherStudent = userRepository.GetAll().First(u => u.Role == UserRole.Student && u.Programme == "Graphic Design");
var admin = userRepository.GetAll().First(u => u.Role == UserRole.Administrator);

var searchStart = DateTime.UtcNow.AddHours(2);
var searchEnd = searchStart.AddHours(1);

Console.WriteLine("== 1. Room search ==");
var available = roomSearchService.SearchAvailableRooms(new RoomSearchCriteria
{
    StartTime = searchStart,
    EndTime = searchEnd,
    MinCapacity = 4
});
foreach (var room in available)
{
    Console.WriteLine($"  Available: {room}");
}

Console.WriteLine();
Console.WriteLine("== 2. Booking creation ==");
var studyPod = available.First(r => r.Type == RoomType.StudyPod);
var bookingResult = bookingService.CreateBooking(new BookingRequest
{
    RoomId = studyPod.Id,
    UserId = student.Id,
    StartTime = searchStart,
    EndTime = searchEnd,
    Purpose = "Group project discussion"
});
Console.WriteLine($"  {(bookingResult.Success ? "Confirmed" : "Failed")}: {studyPod.Name} for {student.FullName}");

Console.WriteLine();
Console.WriteLine("== 3. Double-booking prevention ==");
var conflictResult = bookingService.CreateBooking(new BookingRequest
{
    RoomId = studyPod.Id,
    UserId = otherStudent.Id,
    StartTime = searchStart.AddMinutes(15),
    EndTime = searchEnd.AddMinutes(15),
    Purpose = "Different group, overlapping time"
});
Console.WriteLine($"  Result: {(conflictResult.Success ? "Unexpectedly succeeded" : $"Correctly rejected — {conflictResult.ErrorCode}: {conflictResult.ErrorMessage}")}");

Console.WriteLine();
Console.WriteLine("== 4. Restricted-room access control ==");
var lab = roomRepository.GetAll().First(r => r.Type == RoomType.Laboratory);
var deniedResult = bookingService.CreateBooking(new BookingRequest
{
    RoomId = lab.Id,
    UserId = otherStudent.Id, // Graphic Design student, not on the lab's allowed programme list
    StartTime = searchStart.AddHours(3),
    EndTime = searchEnd.AddHours(3),
    Purpose = "Not permitted"
});
Console.WriteLine($"  Result: {(deniedResult.Success ? "Unexpectedly succeeded" : $"Correctly denied — {deniedResult.ErrorCode}: {deniedResult.ErrorMessage}")}");

Console.WriteLine();
Console.WriteLine("== 5. Booking modification ==");
var modifyResult = bookingService.ModifyBooking(new BookingModificationRequest
{
    BookingId = bookingResult.Booking!.Id,
    RequestingUserId = student.Id,
    NewStartTime = searchStart.AddHours(1),
    NewEndTime = searchEnd.AddHours(1)
});
Console.WriteLine($"  {(modifyResult.Success ? "Modified" : $"Failed — {modifyResult.ErrorMessage}")}");

Console.WriteLine();
Console.WriteLine("== 6. Booking cancellation ==");
var cancelResult = bookingService.CancelBooking(bookingResult.Booking.Id, student.Id, "Plans changed");
Console.WriteLine($"  {(cancelResult.Success ? "Cancelled" : $"Failed — {cancelResult.ErrorMessage}")}");

Console.WriteLine();
Console.WriteLine("== 7. Administrator override ==");
var adminOverride = bookingService.CreateBooking(new BookingRequest
{
    RoomId = lab.Id,
    UserId = admin.Id,
    StartTime = searchStart.AddHours(3),
    EndTime = searchEnd.AddHours(3),
    Purpose = "Admin override booking",
    OverrideConflict = true
});
Console.WriteLine($"  {(adminOverride.Success ? "Admin booking confirmed (access control bypassed correctly)" : $"Failed — {adminOverride.ErrorMessage}")}");
