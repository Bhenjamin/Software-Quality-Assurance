# Cancel Booking - Code Changes Reference

## File 1: Cancel.cshtml.cs (Page Model)

### Location
`VS-Copilot-implement/StudyRoomBooking/src/StudyRoomBooking.Web/Pages/Bookings/Cancel.cshtml.cs`

### Complete Rewritten Content
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Bookings;

public class CancelModel : PageModel
{
	private readonly IBookingService _bookingService;
	private readonly IRoomService _roomService;

	[BindProperty]
	public BookingViewModel? Booking { get; set; }

	[BindProperty]
	public RoomViewModel? Room { get; set; }

	public CancelModel(IBookingService bookingService, IRoomService roomService)
	{
		_bookingService = bookingService;
		_roomService = roomService;
	}

	public IActionResult OnGet(int? id)
	{
		if (!IsAuthenticated())
			return RedirectToPage("/Auth/Login");

		if (!id.HasValue)
			return NotFound();

		var userId = GetCurrentUserId();
		var booking = _bookingService.GetBookingById(id.Value);

		if (booking == null)
		{
			TempData["ErrorMessage"] = "Booking not found.";
			return RedirectToPage("Index");
		}

		// Authorization: user can only cancel their own bookings
		if (booking.UserId != userId)
		{
			TempData["ErrorMessage"] = "You cannot cancel another user's booking.";
			return RedirectToPage("Index");
		}

		// Check if booking can be cancelled
		if (!CanCancelBooking(booking))
		{
			TempData["ErrorMessage"] = "This booking cannot be cancelled. It may be in the past or already cancelled.";
			return RedirectToPage("Index");
		}

		Booking = booking;
		Room = _roomService.GetRoomById(booking.RoomId);

		return Page();
	}

	public IActionResult OnPost(int? id)
	{
		if (!IsAuthenticated())
			return RedirectToPage("/Auth/Login");

		if (!id.HasValue)
			return NotFound();

		var userId = GetCurrentUserId();
		var booking = _bookingService.GetBookingById(id.Value);

		if (booking == null)
		{
			TempData["ErrorMessage"] = "Booking not found.";
			return RedirectToPage("Index");
		}

		// Authorization: user can only cancel their own bookings
		if (booking.UserId != userId)
		{
			TempData["ErrorMessage"] = "You cannot cancel another user's booking.";
			return RedirectToPage("Index");
		}

		// Check if booking can be cancelled
		if (!CanCancelBooking(booking))
		{
			TempData["ErrorMessage"] = "This booking cannot be cancelled.";
			return RedirectToPage("Index");
		}

		try
		{
			_bookingService.CancelBooking(id.Value);
			TempData["SuccessMessage"] = "Your booking has been cancelled successfully.";
			return RedirectToPage("Index");
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = $"Error cancelling booking: {ex.Message}";
			return Page();
		}
	}

	private int GetCurrentUserId()
	{
		return int.Parse(HttpContext.Session.GetString(AppConstants.UserSessionKey) ?? "0");
	}

	private bool IsAuthenticated()
	{
		var userId = HttpContext.Session.GetString(AppConstants.UserSessionKey);
		return !string.IsNullOrEmpty(userId);
	}

	private bool CanCancelBooking(BookingViewModel booking)
	{
		// Cannot cancel bookings that are already cancelled
		if (booking.Status.ToString() == "Cancelled")
			return false;

		// Cannot cancel bookings in the past (same day or earlier)
		if (booking.BookingDate < DateTime.Now.Date)
			return false;

		// Cannot cancel bookings on the same day
		// Optional: You can remove this if you want to allow same-day cancellations
		// if (booking.BookingDate == DateTime.Now.Date)
		//     return false;

		return true;
	}
}
```

---

## File 2: Cancel.cshtml (View)

### Location
`VS-Copilot-implement/StudyRoomBooking/src/StudyRoomBooking.Web/Pages/Bookings/Cancel.cshtml`

### Complete Rewritten Content
```razor
@page "{id:int}"
@model StudyRoomBooking.Web.Pages.Bookings.CancelModel
@{
	ViewData["Title"] = "Cancel Booking";
}

