using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Web.Pages.Admin;

public class OverridesModel : AdminPageModel
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly IUserService _userService;
    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;

    public List<BookingViewModel> AvailableBookings { get; set; } = new();
    public List<BookingOverride> Overrides { get; set; } = new();

    public OverridesModel(IBookingService bookingService, IRoomService roomService, IUserService userService, Domain.Interfaces.IUnitOfWork unitOfWork)
    {
        _bookingService = bookingService;
        _roomService = roomService;
        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    public async Task OnGetAsync()
    {
        await LoadBookings();
        Overrides = await _unitOfWork.BookingOverrides.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync(int bookingId, string reason, bool allowException)
    {
        try
        {
            var admin = await _userService.GetUserByUserIdAsync("ADM001");

            var @override = new BookingOverride
            {
                BookingId = bookingId,
                AdminId = admin?.Id ?? 1,
                Reason = reason,
                AllowsExceptionToRule = allowException
            };

            if (bookingId <= 0 || string.IsNullOrWhiteSpace(reason) || await _bookingService.GetBookingByIdAsync(bookingId) is null)
            {
                ModelState.AddModelError(string.Empty, "Select a valid booking and provide a reason.");
                await LoadBookings();
                Overrides = await _unitOfWork.BookingOverrides.GetAllAsync();
                return Page();
            }

            await _unitOfWork.BookingOverrides.AddAsync(@override);
            await _unitOfWork.SaveChangesAsync();

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error creating override: {ex.Message}");
            await LoadBookings();
            Overrides = await _unitOfWork.BookingOverrides.GetAllAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _unitOfWork.BookingOverrides.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
        return RedirectToPage();
    }

    private async Task LoadBookings()
    {
        try
        {
            var bookings = await _bookingService.GetAllBookingsAsync();

            foreach (var booking in bookings)
            {
                var room = await _roomService.GetRoomByIdAsync(booking.RoomId);
                var user = await _userService.GetUserByIdAsync(booking.UserId);

                AvailableBookings.Add(new BookingViewModel
                {
                    Id = booking.Id,
                    RoomId = booking.RoomId,
                    RoomName = room?.Name ?? "Unknown",
                    UserId = booking.UserId,
                    UserName = user?.Name ?? "Unknown",
                    BookingDate = booking.BookingDate,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                    Status = booking.Status
                });
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error loading bookings: {ex.Message}");
        }
    }
}
