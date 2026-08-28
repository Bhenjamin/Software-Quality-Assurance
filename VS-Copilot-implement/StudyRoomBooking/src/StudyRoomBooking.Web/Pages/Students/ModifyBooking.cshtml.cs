using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Web.Pages.Students;

public class ModifyBookingModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;

    public BookingViewModel? Booking { get; set; }

    public ModifyBookingModel(IBookingService bookingService, IRoomService roomService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
    }

    public async Task OnGetAsync(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking != null)
        {
            var room = await _roomService.GetRoomByIdAsync(booking.RoomId);
            Booking = new BookingViewModel
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                RoomName = room?.Name ?? "Unknown",
                RoomCode = room?.Code ?? "",
                BookingDate = booking.BookingDate,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Status = booking.Status,
                Notes = booking.Notes,
                ConfirmationNumber = booking.ConfirmationNumber
            };
        }
    }

    public async Task<IActionResult> OnPostAsync(int bookingId, string bookingDate, string startTime, string endTime, string? notes)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                ModelState.AddModelError(string.Empty, "Booking not found.");
                await OnGetAsync(bookingId);
                return Page();
            }

            var date = DateTime.ParseExact(bookingDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            var start = TimeSpan.ParseExact(startTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            var end = TimeSpan.ParseExact(endTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);

            // Check availability (excluding current booking)
            var isAvailable = await _roomService.IsRoomAvailableAsync(booking.RoomId, date, start, end);
            if (!isAvailable)
            {
                ModelState.AddModelError(string.Empty, "Selected time slot is not available.");
                await OnGetAsync(bookingId);
                return Page();
            }

            booking.BookingDate = date;
            booking.StartTime = start;
            booking.EndTime = end;
            booking.Notes = notes;

            await _bookingService.UpdateBookingAsync(booking);

            return RedirectToPage("MyBookings", new { message = "Booking updated successfully!" });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error updating booking: {ex.Message}");
            await OnGetAsync(bookingId);
            return Page();
        }
    }
}