<div class="container mt-4">
	<div class="row">
		<div class="col-md-6 offset-md-3">
			<h1 class="mb-4">Cancel Booking</h1>

			@if (!string.IsNullOrEmpty(TempData["SuccessMessage"]?.ToString()))
			{
				<div class="alert alert-success alert-dismissible fade show" role="alert">
					@TempData["SuccessMessage"]
					<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
				</div>
			}

			@if (!string.IsNullOrEmpty(TempData["ErrorMessage"]?.ToString()))
			{
				<div class="alert alert-danger alert-dismissible fade show" role="alert">
					@TempData["ErrorMessage"]
					<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
				</div>
			}

			@if (Model.Room is not null && Model.Booking is not null)
			{
				<div class="alert alert-warning" role="alert">
					<strong>Confirm Cancellation</strong>
					<p class="mb-0 mt-2">Are you sure you want to cancel your booking?</p>
				</div>

				<div class="card mb-4">
					<div class="card-body">
						<dl class="row">
							<dt class="col-sm-3">Room:</dt>
							<dd class="col-sm-9">
								<strong>@Model.Room.RoomName</strong>
							</dd>

							<dt class="col-sm-3">Date:</dt>
							<dd class="col-sm-9">
								@Model.Booking.BookingDate.ToString("dddd, MMMM dd, yyyy")
							</dd>

							<dt class="col-sm-3">Time:</dt>
							<dd class="col-sm-9">
								@Model.Booking.StartTime.ToString("hh\\:mm") to @Model.Booking.EndTime.ToString("hh\\:mm")
							</dd>

							<dt class="col-sm-3">Capacity:</dt>
							<dd class="col-sm-9">
								@Model.Room.Capacity person(s)
							</dd>

							@if (!string.IsNullOrEmpty(Model.Booking.Notes))
							{
								<dt class="col-sm-3">Notes:</dt>
								<dd class="col-sm-9">
									@Model.Booking.Notes
								</dd>
							}
						</dl>
					</div>
				</div>

				<form method="post">
					<button type="submit" class="btn btn-danger">
						<i class="bi bi-trash"></i> Yes, Cancel This Booking
					</button>
					<a href="/Bookings/Index" class="btn btn-secondary">
						<i class="bi bi-arrow-left"></i> No, Keep It
					</a>
				</form>
			}
			else
			{
				<div class="alert alert-danger">
					Unable to load booking details. Please try again.
				</div>
				<a href="/Bookings/Index" class="btn btn-secondary">Back to Bookings</a>
			}
		</div>
	</div>
</div>
```

---

## File 3: BookingService.cs (Business Logic)

### Location
`VS-Copilot-implement/StudyRoomBooking/src/StudyRoomBooking.Application/Services/BookingService.cs`

### Changed Methods

#### Before (Original):
```csharp
public void CancelBooking(int bookingId)
{
	var booking = _dataStore.Bookings.FirstOrDefault(b => b.Id == bookingId);
	if (booking == null)
		return;

	booking.Status = BookingStatus.Cancelled;
	booking.CancelledAt = DateTime.UtcNow;
	booking.UpdatedAt = DateTime.UtcNow;

	var user = _dataStore.Users.FirstOrDefault(u => u.Id == booking.UserId);
	if (user != null)
	{
		var bookingVm = MapToViewModel(booking);
		_notificationService.SendBookingCancellationNotification(bookingVm, user.Email);
	}
}
```

#### After (Enhanced):
```csharp
public void CancelBooking(int bookingId)
{
	var booking = _dataStore.Bookings.FirstOrDefault(b => b.Id == bookingId);
	if (booking == null)
		throw new InvalidOperationException($"Booking with ID {bookingId} not found.");

	if (booking.Status == BookingStatus.Cancelled)
		throw new InvalidOperationException("This booking has already been cancelled.");

	if (booking.BookingDate < DateTime.Now.Date)
		throw new InvalidOperationException("Cannot cancel bookings from the past.");

	booking.Status = BookingStatus.Cancelled;
	booking.CancelledAt = DateTime.UtcNow;
	booking.UpdatedAt = DateTime.UtcNow;

	var user = _dataStore.Users.FirstOrDefault(u => u.Id == booking.UserId);
	if (user != null)
	{
		var bookingVm = MapToViewModel(booking);
		_notificationService.SendBookingCancellationNotification(bookingVm, user.Email);
	}
}

public bool CanCancelBooking(int bookingId)
{
	var booking = _dataStore.Bookings.FirstOrDefault(b => b.Id == bookingId);
	if (booking == null)
		return false;

	if (booking.Status == BookingStatus.Cancelled)
		return false;

	if (booking.BookingDate < DateTime.Now.Date)
		return false;

	return true;
}
```

---

## File 4: IBookingService.cs (Interface)

### Location
`VS-Copilot-implement/StudyRoomBooking/src/StudyRoomBooking.Application/Interfaces/IBookingService.cs`

### Updated Interface
```csharp
using StudyRoomBooking.Application.ViewModels;

namespace StudyRoomBooking.Application.Interfaces;

public interface IBookingService
{
	List<BookingViewModel> GetAllBookings();
	List<BookingViewModel> GetUserBookings(int userId);
	BookingViewModel? GetBookingById(int id);
	void CreateBooking(BookingViewModel booking);
	void UpdateBooking(BookingViewModel booking);
	void CancelBooking(int bookingId);
	bool CanCancelBooking(int bookingId);  // NEW METHOD
	List<BookingViewModel> GetBookingHistory(int userId);
	bool IsRoomAvailable(int roomId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeBookingId = null);
}
```

---

## Summary of Changes

| File | Type | Changes |
|------|------|---------|
| Cancel.cshtml.cs | Rewrite | Complete rewrite with proper auth, validation, error handling |
| Cancel.cshtml | Rewrite | Recreated with proper Razor bindings, Bootstrap UI, alerts |
| BookingService.cs | Enhancement | Enhanced CancelBooking with validation, added CanCancelBooking |
| IBookingService.cs | Update | Added CanCancelBooking method signature |

## Key Improvements

1. **Security**: Authorization checks prevent users from cancelling other users' bookings
2. **Validation**: Business logic validation prevents cancelling past or already-cancelled bookings
3. **Error Handling**: Proper exception handling with user-friendly error messages
4. **UI/UX**: Professional UI with Bootstrap, displays all booking details, clear action buttons
5. **User Feedback**: TempData-based success and error messages
6. **Code Quality**: Proper separation of concerns, single responsibility principle
